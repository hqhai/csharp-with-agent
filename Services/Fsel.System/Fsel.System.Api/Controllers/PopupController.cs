// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.System.Application.Commands.PopupCmd;
    using Fsel.System.Application.Queries.QuestBoardQuery;
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.System.Domain.Models.QueryModels;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/pop-up")]
    [ApiController]

    public class PopupController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PopupController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get pop-up maintain
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<GetPopUpMaintainQueryModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPopUpMaintain()
        {
            MethodResult<GetPopUpMaintainQueryModel> commandResult = await _mediator.Send(new GetPopupMaintainQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Set pop-up maintain time
        /// </summary>
        [HttpPut("set-time-pop-up")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.MasterAdmin))]
        public async Task<IActionResult> SetPopupMaintain([FromBody] SetTimePopupMaintainCommand command)
        {
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Create pop-up maintain
        /// </summary>
        [HttpPost("create-pop-up")]
        [ProducesResponseType(typeof(MethodResult<PopupMaintainModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.MasterAdmin))]
        public async Task<IActionResult> CreatePopupMaintain([FromBody] CreatePopupMaintainCommand command)
        {
            MethodResult<PopupMaintainModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
