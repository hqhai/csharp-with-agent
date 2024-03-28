// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.SystemService
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Application.Services.SystemService.Models;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface ISystemService
    {
        [Get("/v1/forbidden-word/get-list-forbidden-word")]
        Task<IApiResponse<MethodResult<IList<String>>>> CheckContainForbiddenWord([FromQuery] String Word);

        [Get("/v1/token-config/get-token")]
        Task<IApiResponse<MethodResult<TokenConfigModel>>> GetTokenConfigAsync([Query] GetTokenQueryModel query);
        [Post("/v1/school/get-school-by-ids")]
        Task<IApiResponse<MethodResult<IList<SchoolModel>>>> GetSchoolByIds([Body] IList<Guid> ids);
    }
}
