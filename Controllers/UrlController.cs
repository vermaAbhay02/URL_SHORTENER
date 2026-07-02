using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using URL_Shortener.DTOs.URL_ShortenDTO;
using URL_Shortener.Models;
using URL_Shortener.Models.Data;
using URL_Shortener.Services;

namespace URL_Shortener.Controllers;

[ApiController]
[Route("api/urls")]
[Authorize]
public class UrlController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ShortCodeService _shortCodeService;

    public UrlController(AppDbContext db, ShortCodeService shortCodeService)
    {
        _db = db;
        _shortCodeService = shortCodeService;
    }

    // ── Shorten URL ───────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> ShortenUrl([FromBody] CreateUrlDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // If the token is valid but the mapping failed or claim is missing
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Invalid token claims" });

        var user = await _db.Users.FindAsync(userId);

        if (user == null)
            return Unauthorized(new { message = "User no longer exists in the database. Please re-login." });

        var shortCode="";
        // Custom alias is Pro only
        if (!string.IsNullOrEmpty(dto.CustomAlias))
        {
            if (user.Plan != Plan.Pro)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Custom aliases are a Pro feature." });

            // Check if custom alias already taken
            var aliasTaken = await _db.ShortenedURLs
                .AnyAsync(x => x.ShortCode == dto.CustomAlias);

            if (aliasTaken)
                return Conflict(new { message = "This custom alias is already taken" });
            else shortCode=dto.CustomAlias;    
        }
        else{
            shortCode = await _shortCodeService.GenerateUniqueCodeAsync();
        }

        var shortenedUrl = new ShortenedURL
        {
            UserId = userId!,
            OriginalUrl = dto.OriginalUrl,
            ShortCode = shortCode,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _db.ShortenedURLs.Add(shortenedUrl);
        await _db.SaveChangesAsync();

        return Ok(MapToDto(shortenedUrl));
    }

    // ── Get All My URLs ───────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetMyUrls()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var urls = await _db.ShortenedURLs
            .Where(x => x.UserId == userId)
            .Include(x => x.ClickLogs)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(urls.Select(MapToDto));
    }

    // ── Get Single URL ────────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUrl(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var url = await _db.ShortenedURLs
            .Include(x => x.ClickLogs)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (url == null)
            return NotFound(new { message = "URL not found" });

        return Ok(MapToDto(url));
    }

    // ── Delete URL ────────────────────────────────────────────
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUrl(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var url = await _db.ShortenedURLs
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (url == null)
            return NotFound(new { message = "URL not found" });

        _db.ShortenedURLs.Remove(url);
        await _db.SaveChangesAsync();

        return Ok(new { message = "URL deleted successfully" });
    }

    // ── Helper ────────────────────────────────────────────────
    private UrlResponseDto MapToDto(ShortenedURL url)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        return new UrlResponseDto
        {
            Id = url.Id,
            OriginalUrl = url.OriginalUrl,
            ShortCode = url.ShortCode,
            ShortUrl = $"{baseUrl}/r/{url.ShortCode}",
            TotalClicks = url.ClickLogs?.Count ?? 0,
            CreatedAt = url.CreatedAt
        };
    }
}