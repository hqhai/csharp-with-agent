// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi
{
    public class LearningProgressModel
    {
        public IList<TotalLearningProgressModel>? TotalLearningProgress { get; set; }
        public IList<AverageLearningProgressModel>? AverageLearningProgress { get; set; }
        public IList<UnitDoneLearningProgressModel>? UnitDoneLearningProgress { get; set; }
        public IList<LessonDoneLearningProgressModel>? LessonDoneLearningProgress { get; set; }
    }
}
