using Microsoft.EntityFrameworkCore;
using URL_Shortener.Models.Data;

namespace URL_Shortener.Services
{
    public class ShortCodeService
    {
        private readonly AppDbContext _db;
        private const string Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public ShortCodeService(AppDbContext db)
        {
            _db = db;
        }
        public async Task<string> GenerateUniqueCodeAsync() {
            string code;
            do
            {
                code = GenerateCode();
            }while(await _db.ShortenedURLs.AnyAsync(x=>x.ShortCode==code));
            return code;
        }
        private string GenerateCode()
        {
            var random=new Random();
            return new string(Enumerable.Range(0, 6)
                              .Select(_ => Chars[random.Next(Chars.Length)])
                              .ToArray());
        }

    }
}
