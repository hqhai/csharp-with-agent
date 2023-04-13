// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class LessonHomeworkSearchModel
    {
        public HomeWorkModel? HomeWork { get; set; }
        public long CompletedCount { get; set; }
        public long TotalCount { get; set; }
        public double Percent { get { return TotalCount > 0 ? (CompletedCount / (double)TotalCount) * 100 : default;  } }
    }
}
