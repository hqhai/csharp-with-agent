// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class Teacher : Entity
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

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? PassportPath { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? UniversityDegreePath { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? CertificationPath { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? PoliceClearancePath { get; set; }

        public Human? Human { get; set; }
        public TeacherBankAccount? TeacherBankAccount { get; set; }
        public Guid HumanId { get; set; }
    }
}
