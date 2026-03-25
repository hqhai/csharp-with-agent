// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Models.EntityModels.StudentDashboard
{
    using System;

    /// <summary>
    /// Response model cho line chart - tra cuu tien do hoc tap theo tuan
    /// </summary>
    public class WeeklyProgressModel
    {
        /// <summary>
        /// Ngay bat dau tuan
        /// </summary>
        public DateTime WeekStartDate { get; set; }

        /// <summary>
        /// Ngay ket thuc tuan
        /// </summary>
        public DateTime WeekEndDate { get; set; }

        /// <summary>
        /// Thu tu tuan
        /// </summary>
        public int WeekIndex { get; set; }

        /// <summary>
        /// Tong so hoc vien trong tuan
        /// </summary>
        public int TotalStudents { get; set; }

        /// <summary>
        /// Phan tram hoc vien dat tien do trong tuan
        /// </summary>
        public double OnTrackPercentage { get; set; }

        /// <summary>
        /// Phan tram hoc vien chua dat tien do trong tuan
        /// </summary>
        public double BehindPercentage { get; set; }

        /// <summary>
        /// Phan tram hoc vien da hoan thanh muc tieu tuan
        /// </summary>
        public double CompletedPercentage { get; set; }
    }
}
