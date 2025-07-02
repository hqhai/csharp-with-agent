// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Constants
{
    public static class SenderSettings
    {
        public const string HostName = "FSEL";
        public const string HostNameCareers = "no-reply@fsel.vn";
        public const string SendOtpSubject = "[LMS - FSEL] Thông báo mã OTP";
        public const string SendOtpSubjectFullName = "[LMS - FSEL] Thông báo mã OTP cho người dùng {0}";

        public const string TemplateFileName = "Resources//{0}.html";
        public const string SMSTemplateFileName = "Resources//SMS//{0}.txt";

        public const string OtpValidMinute = "{0} phút";
        public const string OtpValidDay = "{0} ngày";

        public const string SendPTResultSubject = "THÔNG BÁO KẾT QUẢ LÀM BÀI PLACEMENT TEST";

        public const string SendSurveyResultSubject = "THÔNG BÁO KẾT QUẢ KHẢO SÁT FSEL";
        public const string SendStudentCompleteUnit = "{0}_THÔNG BÁO KẾT QUẢ HỌC TẬP";
        public const string SendStudentCompleteCourse = "{0}_KẾT QUẢ HỌC TẬP CUỐI KHOÁ_{1}";

        public const string TitleUnit = "[FSEL] CHÚC MỪNG BẠN ĐÃ HOÀN THÀNH UNIT {0}!";

        public const string TitleWeekly1 = "[FSEL] CÙNG XEM KẾT QUẢ HỌC TẬP CỦA TUẦN ĐẦU TIÊN BẠN NHÉ!";
        public const string TitleWeekly2 = "[FSEL] BÁO CÁO TIẾN TRÌNH HỌC TẬP TUẦN NÀY BẠN NHÉ!";
        public const string TitleWeekly3 = "[FSEL] BẠN ƠI! BẠN CÓ QUÊN GÌ KHÔNG?";
        public const string TitleWeekly4 = "[FSEL] BẠN ƠI! FSEL VẪN ĐANG ĐỢI BẠN TRÊN PLATFORM NHÉ!";

        public const string TitlePT = "[FSEL] CÙNG XEM KẾT QUẢ ĐÁNH GIÁ ĐẦU VÀO BẠN NHÉ!";
        public const string MidCourseTitle = "[FSEL] BÁO CÁO HỌC TẬP GIỮA KHÓA";
        public const string PaymentApproval = "[FSEL] TÀI KHOẢN CỦA BẠN ĐÃ ĐƯỢC PHÊ DUYỆT";
        public const string CreateAccountFromCRM = "Chào mừng tới FSEL!";
    }
}
