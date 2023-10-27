// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Constants
{
    public static class SenderSettings
    {
        public const string HostName = "FSEL";
        public const string SendOtpSubject = "[LMS - FSEL] Thông báo mã OTP";
        public const string SendOtpSubjectFullName = "[LMS - FSEL] Thông báo mã OTP cho người dùng {0}";

        public const string TemplateFileName = "Resources//{0}.html";

        public const string OtpValidMinute = "{0} phút";
        public const string OtpValidDay = "{0} ngày";

        public const string SendPTResultSubject = "THÔNG BÁO KẾT QUẢ LÀM BÀI PLACEMENT TEST";

        public const string SendSurveyResultSubject = "THÔNG BÁO KẾT QUẢ KHẢO SÁT FSEL";
        public const string SendStudentCompleteUnit = "Unit {0}_Thông báo kết quả học tập";
        public const string SendStudentCompleteCourse = "{0}_KẾT QUẢ HỌC TẬP CUỐI KHOÁ_{1}";
    }
}
