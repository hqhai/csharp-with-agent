namespace Fsel.System.Domain.Entities.BlindBoxs
{
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class BlindBoxChestConfig : Entity
    {
        public EnumBlindBoxConfigType ConfigType { get; set; }
        public string? ImagePath { get; set; }
        public int? Coin { get; set; }
        public int Percentage { get; set; }
        public Guid BlindBoxChestId { get; set; }
        public BlindBoxChest? BlindBoxChest { get; set; }
        public ICollection<BlindBoxHistory> BlindBoxHistories { get; set; } = new List<BlindBoxHistory>();
    }
}
