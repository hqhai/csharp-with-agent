// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Commands.OrderCmds;
    using Fsel.Ordering.Application.Queries.IntegrationQuery;
    using Fsel.Ordering.Application.Queries.OrderQuery;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/order")]
    [ApiController]
    [Permission]
    public class OrderController : BaseController
    {
        private readonly IMediator _mediator;

        public OrderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Generate Random Order
        /// </summary>
        [HttpGet]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GenerateRandomOrder([FromQuery] GenerateRandomOrderQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Check Status User
        /// </summary>
        [HttpGet("get-status")]
        [ProducesResponseType(typeof(MethodResult<EnumOrderStatus?>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetStatus([FromQuery] GetStatusOrderByUserQuery query)
        {
            MethodResult<EnumOrderStatus?> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Check Status User
        /// </summary>
        [HttpGet("get-list-order")]
        [ProducesResponseType(typeof(MethodResult<IList<OrderModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListOrder([FromQuery] GetListOrderQuery query)
        {
            MethodResult<IList<OrderModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Create Order
        /// </summary>
        [HttpPost]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        [ProducesResponseType(typeof(MethodResult<OrderModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateOrderCommand command)
        {
            MethodResult<OrderModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Payment success
        /// </summary>
        [HttpGet("payment-success/{secretKey}")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> PaymentSuccess([FromRoute] string secretKey)
        {
            var commandResult = await _mediator.Send(new PaymentSuccessCommand { SecretKey = secretKey }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Payment
        /// </summary>
        [HttpPost("payment")]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Payment([FromBody] PaymentCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Check Current Status Of User
        /// </summary>
        [HttpGet("get-current-status/{id}")]
        [ProducesResponseType(typeof(MethodResult<EnumTrialRegistrationStatus?>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCurrentStatus([FromRoute] Guid id)
        {
            MethodResult<EnumTrialRegistrationStatus?> commandResult = await _mediator.Send(new GetCurrentStatusQuery { UserId = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Search Course
        /// </summary>
        [HttpPost("get-order-by-status")]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        [ProducesResponseType(typeof(MethodResult<IList<OrderSearchModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOrderByStatus([FromBody] GetOrderByStatusQuery query)
        {
            MethodResult<IList<OrderSearchModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Integration Query
        /// </summary>
        [HttpPost("integration-order")]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        [ProducesResponseType(typeof(MethodResult<IList<OrderSearchModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOrderIntegrationByStatusQuery([FromBody] GetOrderByStatusIntegrationQuery query)
        {
            MethodResult<IList<OrderSearchModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Order Configure add user to blind bag event
        /// </summary>
        [HttpPost("get-orders-blind-bag-event")]
        [ProducesResponseType(typeof(MethodResult<IList<OrderModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRecentOrders([FromBody] GetRecentOrdersToUserIdsQuery query)
        {
            MethodResult<IList<OrderModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
