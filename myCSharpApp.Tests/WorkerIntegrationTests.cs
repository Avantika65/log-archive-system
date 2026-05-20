using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

using myCSharpApp.Models;
using myCSharpApp.Services;

namespace myCSharpApp.Tests;

public class WorkerIntegrationTests
{
    [Fact]
    public async Task Worker_ShouldCopyFile_ToArchive()
    {
        var sourceFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var archiveFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        Directory.CreateDirectory(sourceFolder);
        Directory.CreateDirectory(archiveFolder);

        var logger = new Mock<ILogger<Worker>>();
        var dbLogger = new Mock<ILogger<DatabaseService>>();
        var validationLogger = new Mock<ILogger<ValidationService>>();
        var databaseService = new DatabaseService(dbLogger.Object, Path.GetTempFileName());
        var validationService = new ValidationService(validationLogger.Object);
        var settings = Options.Create(
                new WorkerSettings
                {
                    ArchiveFolder = archiveFolder,
                    SourceFolders = [ sourceFolder ]
                });

        var processingLogger = new Mock<ILogger<FileProcessingService>>();
        var fileProcessingService = new FileProcessingService(processingLogger.Object, validationService, databaseService);
        var worker = new Worker(logger.Object, databaseService, validationService, fileProcessingService, settings);
        var testFile = Path.Combine(sourceFolder, "test.log");

        await File.WriteAllTextAsync(testFile, "integration test");
        await Task.Delay(2000);

        var archivedFile = Path.Combine(archiveFolder, Path.GetFileName(sourceFolder), "test.log");

        Assert.True(File.Exists(testFile));
        Directory.Delete(sourceFolder, true);
        Directory.Delete(archiveFolder, true);
    }
}