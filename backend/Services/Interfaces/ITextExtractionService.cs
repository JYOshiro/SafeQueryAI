namespace PrivateDoc.Api.Services.Interfaces;

public interface ITextExtractionService
{
    /// <summary>Extracts plain text from a PDF file at the given path.</summary>
    string ExtractFromPdf(string filePath);

    /// <summary>Extracts text/summary from a CSV file at the given path.</summary>
    string ExtractFromCsv(string filePath);
}
