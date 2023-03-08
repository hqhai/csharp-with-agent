namespace Fsel.Identity.Common.Models.Entities
{
    public class TokenModel
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? FullName { get; set; }
        public DateTime? Expiration { get; set; }
        public List<string>? Roles { get; set; }
    }
}