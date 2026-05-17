namespace LogViewerApi.Services;

public class ArchiveService
{
    private readonly string _archivePath;

    public ArchiveService()
    {
        _archivePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..",
            "myCSharpApp",
            "archive");
    }

    public List<string> GetAllFiles()
    {
        if (!Directory.Exists(_archivePath))
        {
            return new List<string>();
        }

        return Directory
            .GetFiles(
                _archivePath,
                "*",
                SearchOption.AllDirectories)
            .Select(file =>
                Path.GetRelativePath(
                    _archivePath,
                    file))
            .ToList();
    }

    public object GetFileContent(string path)
    {
        var fullPath = Path.Combine(
            _archivePath,
            path);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException();
        }

        var extension =
            Path.GetExtension(fullPath)
                .ToLower();

        var textExtensions = new[]
        {
            ".log",
            ".txt",
            ".json",
            ".xml",
            ".csv"
        };

        if (textExtensions.Contains(extension))
        {
            var content =
                File.ReadAllText(fullPath);

            return new
            {
                Type = "text",
                FileName = Path.GetFileName(fullPath),
                Content = content
            };
        }

        return new
        {
            Type = "binary",
            FileName = Path.GetFileName(fullPath),
            Message =
                "Preview not supported for this file type."
        };
    }
}