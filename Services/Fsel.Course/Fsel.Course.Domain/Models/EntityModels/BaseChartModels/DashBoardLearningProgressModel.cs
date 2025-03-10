// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.BaseChartModels
{
    public class DashBoardLearningProgressModel
    {
        public long TotalStudentOnSchedule { get; set; }
        public long TotalStudentBehindSchedule { get; set; }
        public LearningProgressChartModel? LearningProgressChart { get; set; }
        public BaseChartResultModel? OverallLearningProgress { get; set; }
    }

    public class LearningProgressChartModel : BaseChartResultModel
    {
        public IList<DataPieChartModel>? LearningProgressDatas { get; set; }
    }
}
