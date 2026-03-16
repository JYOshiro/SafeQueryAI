using SafeQueryAI.Api.Services.Interfaces;

namespace SafeQueryAI.Api.Services;

/// <summary>
/// Handles saving and deleting uploaded files in session-scoped temporary directories.
/// Files are never stored permanently; they are cleared when the session ends.
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(IConfiguration configuration, ILogger<FileStorageService> logger)
    {
        _logger = logger;
        var configuredPath = configuration["SafeQueryAI:TempStoragePath"] ?? "TempSessions";
        // Resolve relative to the app's content root
        _basePath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(AppContext.BaseDirectory, configuredPath);
    }

    public async Task<string> SaveFileAsync(string sessionId, string fileId, IFormFile file)
    {
        var sessionDir = Path.Combine(_basePath, sessionId);
        Directory.CreateDirectory(sessionDir);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var safeFileName = $"{fileId}{extension}";
        var filePath = Path.Combine(sessionDir, safeFileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        _logger.LogInformation("Saved file {FileId} for session {SessionId}", fileId, sessionId);
        return filePath;
    }

    public void DeleteSessionFiles(string sessionId)
    {
        var sessionDir = Path.Combine(_basePath, sessionId);
        if (Directory.Exists(sessionDir))
        {
            Directory.Delete(sessionDir, recursive: true);
            _logger.LogInformation("Deleted session directory for session {SessionId}", sessionId);
        }
    }
}
