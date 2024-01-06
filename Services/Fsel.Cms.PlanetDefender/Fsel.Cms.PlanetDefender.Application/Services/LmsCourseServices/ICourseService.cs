// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Services.LmsCourseServices
{
    using Fsel.Cms.PlanetDefender.Application.Services.LmsCourseServices.Models;
    using Fsel.Common.ActionResults;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface ICourseService
    {
        [Get("/unit-result/get-course-unit-by-user-id/{id}")]
        Task<IApiResponse<MethodResult<StudentCourseUnitModel>>> GetCourseUnitByUserId([FromRoute] Guid id);
    }
}
