using SafeQueryAI.Api.Contracts;
using SafeQueryAI.Api.Models;
using SafeQueryAI.Api.Services.Interfaces;

namespace SafeQueryAI.Api.Services;

/// <summary>
/// Answers questions using Retrieval-Augmented Generation (RAG):
///   1. The question is embedded via Ollama's embedding model.
///   2. The most relevant document chunks are retrieved from the in-memory vector store.
///   3. Retrieved chunks are sent as context to the Ollama LLM for a grounded answer.
///
/// Gracefully falls back to keyword matching when Ollama is unavailable or the session
/// has not yet been indexed (e.g. Ollama was offline during file upload).
/// </summary>
public class QuestionAnsweringService : IQuestionAnsweringService
{
    private const int TopK = 5;
    private const int MaxSnippetLength = 300;
    private const int ContextWindowChars = 400;
    private const int MinKeywordScore = 1;

    private readonly IOllamaService _ollama;
    private readonly IVectorStoreService _vectorStore;
    private readonly ILogger<QuestionAnsweringService> _logger;

    public QuestionAnsweringService(
        IOllamaService ollama,
        IVectorStoreService vectorStore,
        ILogger<QuestionAnsweringService> logger)
    {
        _ollama = ollama;
        _vectorStore = vectorStore;
        _logger = logger;
    }

    public async Task<AskQuestionResponse> AnswerAsync(
        string question,
        string sessionId,
        IReadOnlyList<StoredFileInfo> files,
        CancellationToken cancellationToken = default)
    {
        if (files.Count == 0)
        {
            return new AskQuestionResponse(
                Question: question,
                Answer: "No files have been uploaded to this session. Please upload a PDF or CSV file first.",
                HasConfidentAnswer: false,
                Evidence: new List<EvidenceItem>());
        }

        // ── RAG path ──────────────────────────────────────────────────────────
        try
        {
            var queryEmbedding = await _ollama.GetEmbeddingAsync(question, cancellationToken);
            var relevantChunks = _vectorStore.Search(sessionId, queryEmbedding, TopK);

            if (relevantChunks.Count > 0)
            {
                var contextItems = relevantChunks
                    .Select(c => $"[From: {c.FileName}]\n{c.Text}");

                var answer = await _ollama.GenerateAnswerAsync(question, contextItems, cancellationToken);

                var evidence = relevantChunks
                    .GroupBy(c => c.FileName)
                    .Select(g => new EvidenceItem(g.Key, TrimSnippet(g.First().Text)))
                    .Take(3)
                    .ToList();

                return new AskQuestionResponse(
                    Question: question,
                    Answer: answer,
                    HasConfidentAnswer: true,
                    Evidence: evidence);
            }

            // Vector store empty for this session — Ollama may have been offline during upload.
            _logger.LogWarning(
                "No indexed chunks found for session {SessionId}. Falling back to keyword search.",
                sessionId);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(ex, "Ollama unavailable. Falling back to keyword search.");
        }

        // ── Keyword-matching fallback ─────────────────────────────────────────
        return KeywordAnswer(question, files);
    }

    // ── Keyword matching (fallback) ───────────────────────────────────────────

    private static AskQuestionResponse KeywordAnswer(string question, IReadOnlyList<StoredFileInfo> files)
    {
        var keywords = ExtractKeywords(question);
        var allMatches = new List<(StoredFileInfo File, string Snippet, int Score)>();

        foreach (var file in files)
        {
            if (string.IsNullOrWhiteSpace(file.ExtractedText)) continue;

            foreach (var (snippet, score) in FindMatches(file.ExtractedText, keywords))
                allMatches.Add((file, snippet, score));
        }

        var topMatches = allMatches
            .OrderByDescending(m => m.Score)
            .Take(3)
            .ToList();

        if (topMatches.Count == 0 || topMatches[0].Score < MinKeywordScore)
        {
            return new AskQuestionResponse(
                Question: question,
                Answer: "The uploaded files do not contain sufficient information to answer this question. " +
                        "Please ensure the relevant content is present in your uploaded files.",
                HasConfidentAnswer: false,
                Evidence: new List<EvidenceItem>());
        }

        var evidence = topMatches
            .Select(m => new EvidenceItem(m.File.OriginalFileName, TrimSnippet(m.Snippet)))
            .ToList();

        var fileNames = topMatches.Select(m => m.File.OriginalFileName).Distinct().ToList();
        var fileList  = fileNames.Count == 1
            ? $"\"{fileNames[0]}\""
            : string.Join(", ", fileNames.SkipLast(1).Select(f => $"\"{f}\"")) + $" and \"{fileNames[^1]}\"";

        var answerText =
            $"Based on the uploaded file(s) {fileList}, the most relevant content found is:\n\n" +
            TrimSnippet(topMatches[0].Snippet);

        return new AskQuestionResponse(
            Question: question,
            Answer: answerText,
            HasConfidentAnswer: true,
            Evidence: evidence);
    }

    private static string[] ExtractKeywords(string question)
    {
        var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "a","an","the","is","are","was","were","be","been","have","has","had",
            "do","does","did","will","would","could","should","may","might","shall",
            "can","need","what","which","who","whom","whose","when","where","why",
            "how","that","this","these","those","i","me","my","we","our","you",
            "your","he","she","it","they","them","their","in","on","at","to","for",
            "of","and","or","but","not","with","about","from","by","as","tell"
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
        if (keywords.Length == 0) return results;

        var segments = text
            .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(s => s.Length > 10);

        foreach (var segment in segments)
        {
            int score = keywords.Count(k => segment.Contains(k, StringComparison.OrdinalIgnoreCase));
            if (score > 0) results.Add((segment, score));
        }

        foreach (var keyword in keywords)
        {
            int idx = 0;
            while (true)
            {
                idx = text.IndexOf(keyword, idx, StringComparison.OrdinalIgnoreCase);
                if (idx < 0) break;

                int start   = Math.Max(0, idx - ContextWindowChars / 2);
                int end     = Math.Min(text.Length, idx + ContextWindowChars / 2);
                var snippet = text[start..end];
                int score   = keywords.Count(k => snippet.Contains(k, StringComparison.OrdinalIgnoreCase));

                results.Add((snippet, score));
                idx += keyword.Length;
            }
        }

        return results;
    }

    private static string TrimSnippet(string snippet)
    {
        snippet = snippet.Trim();
        return snippet.Length <= MaxSnippetLength
            ? snippet
            : snippet[..MaxSnippetLength].TrimEnd() + "…";
    }
}
