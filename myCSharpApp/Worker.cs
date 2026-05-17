using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace myCSharpApp;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    private readonly string[] _sourceFolders;

    private readonly string _archiveFolder;

    private readonly DatabaseService _databaseService;

    private readonly List<FileSystemWatcher> _watchers = new();

    private readonly HashSet<string> _processingFiles = new();


    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
        _databaseService = new DatabaseService();

        var root = Directory.GetCurrentDirectory();

        _sourceFolders = new[]
        {
            Path.Combine(root, "source1"),
            Path.Combine(root, "source2")
        };

        _archiveFolder =
            Path.Combine(root, "archive");
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        Directory.CreateDirectory(_archiveFolder);

        foreach (var folder in _sourceFolders)
        {
            Directory.CreateDirectory(folder);

            var watcher = new FileSystemWatcher(folder);
            watcher.NotifyFilter =
                NotifyFilters.FileName |
                NotifyFilters.CreationTime;

            watcher.Created += OnFileCreated;
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            _watchers.Add(watcher);

            _logger.LogInformation(
                "Watching folder: {folder}",
                folder);
        }

        _logger.LogInformation("Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async void OnFileCreated(
    object sender,
    FileSystemEventArgs e)
{
    try
    {
        _logger.LogInformation(
            "New file detected: {file}",
            e.FullPath);

        var fileName = Path.GetFileName(e.FullPath);

        // Ignore temp/hidden files
        if (fileName.StartsWith(".") ||
            fileName.EndsWith(".tmp"))
        {
            _logger.LogInformation(
                "Ignored temp file: {file}",
                fileName);

            return;
        }

        // Prevent duplicate event processing
        lock (_processingFiles)
        {
            if (_processingFiles.Contains(e.FullPath))
            {
                _logger.LogWarning(
                    "Already processing: {file}",
                    e.FullPath);

                return;
            }

            _processingFiles.Add(e.FullPath);
        }

        // Wait until file is fully available
        int retries = 0;

        while (!IsFileReady(e.FullPath))
        {
            retries++;

            if (retries > 10)
            {
                _logger.LogError(
                    "File never became ready: {file}",
                    e.FullPath);

                return;
            }

            _logger.LogInformation(
                "Waiting for file: {file}",
                e.FullPath);

            await Task.Delay(500);
        }

        // Generate file hash
        var hash = ComputeFileHash(e.FullPath);

        // Skip duplicate content
        if (_databaseService.FileHashExists(hash))
        {
            _logger.LogWarning(
                "Duplicate file skipped: {file}",
                fileName);

            return;
        }

        // Preserve source folder structure
        var parentDirectory =
            Path.GetDirectoryName(e.FullPath);

        if (string.IsNullOrEmpty(parentDirectory))
        {
            _logger.LogWarning(
                "Invalid directory for file: {file}",
                e.FullPath);

            return;
        }

        var sourceFolder =
            Path.GetFileName(parentDirectory);

        var archiveSubFolder =
            Path.Combine(_archiveFolder, sourceFolder);

        Directory.CreateDirectory(archiveSubFolder);

        var destinationPath =
            Path.Combine(archiveSubFolder, fileName);

        // Secondary safety check
        if (File.Exists(destinationPath))
        {
            _logger.LogWarning(
                "Duplicate skipped: {file}",
                fileName);

            return;
        }

        // Copy file asynchronously
        using var sourceStream =
            File.OpenRead(e.FullPath);

        using var destinationStream =
            File.Create(destinationPath);

        await sourceStream.CopyToAsync(destinationStream);

        // Save metadata to SQLite
        _databaseService.SaveProcessedFile(
            fileName,
            e.FullPath,
            destinationPath,
            hash);

        _logger.LogInformation(
            "Copied file: {file}",
            fileName);
    }
    catch (Exception ex)
    {
        _logger.LogError(
            ex,
            "Failed processing file: {file}",
            e.FullPath);
    }
    finally
    {
        lock (_processingFiles)
        {
            _processingFiles.Remove(e.FullPath);
        }
    }
}
    private bool IsFileReady(string path)
    {
        try
        {
            using var stream = File.Open(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.None);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private string ComputeFileHash(string path)
    {
        using var sha256 =
            SHA256.Create();

        using var stream =
            File.OpenRead(path);

        var hashBytes =
            sha256.ComputeHash(stream);

        return Convert.ToHexString(hashBytes);
    }
}