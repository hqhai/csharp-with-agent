// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.UserServices
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IUserService
    {
        [Get("/student/get-by-user-id/{id}")]
        Task<IApiResponse<MethodResult<StudentModel>>> GetStudentByUserIdAsync([FromRoute] Guid id);

        [Get("/student/get-student-by-class-id/{id}")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> GetStudentByClassIdAsync([Body] Guid id);

        [Get("/student/get-class-has-too-many-students/{id}")]
        Task<IApiResponse<MethodResult<bool>>> GetStudentByClassIdCheckAsync([FromRoute] Guid id);

        [Post("/teacher/get-by-ids")]
        Task<IApiResponse<MethodResult<IList<TeacherModel>>>> GetTeacherByIdsAsync([Body] GetTeacherByIdsQueryModel command);

        [Get("/teacher/get-by-id/{id}")]
        Task<IApiResponse<MethodResult<TeacherModel>>> GetTeacherByIdAsync([Body] Guid id);

        [Get("/teacher/get-by-user-id/{id}")]
        Task<IApiResponse<MethodResult<TeacherModel>>> GetTeacherByUserIdAsync([FromRoute] Guid id);

        [Put("/student/update-student-level")]
        Task<IApiResponse<MethodResult<bool>>> UpdateStudentByLevelAsync([Body] UpdateStudentByLevelModel command);

        [Post("/student/get-by-student-ids")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> GetStudentsByStudentIdsAsync([Body] IList<Guid> studentIds);

        [Get("/cso/get-by-user-id/{id}")]
        Task<IApiResponse<MethodResult<CSOModel>>> GetCSOByUserId([FromRoute] Guid id);

        [Post("/student/get-by-user-ids")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> GetStudentByUserIdsAsync([Body] IList<string> ids);
    }
}
