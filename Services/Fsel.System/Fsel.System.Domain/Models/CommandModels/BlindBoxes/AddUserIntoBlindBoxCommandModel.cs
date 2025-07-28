namespace Fsel.System.Domain.Models.CommandModels.BlindBoxes
{
    public class AddUserIntoBlindBoxCommandModel
    {
        public Guid UserId { get; set; }
        public bool IsWin { get; set; }
        public int NumberOpen { get; set; }
    }
}
