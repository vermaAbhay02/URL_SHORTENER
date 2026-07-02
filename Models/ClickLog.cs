namespace URL_Shortener.Models
{
    public class ClickLog
    {
        public int Id { get; set; }
        public int ShortenedURLId { get; set; }
        public ShortenedURL ShortenedURL { get; set; } = null!;
        public DateTime ClickedAt { get; set; } = DateTime.UtcNow;
    }
}
