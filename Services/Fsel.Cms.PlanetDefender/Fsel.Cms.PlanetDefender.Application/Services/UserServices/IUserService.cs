// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Services.UserServices
{
    using Fsel.Cms.PlanetDefender.Application.Services.UserServices.Models;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IUserService
    {
        [Get("/v1/platform/get-students-in-platform")]
        Task<IApiResponse<MethodResult<IList<StudentInPlatformModel>>>> GetStudentsInPlatform([Query] GetStudentInPlatformQueryModel model);

        [Get("/v1/student/get-by-user-id/{id}")]
        Task<IApiResponse<MethodResult<StudentModel>>> GetStudentByUserIdAsync([FromRoute] Guid id);

        [Post("/v1/student/execute-list-query")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> ExecuteListStudentQueryAsync([Body] BaseQueryModel query);

        [Put("/v1/student/update-student-token")]
        Task<IApiResponse<MethodResult<StudentModel>>> UpdateStudentByTokenAsync([Body] UpdateStudentByTokenModel command);

        [Get("/v1/user/get-by-student-id/{id}")]
        Task<IApiResponse<MethodResult<StudentModel>>> GetInfoStudentOrGuest([FromRoute] Guid id);
    }
}
