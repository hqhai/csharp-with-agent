// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi
{
    public class LearningQualityModel
    {
        public IList<TotalLearningModel>? TotalLearnings { get; set; }
        public IList<RateLearningModel>? RateLearnings { get; set; }
        public IList<TotalLearningQualityModel>? TotalLearningQualitys { get; set; }
        public IList<TotalDetailLearningQualityModel>? TotalDetailLearningQualitys { get; set; }
    }
}
