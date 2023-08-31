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

    [ApiVersion(Settings.APIVersion)]
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
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<NotificationsModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetList([FromQuery] GetListNotificationQuery query)
        {
            MethodResult<PagingItemsModel<NotificationsModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }


        /// <summary>
        /// Push Notification
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<NotificationsModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationCommand cmd)
        {
            MethodResult<NotificationsModel> commandResult = await _mediator.Send(cmd).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }


        /// <summary>
        /// Turn Off/On Notification
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost("status")]
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
        [HttpPut("status")]
        [ProducesResponseType(typeof(MethodResult<NotificationRemindModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateNotificationRemind([FromBody] UpdateNotificationRemindCommand cmd)
        {
            MethodResult<NotificationRemindModel> commandResult = await _mediator.Send(cmd).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
