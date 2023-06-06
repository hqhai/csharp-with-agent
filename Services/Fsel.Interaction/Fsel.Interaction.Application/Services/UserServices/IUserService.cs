// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.UserServices.Models
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
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

        [Post("/teacher/get-by-id/{id}")]
        Task<IApiResponse<MethodResult<TeacherModel>>> GetTeacherByIdAsync([Body] Guid id);

        [Put("/student/update-student-level")]
        Task<IApiResponse<MethodResult<bool>>> UpdateStudentByLevelAsync([Body] UpdateStudentByLevelModel command);

        [Get("/student/get-by-ids")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> GetStudentByUserIdsAsync([FromRoute] IList<Guid> ids);
    }
}
