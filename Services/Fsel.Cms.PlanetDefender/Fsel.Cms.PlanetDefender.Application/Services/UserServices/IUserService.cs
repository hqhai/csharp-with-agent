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
        [Get("/platform/get-students-in-platform")]
        Task<IApiResponse<MethodResult<IList<StudentInPlatformModel>>>> GetStudentsInPlatform([Query] GetStudentInPlatformQueryModel model);

        [Get("/student/get-by-user-id/{id}")]
        Task<IApiResponse<MethodResult<StudentModel>>> GetStudentByUserIdAsync([FromRoute] Guid id);

        [Post("/student/execute-list-query")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> ExecuteListStudentQueryAsync([Body] BaseQueryModel query);

        [Put("/student/update-student-token")]
        Task<IApiResponse<MethodResult<StudentModel>>> UpdateStudentByTokenAsync([Body] UpdateStudentByTokenModel command);

        [Get("/user/get-info-student-or-guest-by-student-id/{id}")]
        Task<IApiResponse<MethodResult<StudentModel>>> GetInfoStudentOrGuest([FromRoute] Guid id);
    }
}
