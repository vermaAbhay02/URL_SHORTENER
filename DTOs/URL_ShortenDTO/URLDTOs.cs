using System.ComponentModel.DataAnnotations;

namespace URL_Shortener.DTOs.URL_ShortenDTO;

public class CreateUrlDto
{
    [Required]
    [Url(ErrorMessage = "Please enter a valid URL")]
    public string OriginalUrl { get; set; } = string.Empty;

    // Only for Pro users, optional
    public string? CustomAlias { get; set; }
}

public class UrlResponseDto
{
    public int Id { get; set; }
    public string OriginalUrl { get; set; } = string.Empty;
    public string ShortCode { get; set; } = string.Empty;
    public string ShortUrl { get; set; } = string.Empty;
    public int TotalClicks { get; set; }
    public DateTime CreatedAt { get; set; }
}