// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Shared.Constants;
    using Fsel.System.Application.Queries.SenderConfigQuery;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/sender-config")]
    [ApiController]
    public class SenderConfigController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SenderConfigController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Sender Configs
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<SenderConfigModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetSenderConfigs()
        {
            MethodResult<IList<SenderConfigModel>> commandResult = await _mediator.Send(new GetSenderConfigsQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Sender Config By Id
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<SenderConfigModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetSenderConfigById([FromRoute] Guid id)
        {
            MethodResult<SenderConfigModel> commandResult = await _mediator.Send(new GetSenderConfigByIdQuery { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
