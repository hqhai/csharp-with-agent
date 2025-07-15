// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.LmsCourseService
{
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Application.Services.LmsCourseService.CommandModels;
    using Fsel.Identity.Application.Services.LmsCourseService.Model;
    using Fsel.Identity.Application.Services.LmsCourseService.QueryModels;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface ILmsCourseService
    {
        [Post("/v1/placement-test/admin/get-pt-point-by-ids")]
        Task<IApiResponse<MethodResult<List<StudentPTPointModel>>>> GetPTPointByIds([Body] IList<Guid>? studentIds);

        [Get("/v1/placement-test/check-result/{studentId}")]
        Task<IApiResponse<MethodResult<bool>>> IsPlacementTestAsync([FromRoute] Guid studentId);

        [Get("/v1/placement-test/count-result/{studentId}")]
        Task<IApiResponse<MethodResult<int>>> CountResultByStudentId([FromRoute] Guid studentId);

        [Get("/v1/admin/lesson/{studentId}")]
        Task<IApiResponse<MethodResult<IList<StudentLessonCommentModel>>>> GetLessonCommentByStudent([FromRoute] Guid studentId);

        [Get("/v1/dashboard/leader-board")]
        Task<IApiResponse<MethodResult<LeaderBoardSearchModel>>> GetLeaderBoard();

        [Get("/v1/admin/course/get-course-by-code/{code}")]
        Task<IApiResponse<MethodResult<CourseModel>>> GetCourseByCode([FromRoute] string code);

        [Get("/v1/course/get-course-studied")]
        Task<IApiResponse<MethodResult<CourseModel>>> GetCourseStudied();

        [Delete("/v1/admin/student/delete-student/{id}")]
        Task<IApiResponse<MethodResult<bool>>> DeleteListDataUser([FromRoute] Guid id);

        [Post("/v1/progress/students-competition")]
        Task<IApiResponse<MethodResult<IList<CompetitionStudentProgressModel>>>> GetStudentProgress([FromQuery] StudentCompetitionStatQueryModel query);

        [Get("/v1/admin/course/{id}")]
        Task<IApiResponse<MethodResult<CourseModel>>> GetCourseByIdAsync([FromRoute] Guid id);

        [Post("/v1/placement-test/admin/save-done")]
        Task<IApiResponse<MethodResult<bool>>> SavePlacementTestDoneAsync([FromBody] SavePlacementTestDoneCommandModel command);

        [Post("/v1/course-integration/integration-placement-test-results")]
        Task<IApiResponse<MethodResult<IList<PlacementTestResultModel>>>> GetPalcementTestResults([FromBody] CourseIntegrationQueryModel query);

        [Post("/v1/course-integration/integration-unit-results")]
        Task<IApiResponse<MethodResult<IList<UnitResultModel>>>> GetUnitResults([FromBody] CourseIntegrationQueryModel query);

        [Post("/v1/dashboard/active-course-result")]
        Task<IApiResponse<MethodResult<IList<Guid>>>> GetActiveCourseResultByStudentId([FromBody] ActiveCourseResultModel query);

        [Post("/v1.1/course/retake-course")]
        Task<IApiResponse<MethodResult<CourseResultModel>>> RetakeCourseAsync([FromBody] RetakeCourseResultCommandModel command);

        [Get("/v1.1/course/get-course-by-level/{courseLevel}")]
        Task<IApiResponse<MethodResult<CourseModel>>> GetCourseByLevelAsync([FromRoute] EnumCourseLevel courseLevel);

        [Get("/v1.1/course/get-courses-by-levels")]
        Task<IApiResponse<MethodResult<IList<CourseModel>>>> GetCoursesByLevelsAsync([FromQuery] GetCoursesByCourseLevelsQueryModel query);

        [Post("/v1.1/admin/course")]
        Task<IApiResponse<MethodResult<IList<CourseModel>>>> GetCoursesByIdsAsync([FromBody] IList<Guid> courseIds);

        [Post("/v1.1/admin/course")]
        Task<IApiResponse<MethodResult<IList<CourseModel>>>> Get([FromBody] IList<Guid> courseIds);

        [Post("/v1.1/admin/other/param-beginner-guide")]
        Task<IApiResponse<MethodResult<IList<ParamBeginnerGuideModel>>>> GetParamBeginnerGuide([FromBody] IList<Guid> studentIds);

        [Post("/v1/admin/student/aggregate-data-students")]
        Task<IApiResponse<MethodResult<AggregateDataStudentsByAdminModels>>> AggregateDataStudents([FromBody] AggregateDataStudentsByAdminQueryModels students);

        [Post("/v1/report/aggregate-data-students-in-event")]
        Task<IApiResponse<MethodResult<IList<AggregateDataLearnStudentsInEventModel>>>> AggregateDataStudentsInEvent([Body] AggregateDataLearnStudentsInEventQueryModel students);

        [Delete("/v1/admin/other/{studentId}/pt-and-course")]
        Task<IApiResponse<MethodResult<bool>>> DeletePTAndCourseAsync([FromRoute] Guid studentId);
    }
}
