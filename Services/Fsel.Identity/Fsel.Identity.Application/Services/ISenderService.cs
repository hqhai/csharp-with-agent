using Fsel.Common.ActionResults;
using Fsel.Identity.Common.Models.Entities;
using Fsel.Identity.Domain.Entities;
using Fsel.Sender.Common.Models.Commands;
using Microsoft.AspNetCore.Mvc;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Identity.Application.Services
{
    public interface ISenderService
    {
        [Post("/sender")]
        Task<IApiResponse<MethodResult<bool>>> SendEmailAsync([Body] SendEmailCommandModel command);
    }
}