// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lcms.Api.Controllers
{
    using System.Net;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Course.Application.Queries.CategoryQuery;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Common.Models;

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
        /// Search Course Skill
        /// </summary>
        [HttpGet("course-skill")]
        [ProducesResponseType(typeof(MethodResult<IList<EnumCourseSkill>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetEnumCourseSkillsAsync([FromQuery] EnumPlacementTestLevel? placementTestLevel)
        {
            var queryResult = await _mediator.Send(new GetEnumCourseSkillQuery { PlacementTestLevel = placementTestLevel }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get All PlacementTest type and Course level
        /// </summary>
        [HttpGet("all-course-skill")]
        [ProducesResponseType(typeof(MethodResult<object>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetEnumCourseSkillsAsync()
        {
            var queryResult = await _mediator.Send(new GetAllEnumCourseSkillQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get All Course type and Course level
        /// </summary>
        [HttpGet("all-course-level")]
        [ProducesResponseType(typeof(MethodResult<object>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetEnumCourseLevelsAsync()
        {
            var queryResult = await _mediator.Send(new GetAllEnumCourseLevelQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }



        /// <summary>
        /// Search Course Source Data
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<EnumModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetEnumCourseSourceDatasAsync([FromQuery] EnumCourseSourceData courseSource)
        {
            var queryResult = await _mediator.Send(new GetEnumQuery { EnumCourseSourceData = courseSource }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
