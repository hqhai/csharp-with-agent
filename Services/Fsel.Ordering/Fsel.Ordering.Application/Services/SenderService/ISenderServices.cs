// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.SenderService
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Refit;

    public interface ISenderServices
    {
        [Post("/v1/send-email")]
        Task<IApiResponse<MethodResult<bool>>> SendEmailAsync([Body] SendEmailCommandModel command);

        [Post("/v1/send-email/send-by-template")]
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
