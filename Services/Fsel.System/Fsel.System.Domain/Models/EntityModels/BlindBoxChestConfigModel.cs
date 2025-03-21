namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class BlindBoxChestConfigModel : BaseModel
    {
        public EnumBlindBoxConfigType ConfigType { get; set; }
        public string? ImagePath { get; set; }
        public int? Coin { get; set; }
        public int Percentage { get; set; }
        public Guid BlindBoxChestId { get; set; }
    }
}
