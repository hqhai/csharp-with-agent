// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.V1i2
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels.V1i2;
    using Fsel.Course.Lms.Application.Commands.CourseResultCmd;
    using Fsel.Course.Lms.Application.Commands.UnitResultCmd.V1i2;
    using Fsel.Course.Lms.Application.Queries.UnitQuery.V1i2;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1i2)]
    [Route(Settings.APIDefaultRoute + "/unit")]
    [Permission(role: nameof(EnumRole.Student))]
    [ApiController]
    public class UnitController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UnitController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get units by course Id
        /// </summary>
        [HttpGet("{UnitResultId}")]
        [ProducesResponseType(typeof(MethodResult<UnitDtoModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid unitResultId)
        {
            MethodResult<UnitDtoModel> commandResult = await _mediator.Send(new GetUnitQuery { UnitResultId = unitResultId }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Start Unit Result
        /// </summary>
        [HttpPost("start/{UnitResultId}")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Start([FromRoute] Guid unitResultId)
        {
            var commandResult = await _mediator.Send(new StartUnitResultCommand { UnitResultId = unitResultId }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
