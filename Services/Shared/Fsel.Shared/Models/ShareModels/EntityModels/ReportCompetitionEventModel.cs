// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels.EntityModels
{
    public class ReportCompetitionEventModel
    {
        public string? DistrictName { get; set; }

        /// <summary>
        /// Số trường đăng ký tham gia
        /// </summary>
        public int NumberRegisteredSchool { get; set; }

        /// <summary>
        /// Số trường tham gia thực tế
        /// </summary>
        public int NumberActualParticipatingSchool { get; set; }

        /// <summary>
        /// Số học sinh đăng ký tài khoản hợp lệ
        /// </summary>
        public int NumberValidStudentAccount { get; set; }

        /// <summary>
        /// Số học sinh xác minh thành công
        /// </summary>
        public int NumberStudentCompleteVerify { get; set; }

        public IList<Guid>? StudentIds { get; set; }
        public IList<ReportCompetitionEventSchoolModel> ReportCompetitionEventSchools { get; set; } = new List<ReportCompetitionEventSchoolModel>();
    }
}
