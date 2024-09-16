// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Api.Controllers.V1i2.Admin
{
    using Fsel.Common.ActionResults;
    using System.Net;
    using Fsel.Common.Constants;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Microsoft.AspNetCore.Mvc;
    using MediatR;
    using Fsel.Ordering.Application.Commands.OrderCmds.V1i2;

    using Fsel.Core.Base.BaseModels;
    using Fsel.Ordering.Domain.Models.EntityModels.V1i2;
    using Fsel.Ordering.Application.Queries.OrderQuery.V1i2;

    [ApiVersions(ApiSettings.APIVersion1i2)]
    [Route(Settings.APIDefaultRoute + "/admin/order")]
    [ApiController]
    [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
    public class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// change status order
        /// </summary>
        [HttpPut("change-status-order")]
        [ApiVersions(ApiSettings.APIVersion1i2)]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ChangeStatusOrder([FromBody] ChangeStatusOrderCommand query)
        {
            MethodResult<bool> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search Course
        /// </summary>
        [HttpGet("search-order")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<SearchOrderModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchOrderQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get by id
        /// </summary>
        [HttpGet("get-by-id/{id}")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<SearchOrderModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromRoute] Guid id)
        {
            var queryResult = await _mediator.Send(new GetOrderByIdQuery() { Id = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// create order payment
        /// </summary>
        [HttpPost("create-order-payment")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<SearchOrderModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateOrderPayment([FromBody] CreateOrderPaymentCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
