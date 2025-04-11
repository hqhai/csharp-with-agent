namespace Fsel.System.Domain.Entities.BlindBoxs
{
    using Fsel.Core.Entities;

    public class BlindBoxUser : Entity
    {
        public Guid BlindBoxId { get; set; }
        public Guid UserId { get; set; }
        public int NumberOpen { get; set; }
        public bool IsWin { get; set; }
        public bool IsShowPopUp { get; set; } = true;
        public BlindBox? BlindBox { get; set; }
    }
}
