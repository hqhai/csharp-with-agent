// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Lms.Application.Queries.CourseQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/integration")]
    [ApiController]
    public class IntegrationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public IntegrationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// integration-course
        /// </summary>
        [HttpPost("integration-course")]
        [ProducesResponseType(typeof(MethodResult<List<IList<object>>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUnitSkillDiagram([FromBody] IList<Guid> userIds)
        {
            var commandResult = await _mediator.Send(new GetCourseToIntegrationQuery { UserIds = userIds }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
