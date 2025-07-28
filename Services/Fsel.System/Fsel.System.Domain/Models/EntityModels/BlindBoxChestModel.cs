using Fsel.Core.Base.BaseModels;

namespace Fsel.System.Domain.Models.EntityModels
{
    public class BlindBoxChestModel : BaseModel
    {
        public string? Name { get; set; }
        public int? MaxOpenCount { get; set; }
        public int OpenCount { get; set; }
        public string? ImagePath { get; set; }
        public int OpenPrice { get; set; }
        public int Index { get; set; }
        public bool IsActive { get; set; }
        public bool IsLast { get; set; }
        public Guid BlindBoxId { get; set; }
    }
}
