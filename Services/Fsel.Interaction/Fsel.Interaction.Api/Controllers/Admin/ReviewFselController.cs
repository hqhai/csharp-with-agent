// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Api.Controllers.Admin
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Interaction.Application.Queries.ReviewFselQuery;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/review-fsel/admin")]
    [ApiController]
    [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
    public class ReviewFselController : BaseController
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
            SetQuery(query);
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
            SetQuery(query);
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
            SetQuery(query);
            MethodResult<StudentReviewSearchModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
