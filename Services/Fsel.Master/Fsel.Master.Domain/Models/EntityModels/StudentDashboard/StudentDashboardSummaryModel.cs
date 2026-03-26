// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Models.EntityModels.StudentDashboard
{
    /// <summary>
    /// Response model cho dashboard summary cards
    /// </summary>
    public class StudentDashboardSummaryModel
    {
        /// <summary>
        /// Tong so hoc vien thuc te (hoc vien co dia chi hop le va co dang ky su kien)
        /// </summary>
        public int TotalStudents { get; set; }

        /// <summary>
        /// Tong so tai khoan hoc vien (UserId distinct)
        /// </summary>
        public int TotalAccounts { get; set; }

        /// <summary>
        /// So hoc vien da hoan thanh bai kiem tra dau vao (Status = Done)
        /// </summary>
        public int CompletedEntranceExam { get; set; }

        /// <summary>
        /// So hoc vien dang lam bai kiem tra dau vao (Status = Process)
        /// </summary>
        public int InProgressEntranceExam { get; set; }

        /// <summary>
        /// So hoc vien co placement record nhung chua bat dau lam (Tong co placement - Da hoan thanh - Dang lam)
        /// </summary>
        public int NotStartedEntranceExam { get; set; }

        /// <summary>
        /// So hoc vien chua dang ky bai kiem tra dau vao
        /// </summary>
        public int NotRegisteredEntranceExam { get; set; }

        /// <summary>
        /// So hoc vien da bat dau hoc (co bai hoc da hoan thanh)
        /// </summary>
        public int StudentsEnteredLearning { get; set; }

        /// <summary>
        /// So hoc vien dat tien do (snapshot gan nhat la OnTarget)
        /// </summary>
        public int StudentsOnTrack { get; set; }

        /// <summary>
        /// So hoc vien chua dat tien do (snapshot gan nhat la BelowTarget)
        /// </summary>
        public int StudentsBehindTrack { get; set; }

        /// <summary>
        /// So hoc vien da hoan thanh khoa hoc (TotalCompleted >= TotalTarget)
        /// </summary>
        public int StudentsCompletedLearning { get; set; }

        /// <summary>
        /// So hoc vien chua hoan thanh khoa hoc
        /// </summary>
        public int StudentsNotCompletedLearning { get; set; }

        // === Ratio/Percentage fields ===

        /// <summary>
        /// Ty le tong so hoc vien thuc te (%) - tren tong so tai khoan
        /// </summary>
        public double TotalStudentsRate { get; set; }

        /// <summary>
        /// Ty le tong so tai khoan (%) - tren tong so hoc vien
        /// </summary>
        public double TotalAccountsRate { get; set; }

        /// <summary>
        /// Ty le hoan thanh bai kiem tra dau vao (%)
        /// </summary>
        public double CompletedEntranceExamRate { get; set; }

        /// <summary>
        /// Ty le dang lam bai kiem tra dau vao (%)
        /// </summary>
        public double InProgressEntranceExamRate { get; set; }

        /// <summary>
        /// Ty le chua lam bai kiem tra dau vao (%)
        /// </summary>
        public double NotStartedEntranceExamRate { get; set; }

        /// <summary>
        /// Ty le chua dang ky bai kiem tra dau vao (%)
        /// </summary>
        public double NotRegisteredEntranceExamRate { get; set; }

        /// <summary>
        /// Ty le hoc vien dat tien do (%)
        /// </summary>
        public double StudentsOnTrackRate { get; set; }

        /// <summary>
        /// Ty le hoc vien chua dat tien do (%)
        /// </summary>
        public double StudentsBehindTrackRate { get; set; }

        /// <summary>
        /// Ty le hoc vien hoan thanh khoa hoc (%)
        /// </summary>
        public double StudentsCompletedLearningRate { get; set; }

        /// <summary>
        /// Ty le hoc vien da bat dau hoc (%)
        /// </summary>
        public double StudentsEnteredLearningRate { get; set; }

        /// <summary>
        /// Ty le hoc vien chua hoan thanh khoa hoc (%)
        /// </summary>
        public double StudentsNotCompletedLearningRate { get; set; }
    }
}
