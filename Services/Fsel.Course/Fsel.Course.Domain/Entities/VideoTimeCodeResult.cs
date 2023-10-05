// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    public class VideoTimeCodeResult : BaseResultScore
    {
        public long WorkingTime { get; set; }

        public VideoResult? VideoResult { get; set; }
        public Guid VideoResultId { get; set; }

        public VideoTimeCode? VideoTimeCode { get; set; }
        public Guid VideoTimeCodeId { get; set; }

        public ICollection<VideoTimeCodeAnswer> VideoTimeCodeAnswers { get; set; } = new List<VideoTimeCodeAnswer>();
    }
}
