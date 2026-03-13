// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.IEntities;
    using Fsel.Shared.Enums;

    public class VideoResult : BaseResult, IHighestStreak, ITokenResult
    {
        /// <summary>
        /// Số sao
        /// </summary>
        [Range(0, 5, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double NumberOfStars { get; set; }

        /// <summary>
        /// Feedback
        /// </summary>
        [MaxLength(4000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Feedback { get; set; }

        public string? VideoSkillScoresStr { get; set; }

        [NotMapped]
        public IList<VideoSkillScores>? VideoSkillScores
        {
            get
            {
                return ConvertHelper.Deserialize<IList<VideoSkillScores>>(VideoSkillScoresStr);
            }
            set { VideoSkillScoresStr = ConvertHelper.Serialize(value); }
        }

        public override double Percent { get; set; }
        public int? TokenFirstTime { get; set; }
        public int? TokenLastTime { get; set; }
        public bool IsShowToken { get; set; }
        public EnumPlaybackSpeed PlaybackSpeed { get; set; } = EnumPlaybackSpeed.Normal;
        public int? HighestStreak { get; set; }
        public int? HighestStreakSubQuestion { get; set; }
        public int? TimeCodeHighestStreak { get; set; }

        [NotMapped]
        public int TotalToken
        {
            get
            {
                return (TokenFirstTime ?? default) + (TokenLastTime ?? default);
            }
        }

        public DateTime? NewDate { get; set; }
        public DateTime? ProcessDate { get; set; }
        public DateTime? CompletionDate { get; set; }

        public Guid? CurrentVideoTimeCodeId { get; set; }
        public Guid? LessonModuleId { get; set; }
        public LessonModule? LessonModule { get; set; }
        public Guid LessonResultId { get; set; }
        public LessonResult? LessonResult { get; set; }
        public Video? Video { get; set; }
        public Guid VideoId { get; set; }
        public ICollection<VideoTimeCodeResult> VideoTimeCodeResults { get; set; } = new List<VideoTimeCodeResult>();
        public ICollection<VideoTimeCodeAnswer> VideoTimeCodeAnswers { get; set; } = new List<VideoTimeCodeAnswer>();
    }
}
