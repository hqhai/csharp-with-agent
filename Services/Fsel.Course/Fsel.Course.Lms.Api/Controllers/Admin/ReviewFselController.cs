// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Admin
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.ReviewFselQuery;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/review-fsel/admin")]
    [ApiController]
    [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Admin), nameof(EnumRole.AdminSchool), nameof(EnumRole.CSO) })]
    public class ReviewFselController : BaseController
    {
        private readonly IMediator _mediator;

        public ReviewFselController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Review ai
        /// </summary>
        [HttpGet("review-ai")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<FeedbackClassForumAIModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchReviewAi([FromQuery] SearchFeedbackAIQuery query)
        {
            SetQuery(query);
            MethodResult<PagingItemsModel<FeedbackClassForumAIModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search student star
        /// </summary>
        [HttpGet("student-stars")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<StudentFeedbackModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchStudentStars([FromQuery] SearchStudentFeedbackAIQuery query)
        {
            SetQuery(query);
            MethodResult<PagingItemsModel<StudentFeedbackModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search Review Lesson Detail
        /// </summary>
        [HttpGet("lesson-detail")]
        [ProducesResponseType(typeof(MethodResult<ReviewLessonDetailSearchModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchReviewLessonDetail([FromQuery] SearchReviewLessonDetailQuery query)
        {
            SetQuery(query);
            MethodResult<ReviewLessonDetailSearchModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search Review Lesson With Course
        /// </summary>
        [HttpGet("lesson-with-course")]
        [ProducesResponseType(typeof(MethodResult<ReviewLessonWithCourseSearchModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchReviewLessonWithCourse([FromQuery] SearchReviewLessonWithCourseQuery query)
        {
            SetQuery(query);
            MethodResult<ReviewLessonWithCourseSearchModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search Review Lesson With Unit
        /// </summary>
        [HttpGet("lesson-with-unit")]
        [ProducesResponseType(typeof(MethodResult<ReviewLessonWithUnitSearchModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchReviewLessonWithUnit([FromQuery] SearchReviewLessonWithUnitQuery query)
        {
            SetQuery(query);
            MethodResult<ReviewLessonWithUnitSearchModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search Review Lesson With Lesson
        /// </summary>
        [HttpGet("lesson-with-lesson")]
        [ProducesResponseType(typeof(MethodResult<ReviewLessonWithLessonSearchModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchReviewLessonWithLesson([FromQuery] SearchReviewLessonWithLessonQuery query)
        {
            SetQuery(query);
            MethodResult<ReviewLessonWithLessonSearchModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search Review Teacher Rating Detail
        /// </summary>
        [HttpGet("teacher-rating-detail")]
        [ProducesResponseType(typeof(MethodResult<ReviewTeacherRatingDetailSearchModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchReviewTeacherRatingDetail([FromQuery] SearchReviewTeacherRatingDetailQuery query)
        {
            SetQuery(query);
            MethodResult<ReviewTeacherRatingDetailSearchModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search Review Teacher Rating
        /// </summary>
        [HttpGet("teacher-rating")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<ReviewTeacherRatingSearchModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchReviewTeacherRating([FromQuery] SearchReviewTeacherRatingQuery query)
        {
            SetQuery(query);
            MethodResult<PagingItemsModel<ReviewTeacherRatingSearchModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Teachers By Course
        /// </summary>
        [HttpGet("teachers-by-course")]
        [ProducesResponseType(typeof(MethodResult<IList<TeacherModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetTeachersByCourse([FromQuery] GetListTeacherByCourseQuery query)
        {
            MethodResult<IList<TeacherModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
