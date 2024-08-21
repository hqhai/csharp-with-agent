// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.LmsCourseService
{
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Application.Services.LmsCourseService.CommandModels;
    using Fsel.Identity.Application.Services.LmsCourseService.Model;
    using Fsel.Identity.Domain.Models.EntityModels;
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

        [Get("/v1/course-integration/integration-lesson-results")]
        Task<IApiResponse<MethodResult<IList<LessonResultModel>>>> GetLessonResults([FromQuery] CourseIntegrationQueryModel query);


        [Post("/v1/dashboard/active-course-result")]
        Task<IApiResponse<MethodResult<IList<Guid>>>> GetActiveCourseResultByStudentId([FromBody] ActiveCourseResultModel query);

        [Post("/v1.1/course/retake-course")]
        Task<IApiResponse<MethodResult<CourseResultModel>>> RetakeCourseAsync([FromBody] RetakeCourseResultCommandModel command);
    }
}