// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels.EntityModels
{
    using System;
    using System.Collections.Generic;

    public class ReportCompetitionEventSchoolModel
    {
        public string? SchoolName { get; set; }

        /// <summary>
        /// Số học sinh đăng ký tài khoản hợp lệ
        /// </summary>
        public int NumberValidStudentAccount { get; set; }

        public IList<Guid>? StudentIds { get; set; }
    }
}
