// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Shared.Enums;
using Fsel.Shared.Models.ShareModels;
using Refit;

namespace Fsel.Identity.Application.Services
{
    public interface ISenderService
    {
        [Post("/v1/send-email")]
        Task<IApiResponse<MethodResult<bool>>> SendEmailAsync([Body] SendEmailCommandModel command);

        [Post("/v1/send-email/send-by-template")]
        Task<IApiResponse<MethodResult<bool>>> SendEmailAsync([Body] SendEmailByTemplateCommandModel command);

        [Post("/v1/send-sms")]
        Task<IApiResponse<MethodResult<bool>>> SendSMSAsync([Body] SendSMSCommandModel command);

        [Post("/v1/zalo/send-sms")]
        Task<IApiResponse<MethodResult<bool>>> SendSMSWithZaloAsync([Body] SendSMSByZaloCommandModel command);
    }

    public class SendEmailCommandModel
    {
        public IList<string> ToEmails { get; set; } = new List<string>();
        public IList<string> BccEmails { get; set; } = new List<string>();
        public IList<string> CcEmails { get; set; } = new List<string>();
        public string? Subject { get; set; }
        public string? Content { get; set; }
        public bool? IsCCEmailDefault { get; set; }
    }

    public class SendEmailByTemplateCommandModel : SendEmailCommandModel
    {
        public EnumSenderTemplate? Template { get; set; }
        public object? Params { get; set; }
    }
}
