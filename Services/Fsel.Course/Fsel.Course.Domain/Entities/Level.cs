// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Attributes;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Entities.FlowConfigs;
    using Fsel.Course.Domain.Entities.TestConfigs;

    public class Level : Entity
    {
        [MaxLength(200, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        [RegexValid(Regex = @"^[^<>]*$", ErrorMessage = nameof(EnumSystemErrorCode.InValidFormat))]
        public string? Name { get; set; }

        [MaxLength(200, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        [RegexValid(Regex = @"^[^<>]*$", ErrorMessage = nameof(EnumSystemErrorCode.InValidFormat))]
        public string? Code { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        [RegexValid(Regex = @"^[^<>]*$", ErrorMessage = nameof(EnumSystemErrorCode.InValidFormat))]
        public string? Description { get; set; }

        public int LevelOrder { get; set; }

        public Guid ProgramId { get; set; }

        public Category? Category { get; set; }

        public ICollection<SkillLevel> SkillLevels { get; set; } = new List<SkillLevel>();
        public ICollection<StepFlow> StepFlows { get; set; } = new List<StepFlow>();
        public ICollection<PlacementTest> PlacementTests { get; set; } = new List<PlacementTest>();
        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
        public ICollection<Video> Videos { get; set; } = new List<Video>();
        public ICollection<Unit> Units { get; set; } = new List<Unit>();
        public ICollection<Test> Tests { get; set; } = new List<Test>();
        public ICollection<Course> Courses { get; set; } = new List<Course>();
        public ICollection<HomeWork> HomeWorks { get; set; } = new List<HomeWork>();

        public ICollection<TestGroupResult> TestGroupResults { get; set; } = new List<TestGroupResult>();
        public ICollection<TestGroupResult> TestGroupResultsForCurrentLevel { get; set; } = new List<TestGroupResult>();
        public ICollection<TestGroupResult> TestGroupResultsForEmailLevel { get; set; } = new List<TestGroupResult>();
        public ICollection<StudentGoalAggregate> StudentGoalAggregates { get; set; } = new List<StudentGoalAggregate>();
    }
}
