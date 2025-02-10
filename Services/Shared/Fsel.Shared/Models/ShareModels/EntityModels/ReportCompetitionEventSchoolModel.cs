// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels.EntityModels
{
    using System;
    using System.Collections.Generic;

    public class ReportCompetitionEventSchoolModel
    {
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
        /// Số học sinh xác thực thành công
        /// </summary>
        public int NumberStudentCompleteVerify { get; set; }

        public IList<Guid>? StudentIds { get; set; }
    }
}
