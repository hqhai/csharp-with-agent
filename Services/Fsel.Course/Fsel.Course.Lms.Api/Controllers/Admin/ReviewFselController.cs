// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Admin
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.ReviewFselQuery;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/review-fsel/admin")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.Admin))]
    public class ReviewFselController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReviewFselController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Communication
        /// </summary>
        [HttpGet("communication")]
        [ProducesResponseType(typeof(MethodResult<ReviewCommunicationSearchModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchCommunication([FromQuery] SearchReviewCommunicationQuery query)
        {
            MethodResult<ReviewCommunicationSearchModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search Course
        /// </summary>
        [HttpGet("course")]
        [ProducesResponseType(typeof(MethodResult<ReviewCourseSearchModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchCourse([FromQuery] SearchReviewCourseQuery query)
        {
            MethodResult<ReviewCourseSearchModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search Course Detail
        /// </summary>
        [HttpGet("course-detail")]
        [ProducesResponseType(typeof(MethodResult<ReviewCourseSearchModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchCourseDetail([FromQuery] SearchReviewCourseDetailQuery query)
        {
            MethodResult<ReviewCourseSearchModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
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
            MethodResult<ReviewLessonDetailSearchModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search Review Lesson With Course
        /// </summary>
        [HttpGet("lesson-with-course")]
        [ProducesResponseType(typeof(MethodResult<ReviewLesssonWithCourseSearchModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchReviewLessonWithCourse([FromQuery] SearchReviewLesssonWithCourseQuery query)
        {
            MethodResult<ReviewLesssonWithCourseSearchModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search Review Lesson With Unit
        /// </summary>
        [HttpGet("lesson-with-unit")]
        [ProducesResponseType(typeof(MethodResult<ReviewLessonWithUnitSearchModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchReviewLessonWithUnit([FromQuery] SearchReviewLesssonWithUnitQuery query)
        {
            MethodResult<ReviewLessonWithUnitSearchModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search Review Lesson With Lesson
        /// </summary>
        [HttpGet("lesson-with-lesson")]
        [ProducesResponseType(typeof(MethodResult<ReviewLessonWithLessonSearchModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchReviewLessonWithLesson([FromQuery] SearchReviewLesssonWithLessonQuery query)
        {
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
