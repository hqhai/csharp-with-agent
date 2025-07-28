// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers.Admins
{
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Commands.TokenConfigCmd;
    using Fsel.System.Application.Queries.TokenConfigQuery;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/admin/token-config")]
    //[Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
    [ApiController]
    public class TokenConfigController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TokenConfigController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// get list TokenConfig
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<TokenConfigModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetList([FromQuery] GetTokenConfigsByAdminQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update a token
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(MethodResult<IList<TokenConfigModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromBody] UpdateTokenConfigCommand command)
        {
            MethodResult<IList<TokenConfigModel>> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Notification
        /// </summary>
        [HttpPost("recall-coin")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SendNotificationRecallCoinSurvey()
        {
            var commandResult = await _mediator.Send(new RecallCoinSurveyCommand()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
