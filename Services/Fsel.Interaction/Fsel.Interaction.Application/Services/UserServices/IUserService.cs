// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.UserServices
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Interaction.Application.Services.UserServices.Models;
    using Fsel.Interaction.Application.Services.UserServices.QueryModels;
    using Fsel.Shared.Models.ShareModels.EntityModels;
    using Fsel.Shared.Models.ShareModels.QueryModels;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IUserService
    {
        [Post("/v1/student/get-by-user-ids")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> GetStudentByUserIdsAsync([Body] IList<Guid> ids);

        [Get("/v1/student/get-by-user-id/{id}")]
        Task<IApiResponse<MethodResult<StudentModel>>> GetStudentByUserIdAsync([FromRoute] Guid id);

        [Post("/v1/student/get-by-student-ids")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> GetStudentsByStudentIdsAsync([Body] IList<Guid> studentIds);

        [Post("/v1/user/get-users-by-ids")]
        Task<IApiResponse<MethodResult<IList<UserModel>>>> GetUsersByIdsAsync([Body] GetUsersByIdsQueryModel model);

        [Get("/v1/user/get-user-by-id")]
        Task<IApiResponse<MethodResult<UserModel>>> GetUserByIdAsync([Query] string? id);

        [Get("/v1/student/execute-list-query")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> ExecuteListStudentQueryAsync([Query] BaseQueryModel query);

        [Put("/v1/student/update-student-token")]
        Task<IApiResponse<MethodResult<StudentModel>>> UpdateStudentByTokenAsync([Body] UpdateStudentByTokenModel command);

        [Get("/v1/student-ranking/get-events-by-user-id")]
        Task<IApiResponse<MethodResult<IList<CompetitionEventsModel>>>> GetEventByUserId([Query] Guid? userId);

        [Get("/v1/event/get-event-parent/{id}")]
        Task<IApiResponse<MethodResult<Guid>>> GetParentEventId([FromRoute] Guid? id);

        [Get("/v1/admin/other/report-competition-event")]
        Task<IApiResponse<MethodResult<IList<ReportCompetitionEventModel>>>> GetReportCompetitionEventAsync([Query] GetReportCompetitionEventQueryModel query);

        [Get("/v1/admin/other/report-competition-event-school")]
        Task<IApiResponse<MethodResult<IList<ReportCompetitionEventModel>>>> GetReportCompetitionEventSchoolAsync([Query] GetReportCompetitionEventQueryModel query);

        [Get("/v1/event/get-events-by-event-code-str")]
        Task<IApiResponse<MethodResult<IList<CompetitionEventsModel>>>> GetEventToEventCodeStrAsync([Query] GetReportCompetitionEventQueryModel query);

        [Post("/v1/admin/student/search-students-by-user-ids")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> SearchStudentsByUserIds([Body] SearchStudentsByUserIdsQueryModel model);
    }
}
