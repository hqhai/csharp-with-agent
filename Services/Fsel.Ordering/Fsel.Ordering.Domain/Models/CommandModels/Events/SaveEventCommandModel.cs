// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.Events
{
    using Fsel.Shared.Enums;

    public class SaveEventCommandModel
    {
        public Guid? Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Description { get; set; }
        public IList<string>? ImagePaths { get; set; }
        public IList<SavePackageEventCommandModel>? PackageEvents { get; set; }
        public IList<SaveEventTranslationCommandModel>? Translations { get; set; }
    }

    public class SavePackageEventCommandModel
    {
        public Guid? Id { get; set; }
        public Guid PackageId { get; set; }
        public decimal Price { get; set; }
        public decimal PriceMonth { get; set; }
        public int DayBonus { get; set; }
        public int MonthBonus { get; set; }
        public EnumEventPackageStatus Status { get; set; }
        public IList<EnumPackageSuggest>? Suggests { get; set; }
    }

    public class SaveEventTranslationCommandModel
    {
        public Guid? Id { get; set; }
        public string? Language { get; set; }
        public string? Description { get; set; }
    }
}
