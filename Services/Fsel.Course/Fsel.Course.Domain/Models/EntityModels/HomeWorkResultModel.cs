// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class HomeWorkResultModel : BaseScoreResultModel
    {
        public Guid HomeWorkId { get; set; }
        public Guid LessonResultId { get; set; }
    }
}
