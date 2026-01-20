// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Identity.Domain.Entities.BeginnerGuideConfigs;
    using Fsel.Identity.Domain.Entities.Campus;
    using Fsel.Shared.Enums;

    public class Student : Entity
    {
        public Guid? PackageId { get; set; }

        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Occupation { get; set; }

        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? School { get; set; }

        [MaxLength(100, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SchoolGrade { get; set; }

        [MaxLength(100, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SchoolClass { get; set; }

        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SchoolFaculty { get; set; }

        [MaxLength(20, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? ParentPhoneNumber { get; set; }

        [MaxLength(70, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? ParentEmail { get; set; }

        public EnumCourseLevel? CourseLevel { get; set; }
        public EnumCourseLevel? BaseCourseLevel { get; set; }
        public Guid? LevelId { get; set; }
        public Guid? ProgramId { get; set; }
        public Guid? SubjectId { get; set; }

        public bool CreatedByParent { get; set; }
        public long NumberOfToken { get; set; }
        public long NumberOfTokenReceived { get; set; }
        public long NumberOfTokenExchanged { get; set; }
        public int NumberOfShield { get; set; }
        public Guid? ClassId { get; set; }

        public string? BeginnerGuideStr { get; set; }

        [NotMapped]
        public StudentBeginnerGuide? BeginnerGuide
        {
            get { return ConvertHelper.Deserialize<StudentBeginnerGuide>(BeginnerGuideStr); }
            set { BeginnerGuideStr = ConvertHelper.Serialize(value); }
        }

        public Guid? CourseId { get; set; }
        public Guid? ProvinceId { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? SchoolId { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public Guid? SchoolClassId { get; set; }

        public string? StudentCampusCode { get; set; }

        public string? ClassCampusCode { get; set; }

        public SchoolClass? SchoolClassCampus { get; set; }

        public EnumStatusStudentCampus? StatusStudentCampus { get; set; }

        public virtual User? User { get; set; }
        public Guid UserId { get; set; }

        public EventRegistration? EventRegistration { get; set; }
        public ICollection<ParentStudent> ParentStudents { get; set; } = new List<ParentStudent>();
        public ICollection<StudentDailyStreak> StudentDailyStreaks { get; set; } = new List<StudentDailyStreak>();
    }
}
