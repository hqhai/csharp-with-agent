namespace Fsel.System.Domain.Entities.BlindBoxs
{
    using Fsel.Core.Entities;

    public class BlindBox : Entity
    {
        public string? Name { get; set; }
        public int MileStone { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ICollection<BlindBoxUser> BlindBoxUsers { get; set; } = new List<BlindBoxUser>();
        public ICollection<BlindBoxChest> BlindBoxChests { get; set; } = new List<BlindBoxChest>();
    }
}