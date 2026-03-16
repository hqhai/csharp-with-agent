// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SenderService
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.SenderTemplates;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.Shared.Models.ShareModels.QueryModels;
    using Refit;

    public interface ISenderService
    {
        [Post("/v1/send-email")]
        Task<IApiResponse<MethodResult<bool>>> SendEmailAsync([Body] SendEmailCommandModel command);

        [Post("/v1/send-email/send-by-template")]
        Task<IApiResponse<MethodResult<bool>>> SendEmailAsync([Body] SendEmailByTemplateCommandModel command);

        [Post("/v1/send-email/get-histories-send-mail-learning-progress")]
        Task<IApiResponse<MethodResult<IList<HistorySendMailLearningProgressModel>>>> GetHistoriesSendMailLearningProgress([Body] GetHistoriesSendMailLearningProgressModel model);

        [Post("/v1/pdf/export-from-template")]
        Task<IApiResponse<MethodResult<byte[]>>> ExportPdfFromTemplate([Body] ExportPdfByTemplateCommandModel model);
    }

    public class SendEmailCommandModel
    {
        public IList<string> ToEmails { get; set; } = new List<string>();
        public IList<string> BccEmails { get; set; } = new List<string>();
        public IList<string> CcEmails { get; set; } = new List<string>();
        public string? Subject { get; set; }
        public string? Content { get; set; }
        public bool? IsCCEmail { get; set; }
        public bool? IsCCEmailDefault { get; set; }
        public IList<SendReceiverCommandModel> Receivers { get; set; } = new List<SendReceiverCommandModel>();
    }

    public class SendEmailByTemplateCommandModel : SendEmailCommandModel
    {
        public EnumSenderTemplate? Template { get; set; }
        public object? Params { get; set; }
    }
}
