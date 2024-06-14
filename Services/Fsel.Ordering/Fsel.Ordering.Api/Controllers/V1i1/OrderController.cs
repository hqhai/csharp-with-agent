// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Api.Controllers.V1i1
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Ordering.Application.Commands.OrderCmds.v1i1;
    using Fsel.Ordering.Application.Queries.OrderQuery;
    using Fsel.Ordering.Application.Services.InAppPurchase;
    using Fsel.Ordering.Application.Services.InAppPurchase.Models;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/order")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly INotificationProcessor _notificationProcessor;

        public OrderController(IMediator mediator, INotificationProcessor notificationProcessor)
        {
            _mediator = mediator;
            _notificationProcessor = notificationProcessor;
        }

        /// <summary>
        /// Create Order
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<OrderModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Student))]
        public async Task<IActionResult> Create([FromBody] CreateOrderCommand command)
        {
            MethodResult<OrderModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Order
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<OrderModel?>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Student))]
        public async Task<IActionResult> Get()
        {
            var commandResult = await _mediator.Send(new GetOrderByUserQuery() { }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Order
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<OrderModel?>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetByUserId([FromRoute] Guid userId)
        {
            var commandResult = await _mediator.Send(new GetOrderByUserIdQuery { UserId = userId }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// App Store
        /// </summary>
        [AllowAnonymous]
        [HttpPost("app-store")]
        [ProducesResponseType(typeof(MethodResult<OrderModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AppStore([FromBody] AppleNotification appleNotification)
        {
            try
            {
                var decode = await _notificationProcessor.Process(appleNotification);
                if (decode)
                {
                    return Ok();
                }
                else
                {
                    return StatusCode(500);
                }
            }
            catch
            {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Get info transaction from app store
        /// </summary>
        [AllowAnonymous]
        [HttpGet("get-info-transaction")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetInfoTransaction([FromQuery] GetInfoTransactionFromAppStoreCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Order trial
        /// </summary>
        [HttpGet("get-order-by-status")]
        [ProducesResponseType(typeof(MethodResult<OrderModel?>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Student))]
        public async Task<IActionResult> GetOrderTrial([FromQuery] GetOrdersByStatusQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
