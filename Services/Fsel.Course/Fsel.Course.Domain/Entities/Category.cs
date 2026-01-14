// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Attributes;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Entities.FlowConfigs;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class Category : Entity
    {
        [RegexValid(Regex = @"^(?!.*[\[\]])(?i).*?\.(jpg|jpeg|png|webp|svg|heif|heic)$", ErrorMessage = nameof(EnumSystemErrorCode.InValidFormat))]
        [MaxLength(2000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Thumbnail { get; set; }

        [MaxLength(200, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [RegexValid(Regex = @"^[^<>]{1,200}$", ErrorMessage = nameof(EnumSystemErrorCode.InValidFormat))]
        public string? Name { get; set; }

        [MaxLength(200, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Code { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        [RegexValid(Regex = @"^[^<>]{0,1000}$", ErrorMessage = nameof(EnumSystemErrorCode.InValidFormat))]
        public string? Description { get; set; }

        public EnumTypeCategory Type { get; set; }

        public EnumStatus Status { get; set; }

        public Guid? ParentId { get; set; }

        public bool IsTestDefault { get; set; }
        public EnumTestMode? TestMode { get; set; }
        public Category? CategoryParent { get; set; }
        public ICollection<Category> Categorys { get; set; } = new List<Category>();
        public ICollection<Level> Levels { get; set; } = new List<Level>();
        public ICollection<CategoryTestBank> CategoryTestBanks { get; set; } = new List<CategoryTestBank>();
        public ICollection<Flow> Flows { get; set; } = new List<Flow>();
        public ICollection<PlacementTest> PlacementTests { get; set; } = new List<PlacementTest>();
        public ICollection<ClassForum> ClassForums { get; set; } = new List<ClassForum>();
        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
        public ICollection<Video> Videos { get; set; } = new List<Video>();
        public ICollection<Unit> Units { get; set; } = new List<Unit>();
        public ICollection<SubjectCondition> SubjectConditions { get; set; } = new List<SubjectCondition>();
        public ICollection<Test> Tests { get; set; } = new List<Test>();
        public ICollection<Course> Courses { get; set; } = new List<Course>();
        public ICollection<HomeWork> HomeWorks { get; set; } = new List<HomeWork>();
        public ICollection<TestGroupResult> TestGroupResults { get; set; } = new List<TestGroupResult>();
        public ICollection<StudentGoalAggregate> StudentGoalAggregates { get; set; } = new List<StudentGoalAggregate>();
    }
}
