// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Services.UserServices
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Models.ShareModels.CampusModel;
    using Fsel.Training.Application.Services.UserServices.Models;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IUserService
    {
        [Get("/v1/student/get-student-by-class-id/{id}")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> GetStudentByClassIdAsync([FromRoute] Guid id);

        [Put("/v1/student/update-student-class")]
        Task<IApiResponse<MethodResult<StudentModel>>> UpdateStudentByClassAsync([Body] UpdateStudentByClassIdModel command);

        [Get("/v1/student/get-by-user-id/{id}")]
        Task<IApiResponse<MethodResult<StudentModel>>> GetStudentByUserIdAsync([FromRoute] Guid id);

        [Get("/v1/student/get-class-has-too-many-students/{id}")]
        Task<IApiResponse<MethodResult<bool>>> GetStudentByClassIdCheckAsync([FromRoute] Guid id);

        [Put("/v1/student/delete-student-from-class/{id}")]
        Task<IApiResponse<MethodResult<bool>>> DeleteStudentFromClass([FromRoute] Guid id);

        [Post("/v1/teacher/get-by-ids")]
        Task<IApiResponse<MethodResult<IList<TeacherModel>>>> GetTeacherByIdsAsync([Body] GetTeacherByIdsQueryModel command);

        [Get("/v1/teacher/get-by-user-id/{id}")]
        Task<IApiResponse<MethodResult<TeacherModel>>> GetTeacherByUserIdAsync([FromRoute] Guid id);

        [Get("/v1/teacher/get-by-id/{id}")]
        Task<IApiResponse<MethodResult<TeacherModel>>> GetTeacherByIdAsync([FromRoute] Guid id);

        [Post("/v1/student/get-by-student-ids")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> GetStudentsByStudentIdsAsync([Body] IList<Guid> studentIds);

        [Post("/v1/admin/cso")]
        Task<IApiResponse<MethodResult<IList<HumanModel>>>> GetCSOByIds([Body] IList<Guid>? ids);

        [Get("/v1/cso/get-by-user-id/{id}")]
        Task<IApiResponse<MethodResult<CSOModel>>> GetCsoByUserIdAsync([FromRoute] Guid id);

        [Get("/v1/admin/cso/get-all")]
        Task<IApiResponse<MethodResult<IList<CSOModel>>>> GetAllCSO();

        [Get("/v1/teacher/get-all")]
        Task<IApiResponse<MethodResult<IList<TeacherModel>>>> GetAllTeacher();

        [Get("/v1/teacher/get-teachers-by-keyword/{keyword}")]
        Task<IApiResponse<MethodResult<IList<TeacherModel>>>> GetTeachersByKeyword([FromRoute] string? keyword);

        [Get("/v1/student-ranking/check-lucky-spin")]
        Task<IApiResponse<MethodResult<IList<CompetitionEventsModel>?>>> CheckLuckySpin();

        [Get("/v1/student-ranking/get-events-by-user-id")]
        Task<IApiResponse<MethodResult<IList<CompetitionEventsModel>>>> GetEventByUserId([Query] Guid? userId);

        [Get("/v1/user/get-by-student-id/{id}")]
        Task<IApiResponse<MethodResult<StudentModel>>> GetUserByStudentId([FromRoute] Guid id);

        [Post("/v1/campus/add-course-id-for-students")]
        Task<IApiResponse<MethodResult<bool>>> AddCourseIdForStudentsCampus([Body] AddCourseIdForStudentsCampusCommandModel model);

        [Post("/v1/campus/update-expired-date-for-students")]
        Task<IApiResponse<MethodResult<bool>>> UpdateExpiredDateForStudentsCampus([Body] UpdateExpiredDateForStudentsCampusCommandModels model);

        [Post("/v1/campus/update-course-id-for-students")]
        Task<IApiResponse<MethodResult<bool>>> UpdateCourseIdForStudentsCampus([Body] UpdateCourseIdOfStudentsCommandModels model);
    }
}
