// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.SystemService
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Application.Services.SystemService.Models;
    using Refit;

    public interface ISystemService
    {
        [Get("/forbidden-word/get-list-forbidden-word")]
        Task<IApiResponse<MethodResult<IList<ForbiddenWordModel>>>> GetListForbiddenWordAsync();
    }
}
