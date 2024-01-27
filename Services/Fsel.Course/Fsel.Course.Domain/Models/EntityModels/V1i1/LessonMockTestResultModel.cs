// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.V1i1
{
    public class LessonMockTestResultModel : BaseScoreResultModel
    {
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
        public Guid ObjectId { get; set; }
        public string? Type { get; set; }
        public bool? IsCheckScoreColor { get; set; }
        public double? TargetBandScore { get; set; }
        public bool? IsTeacherGraded { get; set; }
        public double? Scores { get; set; }
    }
}
