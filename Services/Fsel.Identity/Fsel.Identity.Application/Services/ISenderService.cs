using Fsel.Common.ActionResults;
using Fsel.Sender.Common.Models.Commands;
using Refit;

namespace Fsel.Identity.Application.Services
{
    public interface ISenderService
    {
        [Post("/sender")]
        Task<IApiResponse<MethodResult<bool>>> SendEmailAsync([Body] SendEmailCommandModel command);
    }
}