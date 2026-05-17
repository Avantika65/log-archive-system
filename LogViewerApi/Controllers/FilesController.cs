using Microsoft.AspNetCore.Mvc;
using LogViewerApi.Services;

namespace LogViewerApi.Controllers;

[ApiController]
[Route("files")]
public class FilesController : ControllerBase
{
    private readonly ArchiveService _archiveService;

    public FilesController(
        ArchiveService archiveService)
    {
        _archiveService = archiveService;
    }

    [HttpGet]
    public IActionResult GetFiles()
    {
        var files =
            _archiveService.GetAllFiles();

        return Ok(files);
    }

    [HttpGet("content")]
    public IActionResult GetFileContent(
        [FromQuery] string path)
    {
        try
        {
            var result =
                _archiveService
                    .GetFileContent(path);

            return Ok(result);
        }
        catch (FileNotFoundException)
        {
            return NotFound(
                "File not found.");
        }
    }
}