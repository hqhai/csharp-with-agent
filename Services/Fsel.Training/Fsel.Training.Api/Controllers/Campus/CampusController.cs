// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Api.Controllers.Campus
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Shared.Constants;
    using Fsel.Training.Application.Commands.Campus;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/campus")]
    [ApiController]
    public class CampusController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CampusController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Add students into class
        /// </summary>
        [HttpPost("add-students-into-class")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AddStudentsCampusIntoClass([FromBody] AddStudentsCampusIntoClassCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
