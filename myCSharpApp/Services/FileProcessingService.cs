using System.Security.Cryptography;

namespace myCSharpApp.Services;

public class FileProcessingService
{
    private readonly ILogger<FileProcessingService> _logger;
    private readonly ValidationService _validationService;
    private readonly DatabaseService  _databaseService;
    private readonly HashSet<string> _processingFiles = new();
    public FileProcessingService(
        ILogger<FileProcessingService> logger,
        ValidationService validationService,
        DatabaseService databaseService)
    {
        _logger = logger;
        _validationService = validationService;
        _databaseService = databaseService;
    }

    public async Task ProcessFileAsync(string filePath, string archiveFolder)
        {
            try
            {
                lock (_processingFiles)
                {
                    if (_processingFiles.Contains(filePath))
                    {
                        _logger.LogWarning("Already processing: {file}",filePath);
                        return;
                    }
                    _processingFiles.Add(filePath);
                }

                _logger.LogInformation("New file detected: {file}", filePath);
                var retries = 0;
                while (!_validationService.IsFileReady(filePath))
                {
                    retries++;
                    if (retries > 10)
                    {
                        _logger.LogError("File never became ready: {file}", filePath);
                        return;
                    }
                    _logger.LogInformation("Waiting for file: {file}", filePath);
                    await Task.Delay(500);
                }

                var fileName = Path.GetFileName(filePath);
                if (_validationService.IsTemporaryFile(fileName))
                {
                    _logger.LogInformation("Ignored temp file: {file}", fileName);
                    return;
                }

                if (!_validationService.ValidateFile(filePath))
                {
                    _logger.LogError("File validation failed: {file}", filePath);
                    return;
                }

                var hash = ComputeFileHash(filePath);
                if (_databaseService.FileHashExists(hash))
                {
                    _logger.LogWarning("Duplicate file skipped: {file}", fileName);
                    return;
                }
                var parentDirectory = Path.GetDirectoryName(filePath);
                if (string.IsNullOrEmpty(parentDirectory))
                {
                    _logger.LogWarning("Invalid directory for file: {file}", filePath);
                    return;
                }

                var sourceFolder = Path.GetFileName(parentDirectory);
                var archiveSubFolder = Path.Combine(archiveFolder, sourceFolder);
                Directory.CreateDirectory(archiveSubFolder);
                var destinationPath = Path.Combine(archiveSubFolder, fileName);

                if (File.Exists(destinationPath))
                {
                    _logger.LogWarning("Duplicate skipped: {file}", fileName);
                    return;
                }
                using var sourceStream = File.OpenRead(filePath);
                using var destinationStream = File.Create(destinationPath);
                await sourceStream.CopyToAsync(destinationStream);
                _databaseService.SaveProcessedFile(fileName, filePath, destinationPath, hash);
                _logger.LogInformation("Copied file: {file}", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed processing file: {file}", filePath);
            }
            finally
            {
                lock (_processingFiles)
                {
                    _processingFiles.Remove(filePath);
                }
            }
        }

    private static string ComputeFileHash(string path)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        using var stream = File.OpenRead(path);
        var hashBytes = sha256.ComputeHash(stream);
        return Convert.ToHexString(hashBytes);
    }
}