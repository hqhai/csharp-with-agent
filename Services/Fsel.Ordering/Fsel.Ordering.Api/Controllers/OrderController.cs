// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Ordering.Application.Commands.OrderCmds;
    using Fsel.Ordering.Application.Queries.OrderQuery;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/order")]
    [ApiController]
    public class OrderController : ControllerBase
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
        [ProducesResponseType(typeof(MethodResult<GenerateRamdomOrderModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GenerateRandomOrder([FromQuery] GenerateRamdomOrderQuery query)
        {
            MethodResult<GenerateRamdomOrderModel> commandResult = await _mediator.Send(query).ConfigureAwait(false);
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
        /// Create Order
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<OrderModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateOrderCommand command)
        {
            MethodResult<OrderModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
