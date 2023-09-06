// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Constants
{
    using static MassTransit.Logging.OperationName;

    public static class NotificationTemplateSetting
    {
        public const string FlagPostTemplate = "Bài viết của {0} thuộc {1} đã bị gán cờ. Vui lòng kiểm tra.";
        public const string FlagCommentTemplate = "Bình luận của {0} thuộc {1} đã bị gán cờ. Vui lòng kiểm tra.";
        public const string ChangeStatusOrderTemplate = "Bạn đã mua khóa học {0} thành công. Hãy bắt đầu học nào!";
        public const string CreateOrderTemplate = "Bận có hóa đơn khóa học mới phê duyệt. Nhấn để phê duyệt.";
    }
}
