namespace myCSharpApp.Services;

public class ValidationService
{
    private const long MaxFileSize = 50 * 1024 * 1024;

    private readonly ILogger<ValidationService>  _logger;

    public ValidationService(ILogger<ValidationService> logger)
    {
        _logger = logger;
    }

    public bool IsFileSizeValid(string path)
    {
        var fileInfo = new FileInfo(path);
        if (fileInfo.Length > MaxFileSize)
        {
            _logger.LogError("File exceeds 50MB: {file}", path);
            return false;
        }
        return true;
    }
    public bool IsFileReady(string path)
    {
        try
        {
            using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None);
            return stream.Length > 0;
        }
        catch
        {
            return false;
        }
    }
    public bool IsPdfValid(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            byte[] buffer = new byte[4];
            stream.ReadExactly(buffer, 0, 4);
            var header = System.Text.Encoding.ASCII.GetString(buffer);
            return header == "%PDF";
        }
        catch
        {
            return false;
        }
    }
    public bool IsTemporaryFile(string fileName)
    {
        return fileName.StartsWith(".") || fileName.EndsWith(".tmp");
    }
    public bool IsTextFileReadable(string path)
    {
        try
        {
            File.ReadLines(path).FirstOrDefault();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool ValidateFile(string path)
    {
        if (!File.Exists(path))
        {
            return false;
        }
        if (!IsFileSizeValid(path))
        {
            return false;
        }

        var extension = Path.GetExtension(path).ToLower();

        if (extension == ".pdf")
        {
            return IsPdfValid(path);
        }
        if (extension == ".log" || extension == ".txt")
        {
            return IsTextFileReadable(path);
        }

        return true;
    }
}