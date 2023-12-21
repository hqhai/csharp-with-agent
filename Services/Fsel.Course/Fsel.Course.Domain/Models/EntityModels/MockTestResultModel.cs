// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Course.Domain.IEntities;

    public class MockTestResultModel : BaseLearnResultModel, ITokenResult
    {
        public string? FeedbackNote { get; set; }
        public int FeedbackStars { get; set; }
        public Guid MockTestId { get; set; }
        public Guid? UnitId { get; set; }
        public Guid CourseId { get; set; }
        public DateTime? GradingStartDate { get; set; }
        public Guid? GradingTeacherId { get; set; }
        public bool? IsTeacherGraded { get; set; }
        public double Scores { get; set; }
        public object? MockTestScores { get; set; }
        public int? UnitDisplayOrder { get; set; }
        public string? CourseCode { get; set; }
        public int? TokenDone { get; set; }
        public int? TokenHighestStreak { get; set; }
        public int? TokenSuperFire { get; set; }
        public int? TokenQuestionReward { get; set; }
    }
}
