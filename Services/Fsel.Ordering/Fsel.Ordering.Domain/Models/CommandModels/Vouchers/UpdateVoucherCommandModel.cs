// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.Vouchers
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.AspNetCore.Http;

    public class UpdateVoucherCommandModel
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public int Value { get; set; }
        public int Quantity { get; set; }
        public bool IsShowMyVoucher { get; set; }
        public EnumVoucherCategory Category { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Banner { get; set; }
        public IFormFile? File { get; set; }

        [JsonIgnore]
        public VoucherDescription? Description => Translations?.FirstOrDefault()?.Description;

        public IList<Guid>? EventIds { get; set; }
        public IList<EnumApplicableSubjectsVoucher>? ApplicableSubjects { get; set; }
        public IList<Guid>? PackageIds { get; set; }
        public string? TranslationsJsonStr { get; set; }

        [JsonIgnore]
        public IList<CreateVoucherTranslationModel>? Translations => TranslationsJsonStr.Deserialize<IList<CreateVoucherTranslationModel>>();
    }
}
