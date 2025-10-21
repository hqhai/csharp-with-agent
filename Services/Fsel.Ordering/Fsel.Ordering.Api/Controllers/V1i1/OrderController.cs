// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Api.Controllers.V1i1
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Commands.OrderCmds.v1i1;
    using Fsel.Ordering.Application.Queries.OrderQuery;
    using Fsel.Ordering.Application.Services.InAppPurchase.IOS;
    using Fsel.Ordering.Application.Services.InAppPurchase.IOS.Models;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/order")]
    [ApiController]
    public class OrderController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly INotificationProcessor _notificationProcessor;

        public OrderController(IMediator mediator, INotificationProcessor notificationProcessor)
        {
            _mediator = mediator;
            _notificationProcessor = notificationProcessor;
        }

        ///// <summary>
        ///// Create Order
        ///// </summary>
        //[HttpPost]
        //[MapToApiVersion(ApiSettings.APIVersion1i1)]
        //[ProducesResponseType(typeof(MethodResult<OrderModel>), (int)HttpStatusCode.OK)]
        //[ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        //[Common.Attributes.Permission(role: nameof(EnumRole.Student))]
        //public async Task<IActionResult> Create([FromBody] CreateOrderCommand command)
        //{
        //    MethodResult<OrderModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
        //    return commandResult.GetActionResult();
        //}

        /// <summary>
        /// Get Order
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<OrderModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
       [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        public async Task<IActionResult> Get()
        {
            var commandResult = await _mediator.Send(new GetOrderByUserQuery() { }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Order
        /// </summary>
        [HttpGet("trial/{userId}")]
        [ProducesResponseType(typeof(MethodResult<OrderModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetByUserId([FromRoute] Guid userId)
        {
            var commandResult = await _mediator.Send(new GetOrderTrialByUserIdQuery { UserId = userId }).ConfigureAwait(false);
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
       [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        public async Task<IActionResult> GetOrderTrial([FromQuery] GetOrdersByStatusQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Create Order
        /// </summary>
        [HttpPost("create-orders-from-crm")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> Create([FromBody] CreateOrdersFromCRMCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
