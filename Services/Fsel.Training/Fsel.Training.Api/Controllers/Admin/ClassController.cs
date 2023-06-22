// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Api.Controllers.Admin
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Training.Application.Commands.ClassCmd;
    using Fsel.Training.Application.Queries.ClassQuery;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/admin/class")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.Admin))]
    public class ClassController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClassController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<List<Guid>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            MethodResult<List<Guid>?> commandResult = await _mediator.Send(new GetStudentIdsInClassQuery { ClassId = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<ClassModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateClass([FromBody] CreateClassCommand command)
        {
            MethodResult<ClassModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        [HttpPost("change-status")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ChangeStatusClass([FromBody] ChangeStatusClassCommand command)
        {
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
