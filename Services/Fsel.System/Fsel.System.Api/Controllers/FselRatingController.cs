// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.System.Application.Commands.FselRatingCmd;
    using Fsel.System.Application.Queries.FselRatingQuery;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/fsel-rating")]
    [ApiController]
    [Common.Attributes.Permission(role: nameof(EnumRole.Student))]
    public class FselRatingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FselRatingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Check rating app
        /// </summary>
        [HttpGet("check-rating-app/{deviceCode}")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CheckRatingApp([FromRoute] string? deviceCode)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new CheckRatingAppQuery { DeviceCode = deviceCode }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Check rating app
        /// </summary>
        [HttpPut("rating-app")]
        [ProducesResponseType(typeof(MethodResult<FselRatingModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateRating([FromBody] CreateFselRatingCommand cmd)
        {
            MethodResult<FselRatingModel> commandResult = await _mediator.Send(cmd).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
