// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using System;

    /// <summary>
    /// Model gửi từ Client qua Hub để yêu cầu phiên dịch AI response
    /// </summary>
    public class AITranslationRequestModel
    {
        public Guid ClassForumDetailResultId { get; set; }
    }

    /// <summary>
    /// Model kết quả phiên dịch trả về cho Client
    /// </summary>
    public class AITranslationResultModel
    {
        public Guid ClassForumDetailResultId { get; set; }
        public string? TranslatedContent { get; set; }
    }
}
