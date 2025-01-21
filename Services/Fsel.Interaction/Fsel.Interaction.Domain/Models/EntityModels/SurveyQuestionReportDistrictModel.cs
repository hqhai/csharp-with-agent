// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using Fsel.Shared.Helpers;

    public class SurveyQuestionReportDistrictModel
    {
        public string? LocationName { get; set; }
        public int NumberRegisteredSchool { get; set; }
        public int NumberActualParticipatingSchool { get; set; }

        /// <summary>
        /// Tỉ lệ trường tham gia thực tế
        /// </summary>
        public double ActualSchoolParticipationRate
        {
            get
            {
                return NumberRegisteredSchool == 0 ? 0 : NumberHelper.GetPercent(NumberActualParticipatingSchool, NumberRegisteredSchool, 2);
            }
        }

        /// <summary>
        /// Tổng Số học sinh tạo thành công
        /// </summary>
        public int NumberValidStudentAccount { get; set; }

        /// <summary>
        /// Số học sinh đã xác minh
        /// </summary>
        public int NumberStudentVerified { get; set; }

        /// <summary>
        /// Tỉ lệ HS đã xác thực
        /// </summary>
        public double PercentStudentsVerified
        {
            get
            {
                return NumberValidStudentAccount == 0 ? 0 : NumberHelper.GetPercent(NumberStudentVerified, NumberValidStudentAccount, 2);
            }
        }

        /// <summary>
        /// Số học sinh đã hoàn thành PT
        /// </summary>
        public int NumberStudentsCompletedPT { get; set; }

        /// <summary>
        /// Tỷ lệ HS Hoàn thành PT/Đăng ký TK
        /// </summary>
        public double CompletionRate
        {
            get
            {
                return NumberValidStudentAccount == 0 ? 0 : NumberHelper.GetPercent(NumberStudentsCompletedPT, NumberValidStudentAccount, 2);
            }
        }

        /// Số học sinh đã hoàn thành Survey
        /// </summary>
        public int NumberStudentsCompletedSurvey { get; set; }

        /// <summary>
        /// Tỷ lệ học sinh đã hoàn thành Survey
        /// </summary>
        public double PercentageCompletionSurvey
        {
            get
            {
                return NumberValidStudentAccount == 0 ? 0 : NumberHelper.GetPercent(NumberStudentsCompletedSurvey, NumberValidStudentAccount, 2);
            }
        }

        public IList<SurveyQuestionReportSchoolModel> SurveyQuestionReportSchools { get; set; } = new List<SurveyQuestionReportSchoolModel>();
        public IList<SurveyQuestionUserReportModel> SurveyQuestionUserReports { get; set; } = new List<SurveyQuestionUserReportModel>();
    }

    public class SurveyQuestionReportSchoolModel
    {
        public string? LocatonName { get; set; }

        /// <summary>
        /// Tổng Số học sinh tạo thành công
        /// </summary>
        public int NumberValidStudentAccount { get; set; }

        /// <summary>
        /// Số học sinh đã xác minh
        /// </summary>
        public int NumberStudentVerified { get; set; }

        /// <summary>
        /// Tỉ lệ HS đã xác thực
        /// </summary>
        public double PercentStudentsVerified
        {
            get
            {
                return NumberValidStudentAccount == 0 ? 0 : NumberHelper.GetPercent(NumberStudentVerified, NumberValidStudentAccount, 2);
            }
        }

        /// <summary>
        /// Số học sinh đã hoàn thành PT
        /// </summary>
        public int NumberStudentsCompletedPT { get; set; }

        /// <summary>
        /// Tỷ lệ HS Hoàn thành PT/Đăng ký TK
        /// </summary>
        public double CompletionRate
        {
            get
            {
                return NumberValidStudentAccount == 0 ? 0 : NumberHelper.GetPercent(NumberStudentsCompletedPT, NumberValidStudentAccount, 2);
            }
        }

        /// Số học sinh đã hoàn thành Survey
        /// </summary>
        public int NumberStudentsCompletedSurvey { get; set; }

        /// <summary>
        /// Tỷ lệ học sinh đã hoàn thành Survey
        /// </summary>
        public double PercentageCompletionSurvey
        {
            get
            {
                return NumberValidStudentAccount == 0 ? 0 : NumberHelper.GetPercent(NumberStudentsCompletedSurvey, NumberValidStudentAccount, 2);
            }
        }

        public IList<SurveyQuestionUserReportModel> SurveyQuestionUserReports { get; set; } = new List<SurveyQuestionUserReportModel>();
    }

    public class CustomerSurveyReportModel
    {
        public int Id { get; set; }
        public int DisplayLevel { get; set; }
        public float DisplayOrder { get; set; }
        public int NumberSubQuestion { get; set; }
        public double PercentSubQuestion { get; set; }
    }

    public class CustomerSurveyUserReportModel
    {
        public int Id { get; set; }
        public Guid SurveyQuestionId { get; set; }
        public int DisplayLevel { get; set; }
        public float DisplayOrder { get; set; }
        public Guid UserId { get; set; }
        public int NumberSubQuestion { get; set; }
    }

    public class SurveyQuestionUserReportModel
    {
        public int Id { get; set; }
        public Guid SurveyQuestionId { get; set; }
        public int DisplayLevel { get; set; }
        public float DisplayOrder { get; set; }
        public int TotalCount { get; set; }
        public int NumberSubQuestion { get; set; }
    }

    public class CustomerSurveyQuestionReportModel
    {
        public Guid LocationId { get; set; }
        public IList<SurveyQuestionUserReportModel> SurveyQuestionUserReports { get; set; } = new List<SurveyQuestionUserReportModel>();
    }
}
