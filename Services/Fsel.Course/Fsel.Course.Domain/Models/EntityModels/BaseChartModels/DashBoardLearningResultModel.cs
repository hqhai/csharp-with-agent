// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.BaseChartModels
{
    using Fsel.Shared.Enums;

    public class DashBoardLearningResultModel
    {
        public int Percent { get; set; }
        public LearningResultChartModel? LearningResultChart { get; set; }
        public LearningResultUnitChartModel? OverallUnitChart { get; set; }
    }

    public class LearningResultChartModel : BaseChartResultModel
    {
        public IList<LearningResultDataChartModel>? LearningResultDatas { get; set; }
    }

    public class LearningResultUnitChartModel : BaseChartResultModel
    {
        public IList<UnitChartModel>? UnitCharts { get; set; }
    }

    public class UnitChartModel
    {
        public EnumCourseType CourseType { get; set; }
        public IList<DataChartModel>? DataColumns { get; set; }
    }

    public class LearningResultDataChartModel : DataChartModel
    {
        public long TotalStudentAca { get; set; }
        public long TotalStudentIELST { get; set; }
    }
}
