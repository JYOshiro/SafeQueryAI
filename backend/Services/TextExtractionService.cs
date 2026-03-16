using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using PrivateDoc.Api.Services.Interfaces;

namespace PrivateDoc.Api.Services;

/// <summary>
/// Extracts plain text from text-based PDFs and CSV files.
/// OCR is out of scope for Phase 1 — only text-layer PDFs are supported.
/// </summary>
public class TextExtractionService : ITextExtractionService
{
    private readonly ILogger<TextExtractionService> _logger;

    public TextExtractionService(ILogger<TextExtractionService> logger)
    {
        _logger = logger;
    }

    public string ExtractFromPdf(string filePath)
    {
        try
        {
            using var document = PdfDocument.Open(filePath);
            var textBuilder = new System.Text.StringBuilder();

            foreach (Page page in document.GetPages())
            {
                textBuilder.AppendLine(page.Text);
            }

            return textBuilder.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract text from PDF: {FilePath}", filePath);
            return string.Empty;
        }
    }

    public string ExtractFromCsv(string filePath)
    {
        try
        {
            var lines = File.ReadAllLines(filePath);
            if (lines.Length == 0)
                return string.Empty;

            var textBuilder = new System.Text.StringBuilder();

            // First line is treated as the header row
            var headers = lines[0].Split(',');
            textBuilder.AppendLine($"CSV file with {lines.Length - 1} data rows and columns: {string.Join(", ", headers)}");
            textBuilder.AppendLine();

            // Include all rows as readable text (header + value pairs per row)
            foreach (var line in lines.Skip(1))
            {
                var values = line.Split(',');
                for (int i = 0; i < headers.Length && i < values.Length; i++)
                {
                    textBuilder.Append($"{headers[i].Trim()}: {values[i].Trim()}  ");
                }
                textBuilder.AppendLine();
            }

            return textBuilder.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract text from CSV: {FilePath}", filePath);
            return string.Empty;
        }
    }
}
