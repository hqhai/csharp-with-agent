// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.LmsCourseService
{
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Application.Services.LmsCourseService.Model;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface ILmsCourseService
    {
        [Post("/placement-test/admin/get-pt-point-by-ids")]
        Task<IApiResponse<MethodResult<List<StudentPTPointModel>>>> GetPTPointByIds([Body] IList<Guid>? studentIds);

        [Get("/placement-test/check-result/{studentId}")]
        Task<IApiResponse<MethodResult<bool>>> IsPlacementTestAsync([FromRoute] Guid studentId);

        [Get("/admin/lesson/{studentId}")]
        Task<IApiResponse<MethodResult<IList<StudentLessonCommentModel>>>> GetLessonCommentByStudent([FromRoute] Guid studentId);
    }
}
