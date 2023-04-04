// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Refit;

namespace Fsel.Identity.Application.Services
{
    public interface ISenderService
    {
        [Post("/sender")]
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
