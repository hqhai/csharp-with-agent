// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Enums
{
    public enum EnumIntegrationStatus
    {
        /// <summary>
        /// Tạo tài khoản
        /// </summary>
        Register,

        /// <summary>
        /// Đã xác thực
        /// </summary>
        Confirm,

        /// <summary>
        /// Đang làm PT
        /// </summary>
        PTProgress,

        /// <summary>
        /// Done PT
        /// </summary>
        PTDone,

        /// <summary>
        /// Học thử
        /// </summary>
        Trial,

        /// <summary>
        /// Hết hạn học thử
        /// </summary>
        TrialExpired,

        /// <summary>
        /// Đang học
        /// </summary>
        LearningProgress,

        /// <summary>
        /// Chờ duyệt
        /// </summary>
        PendingApproval,

        /// <summary>
        /// Khóa học đã hết hạn
        /// </summary>
        Expired,

        /// <summary>
        /// Hết hạn gói trả phí
        /// </summary>
        PaidExpired,

        /// <summary>
        /// Cutoff dữ liệu
        /// </summary>
        Cutoff
    }
}
