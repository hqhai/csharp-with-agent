// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Api.Controllers.Admin
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Interaction.Application.Queries.ReviewFselQuery;
    using Fsel.Interaction.Domain.Models.EntityModels;
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
        /// Get Platform
        /// </summary>
        [HttpGet("plat-form")]
        [ProducesResponseType(typeof(MethodResult<StudentReviewSearchModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchPlatform([FromQuery] SearchPlatformReviewQuery query)
        {
            MethodResult<StudentReviewSearchModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Course
        /// </summary>
        [HttpGet("course")]
        [ProducesResponseType(typeof(MethodResult<CourseReviewSearchModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchCourse([FromQuery] SearchReviewCourseQuery query)
        {
            MethodResult<CourseReviewSearchModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Course Detail
        /// </summary>
        [HttpGet("course-detail")]
        [ProducesResponseType(typeof(MethodResult<StudentReviewSearchModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchCourseDetail([FromQuery] SearchReviewCourseDetailQuery query)
        {
            MethodResult<StudentReviewSearchModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
