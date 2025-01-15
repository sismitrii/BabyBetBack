using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BabyBetBack.Controllers;

[ApiController]
public class TestController(IBetService betService) :ControllerBase
{
    [HttpGet]
    [Route("test")]
    public async Task<IActionResult> Test()
    {
        try
        {
            var betGame = await betService.GetUser();
            return Ok(betGame);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [HttpGet("contents")]
    public IActionResult GetDataFolderContents()
    {
        var dataFolderPath = "/app/data";
        try
        {
            if (!Directory.Exists(dataFolderPath))
            {
                return NotFound(new { message = $"Le dossier '{dataFolderPath}' n'existe pas." });
            }

            var directoryInfo = new DirectoryInfo(dataFolderPath);
            var files = directoryInfo.GetFiles();
            var directories = directoryInfo.GetDirectories();

            var result = new
            {
                Path = dataFolderPath,
                Files = files.Select(f => new { f.Name, f.Length, f.CreationTime }),
                Directories = directories.Select(d => new { d.Name, d.CreationTime })
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Une erreur s'est produite lors de la lecture du dossier.", error = ex.Message });
        }
    }
}