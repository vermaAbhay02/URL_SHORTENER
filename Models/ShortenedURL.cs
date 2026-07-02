namespace URL_Shortener.Models
{
    public class ShortenedURL
    {
        public int Id { get; set; }
        public User User { get; set; } = null!;
        public string UserId { get; set; } = string.Empty;
        public string OriginalUrl { get; set; } = string.Empty;
        public string ShortCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public List<ClickLog> ClickLogs { get; set; } = new List<ClickLog>();
 
    }
}
