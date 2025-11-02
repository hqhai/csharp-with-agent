// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Api.Controllers.V1i2
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Ordering.Application.Commands.OrderCmds.V1i2;
    using Fsel.Ordering.Application.Queries.OrderQuery.V1i2;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Ordering.Domain.Models.EntityModels.V1i2;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1i2)]
    [Route(Settings.APIDefaultRoute + "/order")]
    [ApiController]
    public class OrderController : BaseController
    {
        private readonly IMediator _mediator;

        public OrderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create Order
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<OrderModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        public async Task<IActionResult> Create([FromBody] CreateOrderCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Create Order By UserId
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<OrderModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateOrderByUserIdCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Create Order
        /// </summary>
        [HttpPost("create-order-trial")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        public async Task<IActionResult> Create([FromBody] CreateOrderTrialCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// verify data from android app
        /// </summary>
        [HttpPost("verify-data-from-android-app")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        public async Task<IActionResult> VerifyDataFromAndroidApp([FromBody] VerifyDataFromAndroidAppCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get by id
        /// </summary>
        [HttpGet("get-by-id/{id}")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<SearchOrderModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        public async Task<IActionResult> Search([FromRoute] Guid id)
        {
            var queryResult = await _mediator.Send(new GetOrderByIdQuery() { Id = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search Course
        /// </summary>
        [HttpGet("search-order")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<SearchOrderModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        public async Task<IActionResult> Search([FromQuery] SearchOrderQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create Order for student
        /// </summary>
        [HttpPost("create-order-for-student-leader-board")]
        [ProducesResponseType(typeof(MethodResult<OrderModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateOrderForStudentLeaderBoard([FromBody] CreateOrderForUserFromLeaderBoardCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get orders by user id
        /// </summary>
        [HttpGet("get-orders-by-user-id")]
        [ProducesResponseType(typeof(MethodResult<IList<OrderModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOrderByUserId([FromQuery] GetOrdersByUserIdQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get users has order payment
        /// </summary>
        [HttpPost("get-users-has-order-payment")]
        [ProducesResponseType(typeof(MethodResult<IList<Guid>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUsersHasOrderPayment([FromBody] GetUsersHasOrderPaymentQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get order revenue
        /// </summary>
        [HttpGet("get-order-revenue")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<SearchOrderModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> GetOrderRevenues()
        {
            var query = new SearchOrderQuery()
            {
                RevenueType = EnumPaymentRevenueType.Revenue,
            };
            query.SetIsQueryAll(true);
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
