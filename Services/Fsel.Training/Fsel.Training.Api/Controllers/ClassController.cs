// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Api.Controllers
{
    using System.Collections.Generic;
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Common.Enums;
    using Fsel.Training.Application.Commands.ClassCmd;
    using Fsel.Training.Application.Queries.ClassQuery;
    using Fsel.Training.Doman.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/class")]
    [ApiController]
    public class ClassController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClassController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// get class in status new
        /// </summary>
        [HttpGet("get-class-new")]
        [ProducesResponseType(typeof(MethodResult<IList<ClassModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetClassNew()
        {
            MethodResult<IList<ClassModel>> commandResult = await _mediator.Send(new GetClassByStatusNewQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get new class code
        /// </summary>
        [HttpGet("get-new-class-code")]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetNewClassCode([FromQuery] GetNewClassCodeQuery query)
        {
            MethodResult<string> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Create a class
        /// </summary>
        [HttpPost("create-class")]
        [ProducesResponseType(typeof(MethodResult<ClassModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateClassCommand command)
        {
            MethodResult<ClassModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
