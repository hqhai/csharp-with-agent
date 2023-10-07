// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Identity.Application.Services.SystemService.Model;
using Refit;

namespace Fsel.Identity.Application.Services.SystemService
{
    public interface ISystemService
    {
        [Get("/focus-time-config")]
        Task<IApiResponse<MethodResult<FocusTimeConfigModel>>> GetFocusTimeConfig();
    }

}
