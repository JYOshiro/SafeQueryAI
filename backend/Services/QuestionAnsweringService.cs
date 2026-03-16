using PrivateDoc.Api.Contracts;
using PrivateDoc.Api.Models;
using PrivateDoc.Api.Services.Interfaces;

namespace PrivateDoc.Api.Services;

/// <summary>
/// Answers questions using only the text content extracted from session-uploaded files.
/// This is a straightforward keyword/phrase matching approach for Phase 1.
/// It does not call any external LLM — all processing is local.
/// An LLM integration point can be added behind this interface in a later phase.
/// </summary>
public class QuestionAnsweringService : IQuestionAnsweringService
{
    private const int MaxEvidenceSnippetLength = 300;
    private const int ContextWindowChars = 400;
    private const int MinMatchScore = 1;

    public AskQuestionResponse Answer(string question, IReadOnlyList<StoredFileInfo> files)
    {
        if (files.Count == 0)
        {
            return new AskQuestionResponse(
                Question: question,
                Answer: "No files have been uploaded to this session. Please upload a PDF or CSV file first.",
                HasConfidentAnswer: false,
                Evidence: new List<EvidenceItem>()
            );
        }

        // Extract meaningful keywords from the question (ignore short stop words)
        var keywords = ExtractKeywords(question);

        var allMatches = new List<(StoredFileInfo File, string Snippet, int Score)>();

        foreach (var file in files)
        {
            if (string.IsNullOrWhiteSpace(file.ExtractedText))
                continue;

            var matches = FindMatches(file.ExtractedText, keywords);
            foreach (var (snippet, score) in matches)
            {
                allMatches.Add((file, snippet, score));
            }
        }

        // Sort by relevance score descending, take top evidence items
        var topMatches = allMatches
            .OrderByDescending(m => m.Score)
            .Take(3)
            .ToList();

        if (topMatches.Count == 0 || topMatches[0].Score < MinMatchScore)
        {
            return new AskQuestionResponse(
                Question: question,
                Answer: "The uploaded files do not contain sufficient information to answer this question confidently. " +
                        "Please ensure the relevant content is present in your uploaded files.",
                HasConfidentAnswer: false,
                Evidence: new List<EvidenceItem>()
            );
        }

        // Build a concise answer from the top matching snippets
        var evidenceItems = topMatches
            .Select(m => new EvidenceItem(
                FileName: m.File.OriginalFileName,
                Snippet: TrimSnippet(m.Snippet)
            ))
            .ToList();

        var answerText = BuildAnswer(question, topMatches);

        return new AskQuestionResponse(
            Question: question,
            Answer: answerText,
            HasConfidentAnswer: true,
            Evidence: evidenceItems
        );
    }

    private static string[] ExtractKeywords(string question)
    {
        var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "a", "an", "the", "is", "are", "was", "were", "be", "been",
            "have", "has", "had", "do", "does", "did", "will", "would",
            "could", "should", "may", "might", "shall", "can", "need",
            "what", "which", "who", "whom", "whose", "when", "where",
            "why", "how", "that", "this", "these", "those", "i", "me",
            "my", "we", "our", "you", "your", "he", "she", "it", "they",
            "them", "their", "in", "on", "at", "to", "for", "of", "and",
            "or", "but", "not", "with", "about", "from", "by", "as", "tell"
        };

        return question
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(w => w.Trim('.', '?', '!', ',', '"', '\''))
            .Where(w => w.Length > 2 && !stopWords.Contains(w))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static List<(string Snippet, int Score)> FindMatches(string text, string[] keywords)
    {
        var results = new List<(string Snippet, int Score)>();

        if (keywords.Length == 0)
            return results;

        // Split text into sentences/segments for snippet extraction
        var segments = text
            .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(s => s.Length > 10)
            .ToArray();

        foreach (var segment in segments)
        {
            var score = keywords.Count(k =>
                segment.Contains(k, StringComparison.OrdinalIgnoreCase));

            if (score > 0)
            {
                results.Add((segment, score));
            }
        }

        // Also do a broader context window search for multi-keyword spans
        foreach (var keyword in keywords)
        {
            int idx = 0;
            while (true)
            {
                idx = text.IndexOf(keyword, idx, StringComparison.OrdinalIgnoreCase);
                if (idx < 0) break;

                int start = Math.Max(0, idx - ContextWindowChars / 2);
                int end = Math.Min(text.Length, idx + ContextWindowChars / 2);
                var snippet = text[start..end];

                var score = keywords.Count(k =>
                    snippet.Contains(k, StringComparison.OrdinalIgnoreCase));

                results.Add((snippet, score));
                idx += keyword.Length;
            }
        }

        return results;
    }

    private static string BuildAnswer(string question, List<(StoredFileInfo File, string Snippet, int Score)> matches)
    {
        var fileNames = matches
            .Select(m => m.File.OriginalFileName)
            .Distinct()
            .ToList();

        var fileList = fileNames.Count == 1
            ? $"\"{fileNames[0]}\""
            : string.Join(", ", fileNames.Take(fileNames.Count - 1).Select(f => $"\"{f}\"")) +
              $" and \"{fileNames[^1]}\"";

        var topSnippet = TrimSnippet(matches[0].Snippet);

        return $"Based on the uploaded file(s) {fileList}, the most relevant content found is:\n\n{topSnippet}";
    }

    private static string TrimSnippet(string snippet)
    {
        snippet = snippet.Trim();
        if (snippet.Length <= MaxEvidenceSnippetLength)
            return snippet;

        return snippet[..MaxEvidenceSnippetLength].TrimEnd() + "…";
    }
}
