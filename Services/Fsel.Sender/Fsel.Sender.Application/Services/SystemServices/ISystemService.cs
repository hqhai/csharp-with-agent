// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Application.Services.SystemServices
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Sender.Application.Services.SystemServices.Models;
    using Refit;

    public interface ISystemService
    {
        [Get("/v1/google-sheet/get-cc-email")]
        Task<IApiResponse<MethodResult<IList<CCEmailModel>>>> GetCCEmail();
    }
}
