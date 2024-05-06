// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.SenderServices
{
    using Fsel.Common.ActionResults;
    using Microsoft.AspNetCore.Http;
    using Refit;

    public interface ISenderService
    {
        [Post("/v1/send-email")]
        Task<IApiResponse<MethodResult<bool>>> SendEmailAsync([Body] SendEmailCommandModel command);

        [Multipart]
        [Post("/v1/send-email/send-with-attachments")]
        Task<IApiResponse<MethodResult<bool>>> SendEmailWithAttachments([Query] IList<string> toEmails, [Query] IList<string>? bccEmails, [Query] IList<string>? ccEmails, [Query] string? subject, [Query] string? content, [AliasAs("attachments")] IList<StreamPart>? attachments);
    }

    public class SendEmailCommandModel
    {
        [AliasAs("ToEmails")]
        public IList<string> ToEmails { get; set; } = new List<string>();

        [AliasAs("BccEmails")]
        public IList<string> BccEmails { get; set; } = new List<string>();

        [AliasAs("CcEmails")]
        public IList<string> CcEmails { get; set; } = new List<string>();

        [AliasAs("Subject")]
        public string? Subject { get; set; }

        [AliasAs("Content")]
        public string? Content { get; set; }

        [AliasAs("Files")]
        public IList<IFormFile>? Attachments { get; set; }
    }
}
