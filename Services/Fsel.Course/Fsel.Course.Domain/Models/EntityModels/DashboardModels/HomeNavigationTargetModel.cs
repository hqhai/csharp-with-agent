// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.DashboardModels
{
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class HomeNavigationTargetModel
    {
        public string? Name { get; set; }
        public string? Thumbnail { get; set; }
        public string? Description { get; set; }
        public string? InstructionContent { get; set; }
        public EnumLessonOverviewStatus? Status { get; set; }
        public EnumTestType? TestType { get; set; }
        public string? Type { get; set; }
        public Guid? ObjectId { get; set; }
        public Guid CourseId { get; set; }
        public Guid? UnitResultId { get; set; }
        public Guid? UnitId { get; set; }
        public Guid? VideoId { get; set; }
        public Guid? ModuleId { get; set; }
        public LessonResultModel? LessonResult { get; set; }
        public IList<SkillScores>? SkillScores { get; set; }
    }
}
