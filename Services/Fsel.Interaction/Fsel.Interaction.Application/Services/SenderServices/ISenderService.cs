// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.SenderServices
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Refit;

    public interface ISenderService
    {
        [Post("/send-email")]
        Task<IApiResponse<MethodResult<bool>>> SendEmailAsync([Body] SendEmailCommandModel command);

        [Post("/send-email/send-by-template")]
        Task<IApiResponse<MethodResult<bool>>> SendEmailAsync([Body] SendEmailByTemplateCommandModel command);
    }

    public class SendEmailCommandModel
    {
        public IList<string> ToEmails { get; set; } = new List<string>();
        public IList<string> BccEmails { get; set; } = new List<string>();
        public IList<string> CcEmails { get; set; } = new List<string>();
        public string? Subject { get; set; }
        public string? Content { get; set; }
    }

    public class SendEmailByTemplateCommandModel : SendEmailCommandModel
    {
        public EnumSenderTemplate? Template { get; set; }
        public object? Params { get; set; }
    }
}
