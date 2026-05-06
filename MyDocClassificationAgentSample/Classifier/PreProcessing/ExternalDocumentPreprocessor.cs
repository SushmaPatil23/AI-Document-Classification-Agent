using Daenet.DocumentParser;
using Daenet.DocumentParser.DocumentParsers;
using Microsoft.Extensions.Logging;

namespace Classifier.PreProcessing;

/// <summary>
/// Extracts text from documents using the external parser library.
/// Supports PDF, DOCX, TXT, and images with OCR.
/// </summary>
public class ExternalDocumentPreprocessor : IDocumentPreprocessor
{
    /// <summary>
    /// Parser used to extract text from documents.
    /// </summary>
    private readonly Parser _parser;

    /// <summary>
    /// Initializes the parser with configuration settings.
    /// </summary>
    public ExternalDocumentPreprocessor()
    {
        var config = new Config
        {
            InFolder = "Documents",
            OutFolder = "Output",
            FileSeparatorInContent = "\n\n",

            OcrParserConfig = new OcrParserConfig
            {
                TessarectFilePath = AppContext.BaseDirectory,
                OcrParserLanguage = "eng"
            }
        };

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
        });

        _parser = new Parser(config, loggerFactory);
    }

    /// <summary>
    /// Extracts text from the given document file.
    /// </summary>
    /// <param name="filePath">Path of the file.</param>
    /// <returns>Extracted text content.</returns>
    public async Task<string> ExtractTextAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException(nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);

        Console.WriteLine($"[Parser] Processing: {Path.GetFileName(filePath)}");

        try
        {
            var text = await _parser.ParseFileAsync(filePath);

            Console.WriteLine($"[Parser] Success: {Path.GetFileName(filePath)}");

            return text;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Parser Error] {Path.GetFileName(filePath)}: {ex.Message}");
            return string.Empty;
        }
    }
}