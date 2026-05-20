using Microsoft.Extensions.Logging;
using Moq;
using myCSharpApp.Services;

namespace myCSharpApp.Tests;

public class ValidationServiceTests
{
    private readonly ValidationService _validationService;

    public ValidationServiceTests()
    {
        var logger = new Mock<ILogger<ValidationService>>();
        _validationService = new ValidationService(logger.Object);
    }

    [Fact]
    public void IsTemporaryFile_ShouldReturnTrue_ForTempFile()
    {
        var result = _validationService.IsTemporaryFile("test.tmp");
        Assert.True(result);
    }

    [Fact]
    public void IsTemporaryFile_ShouldReturnFalse_ForNormalFile()
    {
        var result =_validationService.IsTemporaryFile("test.log");
        Assert.False(result);
    }

    [Fact]
    public void IsPdfValid_ShouldReturnFalse_ForInvalidPdf()
    {
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "not a pdf");

        var result = _validationService.IsPdfValid(tempFile);
        Assert.False(result);
        File.Delete(tempFile);
    }

    [Fact]
    public void IsTextFileReadable_ShouldReturnTrue_ForReadableFile()
    {
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "hello");

        var result = _validationService.IsTextFileReadable(tempFile);
        Assert.True(result);
        File.Delete(tempFile);
    }
}
