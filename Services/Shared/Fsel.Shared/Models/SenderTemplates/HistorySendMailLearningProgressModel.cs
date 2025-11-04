// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.SenderTemplates
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class HistorySendMailLearningProgressModel : BaseModel
    {
        public string? From { get; set; }
        public string? To { get; set; }
        public string? BCC { get; set; }
        public string? CC { get; set; }
        public string? SMSId { get; set; }
        public EnumMessageHistoryType Type { get; set; }
        public EnumMessageHistoryStatus Status { get; set; }
        public string? RequestBody { get; set; }
        public string? ResponseBody { get; set; }
        public EnumSenderTemplate? Template { get; set; }
        public Guid? ReceiverId { get; set; }

        public string? TemplateStr
        {
            get
            {
                return Template.HasValue && Template.Value == EnumSenderTemplate.LearningProgressWarning ? "Mail tiến độ" : "Email cảnh báo trạng thái học tập bất ổn";
            }
        }
    }
}
