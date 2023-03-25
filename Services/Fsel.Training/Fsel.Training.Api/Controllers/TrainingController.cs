// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Api.Controllers
{
    using System.Collections.Generic;
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Training.Application.Commands.TrainingCmd;
    using Fsel.Training.Application.Queries.TrainingQuery;
    using Fsel.Training.Doman.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/class")]
    [ApiController]
    [Authorize]
    public class TrainingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TrainingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// get training in status new
        /// </summary>
        [HttpPost("get-class-new")]
        [ProducesResponseType(typeof(MethodResult<IList<TrainingModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetClassNew()
        {
            MethodResult<IList<TrainingModel>> commandResult = await _mediator.Send(new GetClassByStatusNewQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get new class code
        /// </summary>
        [HttpPost("get-new-class-code")]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetNewClassCode([FromQuery] GetNewTrainingCodeQuery command)
        {
            MethodResult<string> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Create a class
        /// </summary>
        [HttpPost("create-class")]
        [ProducesResponseType(typeof(MethodResult<TrainingModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateClassCommand command)
        {
            MethodResult<TrainingModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
