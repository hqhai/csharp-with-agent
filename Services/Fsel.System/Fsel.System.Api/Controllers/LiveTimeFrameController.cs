// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Fsel.Common.ActionResults;

    using Fsel.Common.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using global::System.Net;
    using Fsel.System.Application.Querys.LiveTimeFrames;
    using Fsel.System.Application.Commands.LiveTimeFrameCmd;
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.System.Application.Querys.LiveTimeFrameQuery;

    [ApiVersion(Settings.APIVersion)]
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
            var commandResult = await _mediator.Send(new GetListLiveTimeFrameQuery { }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        ///  Get get list live time frame
        /// </summary>
        [HttpPost("get-by-ids")]
        [ProducesResponseType(typeof(MethodResult<IList<LiveTimeFrameModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListTimeFrameByIds([FromBody] GetListLiveTimeFrameByIdsQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
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
