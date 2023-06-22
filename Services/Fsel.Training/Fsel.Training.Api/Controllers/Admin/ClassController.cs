// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Api.Controllers.Admin
{
    using Fsel.Common.ActionResults;
    using System.Net;
    using Fsel.Common.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Training.Application.Queries.ClassQuery;
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
    }
}
