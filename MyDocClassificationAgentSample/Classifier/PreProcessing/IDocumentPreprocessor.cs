/// <summary>
/// Defines methods to extract text from documents before generating embeddings.
/// </summary>
public interface IDocumentPreprocessor
{
    /// <summary>
    /// Extracts text from the given document file.
    /// </summary>
    /// <param name="filePath">Path of the file.</param>
    /// <returns>Extracted text content.</returns>
    Task<string> ExtractTextAsync(string filePath);
}