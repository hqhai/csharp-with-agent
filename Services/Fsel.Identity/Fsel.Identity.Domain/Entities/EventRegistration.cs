// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using Fsel.Common.Attributes;

    public class EventRegistration : Entity
    {
        public Guid CompetitionEventId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? LastName { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [RegexValid(Regex = "^(?![-])(?!.*[-]{2})(?!.*@.*\\.{2,})(?!.*@.*\\.$)[a-zA-Z0-9._+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$", ErrorMessage = nameof(EnumSystemErrorCode.InValidFormat))]
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

        public bool IsSchoolarshipAdvising { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public DateTime BirthDay { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Province { get; set; }
        public Guid? ProvinceId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? District { get; set; }
        public Guid? DistrictId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? School { get; set; }

        public Guid? SchoolId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SchoolGrade { get; set; }


        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SchoolClass { get; set; }

        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SchoolStudentCode { get; set; }


        public EnumEventRegistrationStatus Status { get; set; }
    }
}
