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
        public string? CourseTypesStr { get; set; }

        [NotMapped]
        public IList<EnumCourseType>? CourseTypes
        {
            get { return ConvertHelper.Deserialize<IList<EnumCourseType>>(CourseTypesStr); }
            set { CourseTypesStr = ConvertHelper.Serialize(value); }
        }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? CourseLevelsStr { get; set; }

        [NotMapped]
        public IList<EnumCourseLevel>? CourseLevels
        {
            get { return ConvertHelper.Deserialize<IList<EnumCourseLevel>>(CourseLevelsStr); }
            set { CourseLevelsStr = ConvertHelper.Serialize(value); }
        }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? PackageIdsStr { get; set; }

        [NotMapped]
        public IList<Guid>? PackageIds
        {
            get { return ConvertHelper.Deserialize<IList<Guid>>(PackageIdsStr); }
            set { PackageIdsStr = ConvertHelper.Serialize(value); }
        }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? PassportPath { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? UniversityDegreePath { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? CertificationPath { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? PoliceClearancePath { get; set; }

        public virtual User? User { get; set; }
        public Guid UserId { get; set; }
    }
}