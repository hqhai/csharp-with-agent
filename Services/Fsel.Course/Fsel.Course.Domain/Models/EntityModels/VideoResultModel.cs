// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IEntities;
    using Fsel.Shared.Enums;

    public class VideoResultModel : BaseResultModel, ITokenResult
    {
        public double NumberOfStars { get; set; }
        public int? TokenFirstTime { get; set; }
        public int? TokenLastTime { get; set; }
        public string? Feedback { get; set; }
        public Guid LessonResultId { get; set; }
        public Guid VideoId { get; set; }
        public Guid? CurrentVideoTimeCodeId { get; set; }
        public int? HighestStreak { get; set; }
        public int? TimeCodeHighestStreak { get; set; }
        public EnumPlaybackSpeed PlaybackSpeed { get; set; }
        public IList<VideoSkillScores>? VideoSkillScores { get; set; }
    }
}
