using Fsel.Core.Base.BaseModels;
using Fsel.Shared.Enums;

namespace Fsel.Ordering.Domain.Models.EntityModels.IntegrationModel
{
    public class VoucherIntegrationModel : BaseModel
    {
        public string? Code { get; set; }
        public string? CodePrefix { get; set; }
        public string? Name { get; set; }
        public EnumVoucherCategory Category { get; set; }
        public int Value { get; set; }
        public int Quantity { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? SourceName { get; set; }
        public bool IsActive { get; set; }
        public int? NumberOfChanges { get; set; }
        public Guid? UserId { get; set; }
        public EnumVoucherIntegrationStatus Status { get; set; }
        public IList<PackageIntegrationModel>? Packages { get; set; }
    }

    public enum EnumVoucherIntegrationStatus
    {
        Pending = 0,
        Active = 1,
        Redeemed = 2,
        Expired = 3,
        Cancelled = 4
    }
}
