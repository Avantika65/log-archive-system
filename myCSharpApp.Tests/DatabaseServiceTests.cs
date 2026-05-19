using Microsoft.Extensions.Logging;
using Moq;

namespace myCSharpApp.Tests;

public class DatabaseServiceTests
{
    [Fact]
    public void SaveProcessedFile_ShouldInsertRecord()
    {
        var databasePath = Path.GetTempFileName();
        var logger = new Mock<ILogger<DatabaseService>>();
        var databaseService = new DatabaseService(logger.Object, databasePath);

        databaseService.SaveProcessedFile("test.log", "/source/test.log", "/archive/test.log", "hash123");

        var exists = databaseService.FileHashExists("hash123");
        Assert.True(exists);
        File.Delete(databasePath);
    }
}