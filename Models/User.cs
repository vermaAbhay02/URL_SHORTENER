using Microsoft.AspNetCore.Identity;
using System.Numerics;

namespace URL_Shortener.Models
{
    public class User:IdentityUser
    {
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
        public DateTime? CreatedAt { get; set; }=DateTime.UtcNow;
        public Plan Plan { get; set; } = Plan.Free;
        public List<ShortenedURL> ShortenedURLs = new List<ShortenedURL>();

        public string SecurityQuestion {get;set;}=string.Empty;
        public string SecurityAnswerHash {get;set;}=string.Empty;
    }
    public enum Plan { 
        Free=0,
        Pro=1
    }
}

