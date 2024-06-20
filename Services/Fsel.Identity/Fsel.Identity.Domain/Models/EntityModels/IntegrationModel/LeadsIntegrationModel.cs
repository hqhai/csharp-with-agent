// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels.IntegrationModel
{
    using System.Text.Json.Serialization;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;

    public class LeadsIntegrationModel : IntegrationModel
    {
        public EnumIntegrationStatus? Status { get; set; }

        public string? CourseLevel { get; set; }

        public DateTime? StartTrial { get; set; }

        public DateTime? ExpireDate { get; set; }

        public string? CurrentUnit { get; set; }

        public string? CurrentLesson { get; set; }

        public int? LessonCompleted { get; set; }

        public string? StatusPT { get; set; }

        public long? AccessTime { get; set; }

        public IList<IntegrationPlacementTestResultModels>? PlacementTestResults { get; set; }
    }

    public class IntegrationPlacementTestResultModels
    {
        public EnumPlacementTestLevel Level { get; set; }

        public int CorrectCount { get; set; }

        public int CorrectTotal { get; set; }

        public IList<SkillScores>? SkillScores { get; set; }
    }

    public class SkillScores
    {
        [JsonRequired]
        public EnumCourseSkill Skill { get; set; }

        [JsonRequired]
        public double Scores { get; set; }

        [JsonRequired]
        public double TotalCount { get; set; }

        [JsonRequired]
        public double CorrectCount { get; set; }

        [JsonRequired]
        public double TotalQuestion { get; set; }

        [JsonRequired]
        public double CountQuestion { get; set; }

        private double _percent;

        [JsonRequired]
        public double Percent
        {
            get
            {
                return TotalCount > 0 ? NumberHelper.GetPercent(CorrectCount, TotalCount) : _percent;
            }
            set { _percent = TotalCount > 0 ? NumberHelper.GetPercent(CorrectCount, TotalCount) : value; }
        }
    }
}
