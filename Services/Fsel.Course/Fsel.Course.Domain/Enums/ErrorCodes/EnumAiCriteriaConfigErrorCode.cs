// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumAiCriteriaConfigErrorCode
    {
        /// <summary>
        /// AiCriteriaConfig is feature
        /// </summary>
        IsFeature,

        /// <summary>
        /// Cannot edit old version
        /// AiCriteriaConfig phiên bản cũ không thể được chỉnh sửa
        /// </summary>
        CannotEditOldVersion,

        /// <summary>
        /// AiCriteriaConfig is being used
        /// AiCriteriaConfig đang được sử dụng bởi ClassForum hoặc entities khác
        /// </summary>
        AiCriteriaConfigInUse,

        /// <summary>
        /// AiCriteriaConfig not found for ObjectId
        /// Không tìm thấy AICriteriaConfig cho ObjectId
        /// </summary>
        NotFoundForObjectId,

        /// <summary>
        /// AiPromptManager not found
        /// Không tìm thấy AiPromptManager
        /// </summary>
        AiPromptManagerNotFound,
    }
}
