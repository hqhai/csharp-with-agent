using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Common.Enums;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Lms.Application.Commands.CourseCmd;
using Fsel.Course.Lms.Application.Queries.CourseQuery;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Course.Lms.Api.Controllers
{
    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/course")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.Student))]
    public class CourseController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CourseController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get units by course Id
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<CourseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get()
        {
            MethodResult<CourseModel> commandResult = await _mediator.Send(new GetCourseQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
