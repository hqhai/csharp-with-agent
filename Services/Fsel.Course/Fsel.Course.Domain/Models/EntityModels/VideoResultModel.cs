// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.IEntities;

    public class VideoResultModel : BaseResultModel, ITokenResult
    {
        public double NumberOfStars { get; set; }
        public int TokenDone { get; set; }
        public int TokenHighestStreak { get; set; }
        public int TokenSuperFire { get; set; }
        public int TokenQuestionReward { get; set; }
        public string? Feedback { get; set; }
        public Guid LessonResultId { get; set; }
        public Guid VideoId { get; set; }
        public Guid? CurrentVideoTimeCodeId { get; set; }
        public IList<VideoSkillScores>? VideoSkillScores { get; set; }
    }
}
