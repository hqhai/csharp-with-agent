// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lcms.Api.Controllers
{
    using System.Net;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Common.Enums;
    using Fsel.Course.Application.Queries.CategoryQuery;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/category")]
    [ApiController]
    [AllowAnonymous]
    public class CategoryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Course Levels
        /// </summary>
        [HttpGet("course-level")]
        [ProducesResponseType(typeof(MethodResult<IList<EnumCourseLevel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetEnumCourseLevelsAsync([FromQuery] EnumCourseType? courseType)
        {
            var queryResult = await _mediator.Send(new GetEnumCourseLevelQuery { CourseType = courseType }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search Course Source Data
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<string>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetEnumCourseSourceDatasAsync([FromQuery] EnumCourseSourceData courseSource)
        {
            var queryResult = await _mediator.Send(new GetEnumQuery { EnumCourseSourceData = courseSource }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
