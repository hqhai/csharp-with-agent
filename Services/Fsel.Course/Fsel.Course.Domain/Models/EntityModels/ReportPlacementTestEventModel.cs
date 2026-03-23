// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;

    public class ReportPlacementTestEventModel
    {
        public Guid LocationId { get; set; }
        public string? LocationName { get; set; }

        /// <summary>
        /// Số trường đăng ký tham gia
        /// </summary>
        public int NumberRegisteredSchool { get; set; }

        /// <summary>
        /// Số trường tham gia thực tế
        /// </summary>
        public int NumberActualParticipatingSchool { get; set; }

        /// <summary>
        /// Tỉ lệ trường tham gia thực tế
        /// </summary>
        public double ActualSchoolParticipationRate
        {
            get
            {
                return NumberRegisteredSchool == 0 ? 0 : NumberHelper.GetPercent(NumberActualParticipatingSchool, NumberRegisteredSchool);
            }
        }

        /// <summary>
        /// Số học sinh đăng ký tài khoản hợp lệ (Import)
        /// </summary>
        public int NumberValidStudentAccount { get; set; }

        /// <summary>
        /// Số học sinh đăng ký tài khoản hợp lệ (LandingPage)
        /// </summary>
        public int NumberStudentAccountRegister { get; set; }

        /// <summary>
        /// Số học sinh đăng ký tài khoản hợp lệ
        /// </summary>
        public int TotalStudentAccount
        {
            get
            {
                return NumberValidStudentAccount + NumberStudentAccountRegister;
            }
        }

        /// <summary>
        /// Số học sinh xác thực thành công
        /// </summary>
        public int NumberStudentCompleteVerify { get; set; }

        /// <summary>
        /// Số học sinh Hoàn thành PT
        /// </summary>
        public int NumberStudentsCompletedPT { get; set; }

        /// <summary>
        /// Tỷ lệ HS Hoàn thành PT/ Số học sinh xác thực thành công
        /// </summary>
        public double CompletionRatePTVerify
        {
            get
            {
                return NumberStudentCompleteVerify == 0 ? 0 : NumberHelper.GetPercent(NumberStudentsCompletedPT, NumberStudentCompleteVerify);
            }
        }

        /// Tỷ lệ HS xác thực thành công/Đăng ký TK
        /// </summary>
        public double CompleteVerifyRate
        {
            get
            {
                return NumberValidStudentAccount == 0 ? 0 : NumberHelper.GetPercent(NumberStudentCompleteVerify, TotalStudentAccount);
            }
        }

        /// <summary>
        /// Tỷ lệ HS Hoàn thành PT/Đăng ký TK
        /// </summary>
        public double CompletionRate
        {
            get
            {
                return NumberStudentCompleteVerify == 0 ? 0 : NumberHelper.GetPercent(NumberStudentsCompletedPT, TotalStudentAccount);
            }
        }

        /// <summary>
        /// Số học sinh đang làm bài PT
        /// </summary>
        public int NumberStudentsProcessPT { get; set; }

        /// <summary>
        /// Số học sinh đã Học trong hệ thống
        /// </summary>
        public int NumberStudentToLearn { get; set; }

        public IList<ReportCourseLevelModel>? ReportCourseLevels { get; set; }
        public IList<LearningProgressLearnModel> LearningProgressLearns { get; set; } = new List<LearningProgressLearnModel>();
        public IList<ReportPlacementTestEventSchoolModel> ReportPlacementTestEventSchools { get; set; } = new List<ReportPlacementTestEventSchoolModel>();
    }

    public class ReportPlacementTestEventSchoolModel
    {
        public Guid SchoolId { get; set; }
        public string? SchoolName { get; set; }

        /// <summary>
        /// Số học sinh đăng ký tài khoản hợp lệ (Import)
        /// </summary>
        public int NumberValidStudentAccount { get; set; }

        /// <summary>
        /// Số học sinh đăng ký tài khoản hợp lệ (LandingPage)
        /// </summary>
        public int NumberStudentAccountRegister { get; set; }

        /// <summary>
        /// Số học sinh đăng ký tài khoản hợp lệ
        /// </summary>
        public int TotalStudentAccount
        {
            get
            {
                return NumberValidStudentAccount + NumberStudentAccountRegister;
            }
        }

        /// <summary>
        /// Số học sinh đang làm bài PT
        /// </summary>
        public int NumberStudentsProcessPT { get; set; }

        /// <summary>
        /// Số học sinh đã hoàn thành PT
        /// </summary>
        public int NumberStudentsCompletedPT { get; set; }

        /// <summary>
        /// Số học sinh xác thực thành công
        /// </summary>
        public int NumberStudentCompleteVerify { get; set; }

        /// Tỷ lệ HS xác thực thành công/Đăng ký TK
        /// </summary>
        public double CompleteVerifyRate
        {
            get
            {
                return TotalStudentAccount == 0 ? 0 : NumberHelper.GetPercent(NumberStudentCompleteVerify, TotalStudentAccount);
            }
        }

        /// <summary>
        /// Số học sinh đã Học trong hệ thống
        /// </summary>
        public int NumberStudentToLearn { get; set; }

        /// <summary>
        /// Tỷ lệ HS Hoàn thành PT/Đăng ký TK
        /// </summary>
        public double CompletionRate
        {
            get
            {
                return NumberStudentCompleteVerify == 0 ? 0 : NumberHelper.GetPercent(NumberStudentsCompletedPT, TotalStudentAccount);
            }
        }

        public int NumberStudentVerifiedSchool { get; set; }

        /// <summary>
        /// Số học sinh tham gia chuong trinh
        /// </summary>
        public int TotalStudentToLearn { get; set; }

        public IList<LearningProgressLearnModel> LearningProgressLearns { get; set; } = new List<LearningProgressLearnModel>();
        public IList<ReportCourseLevelModel>? ReportCourseLevels { get; set; }
    }

    public class ReportCourseLevelModel
    {
        public EnumCourseLevel CourseLevel { get; set; }
        public string? LevelCode { get; set; }
        public long TotalStudent { get; set; }
        public double Percent { get; set; }
    }
}
