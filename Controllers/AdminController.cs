using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using URL_Shortener.Models.Data;

namespace URL_Shortener.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db)
    {
        _db = db;
    }

    // ── All links with click counts ───────────────────────────
    [HttpGet("urls")]
    public async Task<IActionResult> GetAllUrls()
    {
        var urls = await _db.ShortenedURLs
            .Include(x => x.ClickLogs)
            .Include(x => x.User)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.OriginalUrl,
                x.ShortCode,
                Owner = x.User.Email,
                TotalClicks = x.ClickLogs.Count,
                x.CreatedAt,
                x.IsActive
            })
            .ToListAsync();

        return Ok(urls);
    }

    // ── Analytics for a single link ───────────────────────────
    [HttpGet("urls/{id}/analytics")]
    public async Task<IActionResult> GetUrlAnalytics(int id)
    {
        var url = await _db.ShortenedURLs
            .Include(x => x.ClickLogs)
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (url == null)
            return NotFound(new { message = "URL not found" });

        // Clicks grouped by date
        var clicksByDay = url.ClickLogs
            .GroupBy(x => x.ClickedAt.Date)
            .Select(g => new
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                Clicks = g.Count()
            })
            .OrderBy(x => x.Date)
            .ToList();

        return Ok(new
        {
            url.Id,
            url.OriginalUrl,
            url.ShortCode,
            Owner = url.User.Email,
            TotalClicks = url.ClickLogs.Count,
            ClicksByDay = clicksByDay
        });
    }

    // ── All users ─────────────────────────────────────────────
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _db.Users
            .Select(x => new
            {
                x.Id,
                x.Email,
                Plan = x.Plan.ToString(),
                TotalLinks = x.ShortenedURLs.Count()
            })
            .ToListAsync();

        return Ok(users);
    }

    // ── Deactivate a link ─────────────────────────────────────
    [HttpPatch("urls/{id}/deactivate")]
    public async Task<IActionResult> DeactivateUrl(int id)
    {
        var url = await _db.ShortenedURLs.FindAsync(id);

        if (url == null)
            return NotFound(new { message = "URL not found" });

        url.IsActive = false;
        await _db.SaveChangesAsync();

        return Ok(new { message = "URL deactivated successfully" });
    }
}