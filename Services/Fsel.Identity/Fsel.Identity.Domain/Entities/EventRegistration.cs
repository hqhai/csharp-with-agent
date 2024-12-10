// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using Fsel.Common.Attributes;
    using Fsel.Shared.Constants;

    public class EventRegistration : Entity
    {
        public Guid CompetitionEventId { get; set; }

        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? FirstName { get; set; }

        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? LastName { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [RegexValid(Regex = RegexSetting.EmailValid, ErrorMessage = nameof(EnumSystemErrorCode.InValidFormat))]
        [MaxLength(254, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Email { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? PhoneNumber { get; set; }

        [MaxLength(12, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? TeacherPhoneNumber { get; set; }

        public string? ParentEmail { get; set; }

        [MaxLength(12, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? ParentPhoneNumber { get; set; }

        public bool IsBussinessCheckBox { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public DateTime BirthDay { get; set; }

        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Province { get; set; }

        public Guid? ProvinceId { get; set; }

        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? District { get; set; }

        public Guid? DistrictId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? School { get; set; }

        public Guid? SchoolId { get; set; }

        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SchoolGrade { get; set; }

        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SchoolClass { get; set; }

        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SchoolStudentCode { get; set; }

        public EnumEventRegistrationStatus Status { get; set; }
        public Guid? StudentId { get; set; }
        public Student? Student { get; set; }

        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? StudentMainMajor { get; set; }
    }
}
