using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using myCSharpApp.Services;
using Microsoft.Extensions.Options;
using myCSharpApp.Models;

namespace myCSharpApp;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly string[] _sourceFolders;
    private readonly WorkerSettings _settings;
    private readonly string _archiveFolder;
    private readonly FileProcessingService _fileProcessingService;
    private readonly List<FileSystemWatcher> _watchers = new();

    public Worker(ILogger<Worker> logger, FileProcessingService fileProcessingService, IOptions<WorkerSettings> options)
    {
        _logger = logger;
        _fileProcessingService = fileProcessingService;
        _settings = options.Value;

        var root = Directory.GetCurrentDirectory();
        _sourceFolders = [.. _settings.SourceFolders.Select(folder => Path.Combine(root, folder))];
        _archiveFolder = Path.Combine(root, _settings.ArchiveFolder);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Directory.CreateDirectory(_archiveFolder);
        foreach (var folder in _sourceFolders) {
            Directory.CreateDirectory(folder);
            var files = Directory.GetFiles(folder);

            foreach (var file in files) {
                if (stoppingToken.IsCancellationRequested) { 
                    break;
                }
                await _fileProcessingService.ProcessFileAsync(file, _archiveFolder);
            }

            var watcher = new FileSystemWatcher(folder) {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.LastWrite
            };

            watcher.Created += OnFileCreated;
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            _watchers.Add(watcher);
            _logger.LogInformation("Watching folder: {folder}", folder);
        }

        _logger.LogInformation("Worker started.");

        while (!stoppingToken.IsCancellationRequested) {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        try {
            await _fileProcessingService.ProcessFileAsync( e.FullPath, _archiveFolder);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Failed processing file: {file}", e.FullPath);
        }        
    }

    public override void Dispose() {
        foreach (var watcher in _watchers) {
            watcher.Dispose();
        }
        base.Dispose();
    }
}