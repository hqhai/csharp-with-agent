// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Models.EntityModels.StudentDashboard
{
    /// <summary>
    /// Response model cho pie chart - tra cuu bai kiem tra dau vao
    /// </summary>
    public class EntranceExamOverviewModel
    {
        /// <summary>
        /// Tong so hoc vien
        /// </summary>
        public int TotalStudents { get; set; }

        /// <summary>
        /// Danh sach nhan cho bieu do: [Hoan thanh, Dang lam, Chua lam, Chua dang ky]
        /// </summary>
        public IList<string> Labels { get; set; } = new List<string>();

        /// <summary>
        /// Du lieu tuong ung voi labels: [soHoanThanh, soDangLam, soChuaLam, soChuaDangKy]
        /// </summary>
        public IList<int> Data { get; set; } = new List<int>();
    }
}
