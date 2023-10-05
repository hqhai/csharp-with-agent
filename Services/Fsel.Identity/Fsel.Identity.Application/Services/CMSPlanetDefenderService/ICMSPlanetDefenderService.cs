// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.CMSPlanetDefenderService
{
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Application.Services.CMSPlanetDefenderService.Models;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface ICMSPlanetDefenderService
    {
        [Post("/cms-planet-defender/get-level-by-studentids")]
        Task<IApiResponse<MethodResult<IList<StudentGameInfoModel>>>> GetLevelOfStudentByStudentIds([FromBody] GetLevelOfStudentsByStudentIdsQueryModel model);
    }
}
