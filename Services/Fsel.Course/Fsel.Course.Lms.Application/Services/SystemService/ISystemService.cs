// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Shared.Enums;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface ISystemService
    {
        [Post("/course-time-config/execute-list-query")]
        Task<IApiResponse<MethodResult<IList<CourseTimeConfigModel>>>> CourseTimeConfigQueryAsync([Body] BaseQueryModel query);

        [Get("/course-time-config")]
        Task<IApiResponse<MethodResult<PagingItemsModel<CourseTimeConfigModel>>>> GetCourseTimeConfigAsync();

        [Get("/log-action/{userId}")]
        Task<IApiResponse<MethodResult<LogActionDaysModel>>> GetLogActionsByUserId([FromRoute] Guid userId);

        [Post("/log-action")]
        Task<IApiResponse<MethodResult<IList<LogActionDaysModel>>>> GetLogActionsByUserIdsAsync([FromBody] IList<Guid> ids);

        [Post("/feature-access-time/gets")]
        Task<IApiResponse<MethodResult<IList<FeatureAccessTimeModel>>>> GetFeatureAccessTimesAsync([FromBody] FeatureAccessTimesQueryModel query);

        [Get("/feature-access-time/get-detail")]
        Task<IApiResponse<MethodResult<FeatureAccessTimeModel>>> GetFeatureAccessTimeAsync([FromQuery] FeatureAccessTimeQueryModel query);

        [Get("/forbidden-word/get-list-forbidden-word")]
        Task<IApiResponse<MethodResult<IList<string>>>> CheckContainForbiddenWord([FromQuery] string Word);

        [Get("/student/quest-board/get-list-by-category/{packageId}/{categoryBoards}")]
        Task<IApiResponse<MethodResult<IList<QuestBoardModel>>>> GetListQuestBoardQuery([FromRoute] Guid packageId, string categoryBoards);


        [Post("/student/quest-board/taking-mission/{questBoardId}")]
        Task<IApiResponse<MethodResult<bool>>> CreateQuestBoardStudent([FromRoute] Guid questBoardId);

        [Put("/student/quest-board/update-achieved-points/{questBoardStudent}")]
        Task<IApiResponse<MethodResult<bool>>> UpdateQuestBoardStudentCommand([FromRoute] Guid questBoardStudent);


        [Get("/student/quest-board/get-list-quest-board-student")]
        Task<IApiResponse<MethodResult<IList<QuestBoardStudentModel>>>> GetListQuestBoardStudent([FromRoute] GetListQuestBoardStudentModel query);

    }
}
