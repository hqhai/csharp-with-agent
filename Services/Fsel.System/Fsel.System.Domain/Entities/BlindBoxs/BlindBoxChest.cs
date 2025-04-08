namespace Fsel.System.Domain.Entities.BlindBoxs
{
    using Fsel.Core.Entities;

    public class BlindBoxChest : Entity
    {
        public string? Name { get; set; }
        public int? MaxOpenCount { get; set; }
        public int OpenPrice { get; set; }
        public int Index { get; set; }
        public bool IsLast { get; set; }
        public Guid BlindBoxId { get; set; }
        public BlindBox? BlindBox { get; set; }
        public ICollection<BlindBoxChestConfig> BlindBoxChestConfigs { get; set; } = new List<BlindBoxChestConfig>();
    }
}
