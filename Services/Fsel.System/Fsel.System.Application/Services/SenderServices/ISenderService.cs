// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.SenderServices
{
    using Fsel.Common.ActionResults;
    using Refit;

    public interface ISenderService
    {
        [Post("/v1/send-email")]
        Task<IApiResponse<MethodResult<bool>>> SendEmailAsync([Body] SendEmailCommandModel command);
    }

    public class SendEmailCommandModel
    {
        public IList<string> ToEmails { get; set; } = new List<string>();
        public IList<string> BccEmails { get; set; } = new List<string>();
        public IList<string> CcEmails { get; set; } = new List<string>();
        public string? Subject { get; set; }
        public string? Content { get; set; }
    }
}
