using Fsel.Shared.Enums;

namespace Fsel.Ordering.Domain.Models.EntityModels.IntegrationModel
{
    public class PackageIntegrationModel
    {
        public Guid Id { get; set; }
        public EnumPackageCode? Code { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public decimal PriceMonth { get; set; }
        public int MonthNumber { get; set; }
        public int MonthBonus { get; set; }
        public int DayBonus { get; set; }
        public string? Description { get; set; }
    }
}
