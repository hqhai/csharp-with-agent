// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Identity.Application.Commands.OtherCmd;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/other")]
    [ApiController]
    public class OtherController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHostEnvironment _environment;

        public OtherController(IMediator mediator, IHostEnvironment environment)
        {
            _mediator = mediator;
            _environment = environment;
        }

        /// <summary>
        /// RetakeCourse
        /// </summary>
        [HttpGet("retake-course")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission]
        public async Task<RedirectResult> RetakeCourse([FromQuery] ToolRetakeCourseResultCommand command)
        {
            await _mediator.Send(command).ConfigureAwait(false);
            if (_environment.IsProduction())
            {
                return Redirect("https://lms.fsel.edu.vn/");
            }
            else if (_environment.IsStaging())
            {
                return Redirect("https://lms-beta.fsel.edu.vn/");
            }
            else if (_environment.IsEnvironment(Settings.Environments.Testing))
            {
                return Redirect("https://lms-testing.fsel.edu.vn/");
            }
            else
            {
                return Redirect("https://lms-testing.fsel.edu.vn/");
            }
        }
    }
}
