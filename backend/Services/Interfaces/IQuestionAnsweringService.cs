using SafeQueryAI.Api.Contracts;
using SafeQueryAI.Api.Models;

namespace SafeQueryAI.Api.Services.Interfaces;

public interface IQuestionAnsweringService
{
    /// <summary>
    /// Searches the session's uploaded file content for an answer to the question.
    /// Returns a grounded response based only on available session content.
    /// </summary>
    AskQuestionResponse Answer(string question, IReadOnlyList<StoredFileInfo> files);
}
