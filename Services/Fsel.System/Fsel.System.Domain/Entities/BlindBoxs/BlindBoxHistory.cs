using Fsel.Core.Entities;

namespace Fsel.System.Domain.Entities.BlindBoxs
{
    public class BlindBoxHistory : Entity
    {
        public Guid BlindBoxChestConfigId { get; set; }
        public int? Coin { get; set; }
        public bool IsPiece { get; set; }
        public string? Code { get; set; }
        public BlindBoxChestConfig? BlindBoxChestConfig { get; set; }
    }
}
