// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.SkillScoresConfigs
{
    using System.Text.Json.Serialization;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;

    public class SkillScores
    {
        [JsonRequired] public EnumCourseSkill Skill { get; set; }

        [JsonRequired] public double Scores { get; set; }

        /// <summary>
        /// This field is CorrectTotal in BaseResult
        /// Sum of CorrectTotal of Questions
        /// </summary>
        [JsonRequired]
        public double TotalCount { get; set; }

        /// <summary>
        /// This field is CorrectCount in BaseResult
        /// Sum of CorrectCount of Answers
        /// </summary>
        [JsonRequired]
        public double CorrectCount { get; set; }

        /// <summary>
        /// Entire questions of TestSection
        /// </summary>
        [JsonRequired]
        public double TotalQuestion { get; set; }

        /// <summary>
        /// Entire questions which had answered, equal numbers of answers
        /// </summary>
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

        public double TokenReceived { get; set; }

        public double? CorrectQuestion { get; set; }

        public Guid? SkillId { get; set; }
        public string? SkillName { get; set; }
        public string? SkillFilePath { get; set; }
    }
}
