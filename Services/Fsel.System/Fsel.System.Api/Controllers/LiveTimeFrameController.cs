// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.System.Application.Commands.LiveTimeFrameCmd;
    using Fsel.System.Application.Querys.LiveTimeFrameQuery;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Asp.Versioning;
    using Fsel.Shared.Constants;

    [ApiVersion(ApiSettings.APIVersion1)][ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/live-time-frame")]
    [ApiController]
    public class LiveTimeFrameController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LiveTimeFrameController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get get list live time frame
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<LiveTimeFrameModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get()
        {
            MethodResult<IList<LiveTimeFrameModel>> commandResult = await _mediator.Send(new GetListLiveTimeFrameQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Save list live time frame
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<IList<LiveTimeFrameModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SaveList([FromBody] SaveListLiveTimeFrameCommand command)
        {
            MethodResult<IList<LiveTimeFrameModel>> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
