// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Models.EntityModels.StudentDashboard
{
    using System;

    /// <summary>
    /// Response model cho stacked bar chart - tra cuu bai kiem tra dau vao theo tinh/thanh
    /// </summary>
    public class ProvinceEntranceExamModel
    {
        /// <summary>
        /// Id cua tinh/thanh
        /// </summary>
        public Guid ProvinceId { get; set; }

        /// <summary>
        /// Ten tinh/thanh
        /// </summary>
        public string? ProvinceName { get; set; }

        /// <summary>
        /// Tong so hoc vien trong tinh/thanh
        /// </summary>
        public int TotalStudents { get; set; }

        /// <summary>
        /// Phan tram hoc vien da hoan thanh bai kiem tra
        /// </summary>
        public double CompletedPercentage { get; set; }

        /// <summary>
        /// Phan tram hoc vien dang lam bai kiem tra
        /// </summary>
        public double InProgressPercentage { get; set; }

        /// <summary>
        /// Phan tram hoc vien chua lam bai kiem tra
        /// </summary>
        public double NotStartedPercentage { get; set; }

        /// <summary>
        /// Phan tram hoc vien chua dang ky bai kiem tra
        /// </summary>
        public double NotRegisteredPercentage { get; set; }
    }
}
