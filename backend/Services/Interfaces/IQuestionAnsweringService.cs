using PrivateDoc.Api.Contracts;
using PrivateDoc.Api.Models;

namespace PrivateDoc.Api.Services.Interfaces;

public interface IQuestionAnsweringService
{
    /// <summary>
    /// Searches the session's uploaded file content for an answer to the question.
    /// Returns a grounded response based only on available session content.
    /// </summary>
    AskQuestionResponse Answer(string question, IReadOnlyList<StoredFileInfo> files);
}
