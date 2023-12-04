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
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;

    public class Student : Entity
    {
        public Guid? PackageId { get; set; }

        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Occupation { get; set; }

        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? School { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public bool CreatedByParent { get; set; }
        public long NumberOfToken { get; set; }
        public int NumberOfShield { get; set; }
        public Guid? ClassId { get; set; }

        public string? BeginnerGuides { get; set; }

        [NotMapped]
        public BeginnerGuidesModel? Config
        {
            get { return ConvertHelper.Deserialize<BeginnerGuidesModel>(BeginnerGuides); }
            set { BeginnerGuides = ConvertHelper.Serialize(value); }
        }

        public Human? Human { get; set; }

        public Guid HumanId { get; set; }

        public ICollection<ParentStudent> ParentStudents { get; set; } = new List<ParentStudent>();
        public ICollection<StudentDailyStreak> StudentDailyStreaks { get; set; } = new List<StudentDailyStreak>();
    }
}
