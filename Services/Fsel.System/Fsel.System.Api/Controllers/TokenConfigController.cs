// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Shared.Constants;
    using Fsel.System.Application.Commands.TokenConfigCmd;
    using Fsel.System.Application.Queries.TokenConfigQuery;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/token-config")]
    [ApiController]
    public class TokenConfigController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TokenConfigController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// get list blog
        /// </summary>
        [HttpGet("get-list-token")]
        [ProducesResponseType(typeof(MethodResult<IList<TokenConfigModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetList()
        {
            var commandResult = await _mediator.Send(new GetListTokenConfigQuery { }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update a token
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MethodResult<TokenConfigModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateTokenConfigCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = id;
            MethodResult<TokenConfigModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
