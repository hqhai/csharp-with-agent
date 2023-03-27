// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.StudentServices
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Lms.Application.Services.StudentServices.Models;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IStudentService
    {
        [Get("/student/get-by-user-id/{id}")]
        Task<IApiResponse<MethodResult<StudentModel>>> GetStudentByUserIdAsync([FromRoute] string id);

        [Get("/student/get-student-by-class-id/{id}")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> GetStudentByClassIdAsync([Body] Guid id);

        [Put("/student/update-student-class")]
        Task<IApiResponse<MethodResult<StudentModel>>> UpdateStudentByClassAsync([Body] Guid id);
    }
}
