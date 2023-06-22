// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using System.Collections.Generic;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;

    public class VideoResultModel : BaseModel
    {
        public double Percent { get; set; }
        public int CorrectCount { get; set; }
        public int CorrectTotal { get; set; }
        public EnumResultStatus Status { get; set; }
        public double NumberOfStars { get; set; }
        public string? Feedback { get; set; }
        public LessonResultModel? LessonResult { get; set; }
        public Guid LessonResultId { get; set; }
        public VideoModel? Video { get; set; }
        public Guid VideoId { get; set; }
        public Guid StudentId { get; set; }
        public IList<VideoTimeCodeAnswerModel>? VideoTimeCodeAnswers { get; set; }
    }
}
