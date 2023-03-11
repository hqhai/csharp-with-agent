namespace Fsel.Core.Base
{
    public class AuthContext
    {
        public Guid CurrentUserId { get; set; }
        public string? CurrentUsername { get; set; }
        public string? CurrentFullName { get; set; }
        public string? Email { get; set; }
    }
}