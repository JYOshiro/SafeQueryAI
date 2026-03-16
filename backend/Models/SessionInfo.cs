namespace PrivateDoc.Api.Models;

/// <summary>
/// Represents an active temporary processing session.
/// Sessions are held in memory only and are not persisted.
/// </summary>
public class SessionInfo
{
    public string SessionId { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;
    public List<StoredFileInfo> Files { get; init; } = new();
}
