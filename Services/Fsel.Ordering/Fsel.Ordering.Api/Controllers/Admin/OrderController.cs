// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Api.Controllers.Admin
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Ordering.Application.Commands.OrderCmds;
    using Fsel.Ordering.Application.Commands.OrderCmds.v1i1;
    using Fsel.Ordering.Application.Queries.OrderQuery;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/admin/order")]
    [ApiController]
    [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
    public class OrderController : BaseController
    {
        private readonly IMediator _mediator;

        public OrderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Course
        /// </summary>
        [HttpGet("search-order")]
        [MapToApiVersion(ApiSettings.APIVersion1i1)]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<OrderSearchModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchOrderQuery query)
        {
            MethodResult<PagingItemsModel<OrderSearchModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search Course
        /// </summary>
        [HttpPut("change-status-order")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ChangeStatusOrder([FromBody] ChangeStatusOrderCommand query)
        {
            MethodResult<bool> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create Order for student
        /// </summary>
        [HttpPost("create-order-for-student")]
        [ProducesResponseType(typeof(MethodResult<OrderModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderByUserIdCommand command)
        {
            MethodResult<OrderModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
