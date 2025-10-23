// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class StudentSurveySummaryChartModels
    {
        public int TotalUser { get; set; }
        public IList<StudentSurveySummaryChartModel> StudentSurveySummaries { get; set; } = new List<StudentSurveySummaryChartModel>();
    }

    public class StudentSurveySummaryChartModel
    {
        public string? Question { get; set; }
        public int TotalAnswer { get; set; }
        public EnumSurveyQuestion QuestionType { get; set; }
        public IList<StudentSurveyShortAnswerModel> StudentSurveyShortAnswers { get; set; } = new List<StudentSurveyShortAnswerModel>();
        public IList<StudentSurveySummaryAnswerChartModel> StudentSurveySummaries { get; set; } = new List<StudentSurveySummaryAnswerChartModel>();
    }

    public class StudentSurveySummaryAnswerChartModel
    {
        public string? Answer { get; set; }
        public int TotalAnswer { get; set; }
        public double PercentAnswer { get; set; }
    }

    public class StudentSurveyShortAnswerModel
    {
        public string? Answer { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
