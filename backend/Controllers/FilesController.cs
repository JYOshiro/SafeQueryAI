using Microsoft.AspNetCore.Mvc;
using SafeQueryAI.Api.Contracts;
using SafeQueryAI.Api.Models;
using SafeQueryAI.Api.Services.Interfaces;

namespace SafeQueryAI.Api.Controllers;

[ApiController]
[Route("api/sessions/{sessionId}/files")]
public class FilesController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly IFileStorageService _fileStorage;
    private readonly ITextExtractionService _textExtraction;
    private readonly IDocumentIndexingService _indexing;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FilesController> _logger;

    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".csv" };

    public FilesController(
        ISessionService sessionService,
        IFileStorageService fileStorage,
        ITextExtractionService textExtraction,
        IDocumentIndexingService indexing,
        IConfiguration configuration,
        ILogger<FilesController> logger)
    {
        _sessionService = sessionService;
        _fileStorage = fileStorage;
        _textExtraction = textExtraction;
        _indexing = indexing;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>Returns the list of files uploaded in the session (no content, metadata only).</summary>
    [HttpGet]
    public IActionResult GetFiles(string sessionId)
    {
        var session = _sessionService.GetSession(sessionId);
        if (session is null)
            return NotFound(new ErrorResponse("Session not found"));

        var items = session.Files.Select(f => new SessionFileItem(
            f.FileId, f.OriginalFileName, f.FileType, f.FileSizeBytes, f.UploadedAt)).ToList();

        return Ok(new SessionFilesResponse(sessionId, items));
    }

    /// <summary>
    /// Uploads a PDF or CSV file to the session.
    /// The file is saved temporarily and text is extracted immediately.
    /// </summary>
    [HttpPost]
    [RequestSizeLimit(25 * 1024 * 1024)] // 25 MB absolute ceiling
    public async Task<IActionResult> UploadFile(string sessionId, IFormFile file)
    {
        var session = _sessionService.GetSession(sessionId);
        if (session is null)
            return NotFound(new ErrorResponse("Session not found"));

        if (file is null || file.Length == 0)
            return BadRequest(new ErrorResponse("No file provided"));

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return BadRequest(new ErrorResponse("Unsupported file type", "Only PDF and CSV files are supported."));

        var maxMb = _configuration.GetValue<int>("SafeQueryAI:MaxFileSizeMb", 20);
        if (file.Length > maxMb * 1024 * 1024)
            return BadRequest(new ErrorResponse("File too large", $"Maximum allowed size is {maxMb} MB."));

        var fileId = Guid.NewGuid().ToString("N");
        var localPath = await _fileStorage.SaveFileAsync(sessionId, fileId, file);

        // Extract text immediately after saving
        var extractedText = extension == ".pdf"
            ? _textExtraction.ExtractFromPdf(localPath)
            : _textExtraction.ExtractFromCsv(localPath);

        var storedFile = new StoredFileInfo
        {
            FileId = fileId,
            OriginalFileName = file.FileName,
            FileType = extension.TrimStart('.'),
            FileSizeBytes = file.Length,
            UploadedAt = DateTime.UtcNow,
            LocalPath = localPath,
            ExtractedText = extractedText  // kept server-side only
        };

        _sessionService.AddFileToSession(sessionId, storedFile);

        _logger.LogInformation(
            "File {FileName} uploaded to session {SessionId}. Extracted {CharCount} characters.",
            file.FileName, sessionId, extractedText.Length);

        // Index the file for RAG. If Ollama is unavailable the upload still succeeds;
        // question answering will fall back to keyword matching for this session.
        try
        {
            await _indexing.IndexFileAsync(sessionId, fileId, file.FileName, extractedText);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(ex,
                "Ollama unavailable — file {FileName} uploaded but not indexed for RAG.",
                file.FileName);
        }

        return Ok(new FileUploadResponse(
            fileId, file.FileName, storedFile.FileType, file.Length, storedFile.UploadedAt));
    }
}
