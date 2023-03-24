using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Common.Enums;
using Fsel.Course.Application.Queries.CourseQuery;
using Fsel.Course.Application.Services;
using Fsel.Course.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Course.Lms.Api.Controllers
{
    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/test")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.Parent))]
    [Authorize(Roles = nameof(EnumRole.Student))]
    public class TestController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TestController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Course
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public IActionResult Search()
        {
            MethodResult<string> queryResult = new MethodResult<string> { Result = nameof(Search) };
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get List Course By Level
        /// </summary>
        [HttpGet("courses-by-level")]
        [ProducesResponseType(typeof(MethodResult<IList<CourseModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListCourseByLevel([FromQuery] GetCoursesByLevelQuery query)
        {
            MethodResult<IList<CourseModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Class in course
        /// </summary>
        [HttpGet("class-course")]
        [ProducesResponseType(typeof(MethodResult<ClassModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetClassIncourse([FromQuery] GetClassCourseQuery query)
        {
            MethodResult<ClassModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
