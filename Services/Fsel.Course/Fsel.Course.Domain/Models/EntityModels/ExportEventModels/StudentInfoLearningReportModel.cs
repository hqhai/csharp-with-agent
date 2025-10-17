// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ExportEventModels
{
    using Fsel.Course.Domain.Entities;
    using Fsel.Shared.Enums;

    public class StudentInfoLearningReportModel
    {
        public string? UserName { get; set; }
        public EnumCourseLevel? SuggetLevel { get; set; }          // Trình độ Sugget PT
        public string? Target { get; set; } // chưa phát triển dữ liệu
        public int? CountCourse { get; set; } // chưa phát triển dữ liệu
        public DateTime? EstimatedDate { get; set; }// chưa phát triển dữ liệu

        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public DateTime? BirthDay { get; set; }

        public string? ParentFullName { get; set; }
        public string? ParentPhoneNumber { get; set; }
        public string? ParentEmail { get; set; }

        public string? Data { get; set; } // chưa phát triển dữ liệu
        public string? School { get; set; }
        public string? SchoolClass { get; set; }
        public string? TearchName { get; set; } // chưa phát triển dữ liệu
        public string? District { get; set; }
        public string? Province { get; set; }

        public bool NotLoggedIn { get; set; }             // Chưa đăng nhập
        public bool LoggedInButNoPT { get; set; }         // Đăng nhập mà chưa PT
        public bool PTButNotStudied { get; set; }         // PT mà chưa học
        public bool SelectedLessonButNotStudied { get; set; } // Chọn bài mà chưa học
        public int? ExpiredDaysPaid { get; set; }  //Số ngày đã hết hạn gói học, đã có order tính phí
        public EnumCourseLevel? CurrentLevel { get; set; }         // Trình độ hiện tại

        public string? Package { get; set; } // Gói học hiện tại
        public int CountOrder { get; set; } // Số Lần mua hàng
        public DateTime? StudyStartDate { get; set; } //Ngày bắt đầu học (lấy date khóa đầu tiên)
        public DateTime? ExpiredDate { get; set; }
        public DateTime? LastVisitDate { get; set; } //Lần cuối truy cập
        public int? CountRemainingDay { get; set; } //Số ngày học còn lại
        public int CounTabsentDay { get; set; } // Số ngày không vào học
        public double TokenUser { get; set; }

        public DateTime? FirstStudyDate { get; set; }     // Ngày học đầu tiên
        public string? CourseName { get; set; }
        public double CourseOverall { get; set; }
        public string? DataFMT1 { get; set; }
        public string? DataFMT2 { get; set; }
        public string? ProgressModule { get; set; }       // Học phần
        public double? ProgressPercent { get; set; }      // Phần trăm tiến độ
        public IList<WeeklyProgressModel> WeeklyResults { get; set; } = new List<WeeklyProgressModel>();
    }
}
