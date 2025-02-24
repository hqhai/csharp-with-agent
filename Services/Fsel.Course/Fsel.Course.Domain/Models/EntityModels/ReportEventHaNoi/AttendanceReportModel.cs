// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi
{
    public class AttendanceReportModel
    {
        public OverallStudentModel? OverallStudent { get; set; }
        public IList<NumberStudentLearnOnSystemModel>? NumberStudentLearnOnSystem { get; set; }
        public IList<SummaryDataOnCityModel>? SummaryDataOnCity { get; set; }
    }
}
