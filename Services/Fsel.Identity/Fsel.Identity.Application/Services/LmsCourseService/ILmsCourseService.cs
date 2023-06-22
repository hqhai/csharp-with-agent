// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.LmsCourseService
{
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Application.Services.LmsCourseService.Model;
    using Refit;

    public interface ILmsCourseService
    {
        [Post("/placement-test/admin/get-pt-point-by-ids")]
        Task<IApiResponse<MethodResult<List<StudentPTPointModel>>>> GetPTPointByIds([Body] IList<Guid>? studentIds);
    }
}
