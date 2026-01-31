using System.Text;
using System.Text.RegularExpressions;
using CvEvaluator.Application.Interfaces;
using UglyToad.PdfPig;

namespace CvEvaluator.Infrastructure.Parsing;

public class PdfDocumentParser : IDocumentParser
{
    public async Task<string> ParseAsync(Stream fileStream)
    {
        if (fileStream == null || !fileStream.CanRead)
            return string.Empty;

        using var ms = new MemoryStream();
        await fileStream.CopyToAsync(ms);
        ms.Position = 0;

        var sb = new StringBuilder();

        using (var document = PdfDocument.Open(ms))
        {
            foreach (var page in document.GetPages())
            {
                var pageText = page.Text;

                if (!string.IsNullOrWhiteSpace(pageText))
                {
                    sb.AppendLine(pageText);
                    sb.AppendLine(); // page separator
                }
            }
        }

        return NormalizeText(sb.ToString());
    }

    private static string NormalizeText(string text)
    {
        // Normalize line endings
        text = text
            .Replace("\r\n", "\n")
            .Replace("\r", "\n");

        // Reduce excessive empty lines (keep max 2)
        text = Regex.Replace(text, "\n{3,}", "\n\n");

        // Reduce excessive spaces but keep line structure
        text = Regex.Replace(text, "[ \t]{2,}", " ");

        return text.Trim();
    }
}
