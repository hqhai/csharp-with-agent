// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Enums
{
    public enum EnumUserDeletionReason
    {
        FoundBetterApp,             // Tìm thấy ứng dụng khác tốt hơn
        AppDoesNotMeetLearningNeeds, // Ứng dụng không đáp ứng được nhu cầu học tập
        TechnicalIssuesOrBugs,      // Gặp vấn đề kỹ thuật hoặc lỗi khi sử dụng
        UnsatisfiedWithCourseContent, // Không hài lòng với nội dung khóa học
        AppTooComplexOrDifficult,   // Ứng dụng quá phức tạp hoặc khó sử dụng
        UnreasonableOrHighServiceCost, // Giá dịch vụ không hợp lý hoặc quá cao
        PoorCustomerSupport,        // Chất lượng hỗ trợ khách hàng không tốt
        CompletedLearningGoals,     // Đã hoàn thành mục tiêu học tập và không cần sử dụng nữa
        Other                       // Khác
    }
}
