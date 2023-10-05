// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;

    public class VideoResultModel : BaseResultModel
    {
        public double NumberOfStars { get; set; }
        public string? Feedback { get; set; }
        public Guid LessonResultId { get; set; }
        public Guid VideoId { get; set; }
        public Guid StudentId { get; set; }
        public Guid? CurrentVideoTimeCodeId { get; set; }
        public IList<VideoSkillScores>? VideoSkillScores { get; set; }
    }
}
