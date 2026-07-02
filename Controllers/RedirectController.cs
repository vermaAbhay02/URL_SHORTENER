using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using URL_Shortener.Models;
using URL_Shortener.Models.Data;
using URL_Shortener.Services;

namespace URL_Shortener.Controllers;

[ApiController]
public class RedirectController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ClickLogQueue _queue;

    public RedirectController(AppDbContext db,ClickLogQueue queue)
    {
        _db = db;
        _queue=queue;
    }

    [HttpGet("r/{code}")]
    public async Task<IActionResult> RedirectToUrl(string code)
    {
        // Find by short code 
        var url = await _db.ShortenedURLs
            .FirstOrDefaultAsync(x => x.ShortCode == code);

        if (url == null)
            return NotFound(new { message = "Short URL not found" });

        if (!url.IsActive)
            return BadRequest(new { message = "This link is no longer active" });

        // Log the click
        // var click = new ClickLog
        // {
        //     ShortenedURLId = url.Id,
        //     ClickedAt = DateTime.UtcNow
        // };
        // _db.ClickLogs.Add(click);
        // await _db.SaveChangesAsync();

        _queue.Enqueue(new ClickLog{
            ShortenedURLId = url.Id,
            ClickedAt = DateTime.UtcNow
        });
        // Redirect to original URL
        return Redirect(url.OriginalUrl);
    }
}