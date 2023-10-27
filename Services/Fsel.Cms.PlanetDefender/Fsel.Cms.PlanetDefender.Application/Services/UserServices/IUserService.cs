// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Services.UserServices
{
    using Fsel.Cms.PlanetDefender.Application.Services.UserServices.Models;
    using Fsel.Common.ActionResults;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IUserService
    {
        [Get("/platform/get-students-in-platform")]
        Task<IApiResponse<MethodResult<IList<StudentInPlatformModel>>>> GetStudentsInPlatform([Query] GetStudentInPlatformQueryModel model);

        [Get("/student/get-by-user-id/{id}")]
        Task<IApiResponse<MethodResult<StudentModel>>> GetStudentByUserIdAsync([FromRoute] Guid id);
    }
}
