namespace Fsel.Ordering.Application.Services.SystemService.Models
{
    public class AddUserIntoBlindBoxCommandModel
    {
        public Guid UserId { get; set; }
        public bool IsWin { get; set; }
        public int NumberOpen { get; set; }
    }
}
