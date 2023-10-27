// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Services.UserServices
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Training.Application.Services.UserServices.Models;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IUserService
    {
        [Get("/student/get-student-by-class-id/{id}")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> GetStudentByClassIdAsync([FromRoute] Guid id);

        [Put("/student/update-student-class")]
        Task<IApiResponse<MethodResult<StudentModel>>> UpdateStudentByClassAsync([Body] UpdateStudentByClassIdModel command);

        [Get("/student/get-by-user-id/{id}")]
        Task<IApiResponse<MethodResult<StudentModel>>> GetStudentByUserIdAsync([FromRoute] Guid id);

        [Get("/student/get-class-has-too-many-students/{id}")]
        Task<IApiResponse<MethodResult<bool>>> GetStudentByClassIdCheckAsync([FromRoute] Guid id);

        [Put("/student/delete-student-from-class/{id}")]
        Task<IApiResponse<MethodResult<bool>>> DeleteStudentFromClass([FromRoute] Guid id);

        [Post("/teacher/get-by-ids")]
        Task<IApiResponse<MethodResult<IList<TeacherModel>>>> GetTeacherByIdsAsync([Body] GetTeacherByIdsQueryModel command);

        [Get("/teacher/get-by-user-id/{id}")]
        Task<IApiResponse<MethodResult<TeacherModel>>> GetTeacherByUserIdAsync([FromRoute] Guid id);

        [Get("/teacher/get-by-id/{id}")]
        Task<IApiResponse<MethodResult<TeacherModel>>> GetTeacherByIdAsync([FromRoute] Guid id);

        [Post("/student/get-by-student-ids")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> GetStudentsByStudentIdsAsync([Body] IList<Guid> studentIds);

        [Post("/cso/admin")]
        Task<IApiResponse<MethodResult<IList<HumanModel>>>> GetCSOByIds([Body] IList<Guid>? ids);

        [Get("/cso/get-by-user-id/{id}")]
        Task<IApiResponse<MethodResult<CSOModel>>> GetCsoByUserIdAsync([FromRoute] Guid id);

        [Get("/cso/admin/get-all")]
        Task<IApiResponse<MethodResult<IList<CSOModel>>>> GetAllCSO();

        [Get("/teacher/get-all")]
        Task<IApiResponse<MethodResult<IList<TeacherModel>>>> GetAllTeacher();

        [Get("/teacher/get-teachers-by-keyword/{keyword}")]
        Task<IApiResponse<MethodResult<IList<TeacherModel>>>> GetTeachersByKeyword([FromRoute] string? keyword);
    }
}
