// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Api.Controllers
{
    using Fsel.Common.ActionResults;
    using System.Net;
    using Fsel.Common.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Notification.Domain.Model.EntityModels;
    using Fsel.Notification.Application.Queries;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Notification.Application.Commands;
    using Asp.Versioning;
    using Fsel.Shared.Constants;
    using Fsel.Notification.Domain.Model.CommandModels.Notification;
    using Microsoft.AspNetCore.Authorization;

    [ApiVersion(ApiSettings.APIVersion1)][ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/notifications")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NotificationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Notification
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<NotificationMessageModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetList([FromQuery] GetListNotificationQuery query)
        {
            MethodResult<PagingItemsModel<NotificationMessageModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Push Notification
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<NotificationMessageModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationCommand cmd)
        {
            MethodResult<NotificationMessageModel> commandResult = await _mediator.Send(cmd).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        [HttpPost("test-socket-notification")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Authorize]
        public async Task<IActionResult> CreateNotificationTest([FromBody] CreateNotificationTestCommand cmd)
        {
            MethodResult<bool> commandResult = await _mediator.Send(cmd).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Turn Off/On Notification
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost("remind-status")]
        [ProducesResponseType(typeof(MethodResult<NotificationRemindModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateNotificationRemind([FromBody] CreateNotificationRemindCommand cmd)
        {
            MethodResult<NotificationRemindModel> commandResult = await _mediator.Send(cmd).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Turn Off/On Notification
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost("remind-by-status")]
        [ProducesResponseType(typeof(MethodResult<IList<NotificationRemindModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListNotificationRemind([FromBody] GetListNotificationRemindQuery query)
        {
            MethodResult<IList<NotificationRemindModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Mark Read Notification
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPut("status")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateStatusNotificationMessage([FromBody] UpdateStatusNotificationCommand cmd)
        {
            MethodResult<bool> commandResult = await _mediator.Send(cmd).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
