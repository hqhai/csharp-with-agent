// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Shared.Enums;

    public class VoucherModel : BaseModel
    {
        public string? Code { get; set; }
        public string? CodePrefix { get; set; }
        public string? Name { get; set; }
        public int Value { get; set; }
        public int Quantity { get; set; }
        public int QuantityUsed { get; set; }
        public int RemainingQuantity { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public IList<EnumApplicableSubjectsVoucher>? ApplicableSubjects { get; set; }
        public EnumApplicableSubjectsVoucher VoucherType { get; set; }
        public EnumVoucherCategory Category { get; set; }
        public EnumVoucherSource Source { get; set; }
        public bool Status { get; set; }
        public IList<string>? ApplicableEmails { get; set; }
        public bool IsShowMyVoucher { get; set; }
        public VoucherDescription? Description { get; set; }
        public int? NumberOfChanges { get; set; }
        public bool IsActive { get; set; }
        public string? Banner { get; set; }
        public string? Duration { get; set; }
        public string? ItemStatus { get; set; }
        public IList<Guid>? PackageIds { get; set; }
        public IList<Guid>? EventIds { get; set; }
        public string? TranslationsStr { get; set; }
        public IList<EventModel>? EventModels { get; set; }
        public ICollection<VoucherTranslation>? Translations { get; set; }
        public ICollection<VoucherPackageModel>? VoucherPackages { get; set; }
    }
}
