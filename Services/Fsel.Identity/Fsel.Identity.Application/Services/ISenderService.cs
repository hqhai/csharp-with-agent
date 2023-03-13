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
        public List<string> ToEmails { get; set; } = new List<string>();
        public List<string> BccEmails { get; set; } = new List<string>();
        public List<string> CcEmails { get; set; } = new List<string>();
        public string? Subject { get; set; }
        public string? Content { get; set; }
    }
}
