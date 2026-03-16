using SafeQueryAI.Api.Services.Interfaces;

namespace SafeQueryAI.Api.Services;

/// <summary>
/// Handles saving and deleting uploaded files in session-scoped temporary directories.
/// Files are never stored permanently; they are cleared when the session ends.
///
/// On startup, any pre-existing temp directory is wiped to remove files left by a
/// previous crashed or abruptly stopped server process, honouring the temporary
/// storage promise even across unexpected restarts.
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

        PurgeOrphanedFilesOnStartup();
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

    /// <summary>
    /// Deletes the entire TempSessions directory on startup to remove any files left
    /// by a previous server instance that exited without a clean session clear.
    /// This preserves the privacy promise of strictly temporary file storage.
    /// </summary>
    private void PurgeOrphanedFilesOnStartup()
    {
        if (!Directory.Exists(_basePath))
            return;

        try
        {
            Directory.Delete(_basePath, recursive: true);
            _logger.LogInformation(
                "Startup cleanup: removed orphaned temp directory '{BasePath}'.", _basePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Startup cleanup: could not fully remove '{BasePath}'. " +
                "Orphaned files may remain on disk.", _basePath);
        }
    }
}
