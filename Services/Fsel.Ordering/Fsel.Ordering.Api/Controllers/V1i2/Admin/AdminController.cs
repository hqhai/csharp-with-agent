// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Api.Controllers.V1i2.Admin
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Ordering.Application.Commands.OrderCmds.V1i2;
    using Fsel.Ordering.Application.Queries.OrderQuery.V1i2;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Ordering.Domain.Models.EntityModels.V1i2;
    using Fsel.Ordering.Domain.Models.QueryModels.Oders.V1i2;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1i2)]
    [Route(Settings.APIDefaultRoute + "/admin/order")]
    [ApiController]
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
        [Permission(PaymentManagement.Update)]
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
        [Permission(PaymentManagement.View)]
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
        [Permission(PaymentManagement.View)]
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
        [Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> CreateOrderPayment([FromBody] CreateOrderPaymentCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search history voucher
        /// </summary>
        [HttpPost("create-voucher-and-send-mail")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> GetUserVoucherLockByUser([FromBody] CreateVoucherAndSendMailCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Expot revenue report
        /// </summary>
        [HttpPost("export-revenue-report")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> ExportFile([FromQuery] ExportRevenueReportQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            if (!queryResult.IsOK || queryResult.Result == null)
            {
                return queryResult.GetActionResult();
            }
            return File(queryResult.Result, Settings.Excels.ContentType, "export_revenue_report.xlsx");
        }

        /// <summary>
        /// Create Order For Students Event
        /// </summary>
        [HttpPost("create-order-for-students-event")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> CreateOrderForStudentsEvent([FromBody] CreateOrderForStudentsEventCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        ///// <summary>
        ///// delete Order of Students Event
        ///// </summary>
        //[HttpPost("delete-order-of-students-event")]
        //[ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        //[ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        //public async Task<IActionResult> DeleteOrderOfStudentsEvent([FromBody] DeleteOrderOfStudentsInEventCommand command)
        //{
        //    var queryResult = await _mediator.Send(command).ConfigureAwait(false);
        //    return queryResult.GetActionResult();
        //}

        /// <summary>
        /// get orders by userids
        /// </summary>
        [HttpPost("get-orders-by-user-ids")]
        [ProducesResponseType(typeof(MethodResult<OrdersByUserIdsModels>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> GetOrderByUserIds([FromBody] GetOrdersByUserIdsQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get users has order revenue
        /// </summary>
        [HttpPost("get-users-has-order-revenue")]
        [ProducesResponseType(typeof(MethodResult<IList<OrderModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUsersHasOrderRevenue([FromBody] GetUsersHasOrderRevenueByUserIdsQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}