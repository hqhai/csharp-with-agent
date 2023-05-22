// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class CSO : Entity
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? RoleLivesStr { get; set; }

        [NotMapped]
        public IList<EnumRoleLive>? RoleLives
        {
            get { return ConvertHelper.Deserialize<IList<EnumRoleLive>>(RoleLivesStr); }
            set { RoleLivesStr = ConvertHelper.Serialize(value); }
        }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? CourseLevelsStr { get; set; }

        [NotMapped]
        public object? CourseLevels
        {
            get { return ConvertHelper.Deserialize<object>(CourseLevelsStr); }
            set { CourseLevelsStr = ConvertHelper.Serialize(value); }
        }


        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? SubscriptionClassesStr { get; set; }

        [NotMapped]
        public IList<EnumSubscriptionClass>? SubscriptionClasses
        {
            get { return ConvertHelper.Deserialize<IList<EnumSubscriptionClass>>(SubscriptionClassesStr); }
            set { SubscriptionClassesStr = ConvertHelper.Serialize(value); }
        }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? PassportPath { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? UniversityDegreePath { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? CertificationPath { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? PoliceClearancePath { get; set; }

        public Human? Human { get; set; }

        public Guid HumanId { get; set; }
    }
}
