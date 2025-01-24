using Application.Exceptions;
using Application.Services;
using Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace BabyBetBack.Controllers;

[ApiController]
public class TestController(
    IBetService betService, 
    ILogger<TestController> logger,
    UserManager<User> userManager) :ControllerBase
{
    
    private readonly string _dataFolderPath = "/app/data";

    [HttpGet("api/test/exception")]
    public IActionResult TestLogger()
    {
        throw new BetException("Test exception");
        return Ok();
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
    [Authorize(Roles = "Admin")]
    public IActionResult DownloadDatabase()
    {
        var sqliteDb = Path.Combine(_dataFolderPath, "sqlite.db");

        if (!System.IO.File.Exists(sqliteDb))
            return NotFound("db file doesn't exist !");

        var fileBytes = System.IO.File.ReadAllBytes(sqliteDb);
        var fileName = "sqlite.db";
        return File(fileBytes, "application/octet-stream", fileName);
    }
    
    [HttpPost("upload")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UploadDatabase(IFormFile file)
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

    [HttpPost("confirm-email")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UploadDatabase(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        user.EmailConfirmed = true;
        var result = await userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            return Ok(new { Message = "L'email a été confirmé avec succès." });
        }

        return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
    }
}