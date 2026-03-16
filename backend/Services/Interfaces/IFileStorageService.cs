using Microsoft.AspNetCore.Http;

namespace PrivateDoc.Api.Services.Interfaces;

public interface IFileStorageService
{
    /// <summary>Saves an uploaded file to a session-scoped temp folder and returns the local path.</summary>
    Task<string> SaveFileAsync(string sessionId, string fileId, IFormFile file);

    /// <summary>Deletes all files and the folder for the given session.</summary>
    void DeleteSessionFiles(string sessionId);
}
