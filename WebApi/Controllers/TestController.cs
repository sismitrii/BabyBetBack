using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace BabyBetBack.Controllers;

[ApiController]
[Authorize]
public class TestController(IBetService betService) :ControllerBase
{
    
    private readonly string _dataFolderPath = "/app/data";

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
        try
        {
            if (!Directory.Exists(_dataFolderPath))
            {
                return NotFound(new { message = $"Le dossier '{_dataFolderPath}' n'existe pas." });
            }

            var directoryInfo = new DirectoryInfo(_dataFolderPath);
            var files = directoryInfo.GetFiles();
            var directories = directoryInfo.GetDirectories();

            var result = new
            {
                Path = _dataFolderPath,
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
    
    [HttpGet("download")]
    public IActionResult DownloadDatabase()
    {
        if (User.GetNameIdentifierId() != "floguerin156@gmail.com")
            return Forbid();
        
        var sqliteDb = Path.Combine(_dataFolderPath, "sqlite.db");

        if (!System.IO.File.Exists(sqliteDb))
            return NotFound("db file doesn't exist !");

        var fileBytes = System.IO.File.ReadAllBytes(sqliteDb);
        var fileName = "sqlite.db";
        return File(fileBytes, "application/octet-stream", fileName);
    }
    
    [HttpPost("upload")]
    public async Task<IActionResult> UploadDatabase([FromForm] IFormFile file)
    {
        if (User.GetNameIdentifierId() != "floguerin156@gmail.com")
            return Forbid();
        
        var sqliteDb = Path.Combine(_dataFolderPath, "sqlite_test.db");
        
        if (file == null || file.Length == 0)
            return BadRequest("Please provide a file.");

        if (!file.FileName.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
            return BadRequest("File must be a Sqlite Db (.db).");

        var tempPath = Path.Combine(Path.GetTempPath(), file.FileName);

        await using (var stream = new FileStream(tempPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        if (System.IO.File.Exists(sqliteDb))
            System.IO.File.Delete(sqliteDb);

        System.IO.File.Move(tempPath, sqliteDb);

        return Ok("Base db replace successfully");
    }
}