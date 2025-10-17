// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.UserService
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Application.Services.UserService.Models;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IUserService
    {
        [Get("/v1/student/get-by-user-id/{id}")]
        Task<IApiResponse<MethodResult<StudentModel>>> GetStudentByUserIdAsync([FromRoute] Guid id);

        [Post("/v1/student/get-by-user-ids")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> GetStudentsByIdsAsync([FromBody] IList<Guid> ids);

        [Put("/v1/student/update-student-class")]
        Task<IApiResponse<MethodResult<StudentModel>>> UpdateStudentByClassAsync([Body] UpdateStudentByClassIdModel command);

        [Put("/v1/student/update-student-token")]
        Task<IApiResponse<MethodResult<StudentModel>>> UpdateStudentByTokenAsync([Body] UpdateStudentByTokenModel command);

        [Post("/v1/student-trial-registration")]
        Task<IApiResponse<MethodResult<StudentRegistrationModel>>> CreateStudentTrialRegistration();

        [Put("/v1/student-trial-registration")]
        Task<IApiResponse<MethodResult<StudentRegistrationModel>>> UpdateStudentTrialRegistration([Body] UpdateStudentTrialRegistrationModel command);

        [Get("/v1/student-ranking/get-events-by-user-id")]
        Task<IApiResponse<MethodResult<IList<CompetitionEventsModel>>>> GetEventByUserId([Query] Guid? userId);

        [Get("/v1/student/get-student-by-email")]
        Task<IApiResponse<MethodResult<StudentModel?>>> GetStudentByEmail([Query] string email);

        [Put("/v1/admin-school/student/update-expired-date-for-students")]
        Task<IApiResponse<MethodResult<bool>>> UpdateExpiredDateForStudentsEvent([Body] UpdateExpiredDateForStudentsEventCommandModel model);

        [Post("/v1/student/deduct-coin-of-student")]
        Task<IApiResponse<MethodResult<bool>>> DeductCoinOfStudent([Body] DeductCoinOfStudentCommandModel model);

        [Get("/v1/user/get-role-by-user-id/{userId}")]
        Task<IApiResponse<MethodResult<string?>>> GetRoleByUserId([FromRoute] string userId);
    }
}
