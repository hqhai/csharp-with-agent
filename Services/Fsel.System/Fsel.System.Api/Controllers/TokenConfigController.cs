// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Queries.TokenConfigQuery;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
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
        /// get list tokenConfig
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<TokenConfigModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetList()
        {
            var commandResult = await _mediator.Send(new GetListTokenConfigQuery { }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get list tokenConfig
        /// </summary>
        [HttpGet("get-tokens")]
        [ProducesResponseType(typeof(MethodResult<IList<TokenConfigModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetTokenConfigs([FromQuery] GetTokenConfigsQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get token
        /// </summary>
        [HttpGet("get-token")]
        [ProducesResponseType(typeof(MethodResult<TokenConfigModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] GetTokenConfigQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
