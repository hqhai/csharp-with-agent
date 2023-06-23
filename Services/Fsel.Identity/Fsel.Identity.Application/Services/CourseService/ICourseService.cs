// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.CourseService
{
    using Fsel.Common.ActionResults;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface ICourseService
    {
        [Get("/placement-test/check-result/{studentId}")]
        Task<IApiResponse<MethodResult<bool>>> IsPlacementTestAsync([FromRoute] Guid studentId);
    }
}
