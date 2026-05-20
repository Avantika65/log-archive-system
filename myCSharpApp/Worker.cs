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
    private readonly DatabaseService _databaseService;
    private readonly ValidationService _validationService;
    private readonly FileProcessingService _fileProcessingService;
    private readonly List<FileSystemWatcher> _watchers = new();
    private readonly HashSet<string> _processingFiles = new();

    public Worker(ILogger<Worker> logger, DatabaseService databaseService, 
    ValidationService validationService, FileProcessingService fileProcessingService, IOptions<WorkerSettings> options)
    {
        _logger = logger;
        _databaseService = databaseService;
        _validationService = validationService;
        _fileProcessingService = fileProcessingService;
        _settings = options.Value;

        var root = Directory.GetCurrentDirectory();
        _sourceFolders = [.. _settings.SourceFolders.Select(folder => Path.Combine(root, folder))];
        _archiveFolder = Path.Combine(root, _settings.ArchiveFolder);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Directory.CreateDirectory(_archiveFolder);
        foreach (var folder in _sourceFolders)
        {
            Directory.CreateDirectory(folder);
            var watcher = new FileSystemWatcher(folder)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
            };

            watcher.Created += OnFileCreated;
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            _watchers.Add(watcher);

            _logger.LogInformation("Watching folder: {folder}", folder);
        }

        _logger.LogInformation("Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        try
        {
            await _fileProcessingService.ProcessFileAsync( e.FullPath, _archiveFolder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed processing file: {file}", e.FullPath);
        }        
    }
}