// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Admin
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.CourseQuery;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Asp.Versioning;
    using Fsel.Shared.Constants;

    [ApiVersion(ApiSettings.APIVersion1)][ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/admin/course")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CourseController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get List Courses by ids
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<IList<CourseModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCoursesByIds([FromBody] IList<Guid> courseIds)
        {
            MethodResult<IList<CourseModel>> queryResult = await _mediator.Send(new GetCoursesByIdsQuery { CourseIds = courseIds }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search Course
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<CourseSearchModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchCourseTimeQuery query)
        {
            MethodResult<PagingItemsModel<CourseSearchModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get course by code
        /// </summary>
        [HttpGet("get-course-by-code/{code}")]
        [ProducesResponseType(typeof(MethodResult<CourseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCourseByCode([FromRoute] string code)
        {
            MethodResult<CourseModel> queryResult = await _mediator.Send(new GetCourseByCodeQuery { CourseCode = code }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
