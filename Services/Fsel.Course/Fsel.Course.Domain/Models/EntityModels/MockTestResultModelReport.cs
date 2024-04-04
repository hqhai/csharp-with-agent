// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class MockTestResultReportModel : TestResultReportModel
    {
        public bool? IsTeacherGraded { get; set; }
        public double TargetBandScore { get; set; }
        public bool IsCheckScoreColor { get; set; }
    }
}
