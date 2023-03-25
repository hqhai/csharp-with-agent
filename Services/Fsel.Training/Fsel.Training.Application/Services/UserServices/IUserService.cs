// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Services.UserServices
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Application.Services.UserServices.Models;
    using Refit;

    public interface IUserService
    {
        [Post("/teacher/get-by-ids")]
        Task<IApiResponse<MethodResult<IList<TeacherModel>>>> GetTeacherByIdsAsync([Body] GetTeacherByIdsQueryModel command);

        [Post("/teacher/get-by-id/{id}")]
        Task<IApiResponse<MethodResult<TeacherModel>>> GetTeacherByIdAsync([Body] Guid id);

        [Get("/user/get-student-by-id/{id}")]
        Task<IApiResponse<MethodResult<StudentModel>>> GetStudentByIdAsync([Body] Guid id);

        [Get("/user/get-student-by-class-id/{id}")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> GetStudentByClassIdAsync([Body] Guid id);
    }
}
