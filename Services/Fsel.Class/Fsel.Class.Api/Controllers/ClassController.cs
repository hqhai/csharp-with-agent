// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Class.Api.Controllers
{
    using System.Collections.Generic;
    using System.Net;
    using Fsel.Class.Application.Queries.ClassQuery;
    using Fsel.Class.Doman.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
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
        [HttpPost("get-class-new")]
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
        [HttpPost("get-new-class-code")]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetNewClassCode([FromQuery] GetNewClassCodeQuery command)
        {
            MethodResult<string> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
