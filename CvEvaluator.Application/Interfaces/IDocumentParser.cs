namespace CvEvaluator.Application.Interfaces;

public interface IDocumentParser
{
    Task<string> ParseAsync(Stream fileStream);
}
