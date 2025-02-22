// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi
{
    public class LearningProgressModel
    {
        public IList<TotalLearningProgressModel>? TotalLearningProgress { get; set; }
        public IList<AverageLearningProgressModel>? AverageLearningProgress { get; set; }
        public IList<UnitDoneLearningProgressIeltsModel>? UnitDoneLearningProgressIelts { get; set; }
        public IList<UnitDoneLearningProgressAcademicModel>? UnitDoneLearningProgressAcademics { get; set; }
        public IList<LessonDoneLearningProgressIeltsModel>? LessonDoneLearningProgressIelts { get; set; }
        public IList<LessonDoneLearningProgressAcademicModel>? LessonDoneLearningProgressAcademics { get; set; }
    }
}
