using Fsel.Core.Base.BaseModels;

namespace Fsel.System.Domain.Models.EntityModels
{
    public class BlindBoxModel : BaseModel
    {
        public string? Name { get; set; }
        public int MileStone { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? TotalUser { get; set; }
        public bool IsShowPopUp { get; set; }
        public bool IsReceived { get; set; }
        public IList<BlindBoxChestModel>? BlindBoxChests { get; set; }
    }
}