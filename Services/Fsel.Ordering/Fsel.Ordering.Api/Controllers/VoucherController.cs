// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Ordering.Application.Commands.Products;
    using Fsel.Ordering.Application.Commands.VoucherCmds;
    using Fsel.Ordering.Application.Queries.UserVoucher;
    using Fsel.Ordering.Application.Queries.VoucherQuery;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/voucher")]
    [ApiController]
    public class VoucherController : BaseController
    {
        private readonly IMediator _mediator;

        public VoucherController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search voucher FSEL
        /// </summary>
        [HttpGet("search-voucher-fsel")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<VoucherModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> SearchVoucherFSEL([FromQuery] SearchVoucherFSELQuery query)
        {
            MethodResult<PagingItemsModel<VoucherModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search voucher auto
        /// </summary>
        [HttpGet("search-voucher-auto")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<VoucherModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> SearchVoucherAuto([FromQuery] SearchVoucherAutoQuery query)
        {
            MethodResult<PagingItemsModel<VoucherModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Voucher
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<VoucherModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            MethodResult<VoucherModel> queryResult = await _mediator.Send(new GetVoucherQuery { Id = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Voucher
        /// </summary>
        [HttpGet("get-info-voucher-auto")]
        [ProducesResponseType(typeof(MethodResult<VoucherModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> GetInfoVoucherAuto([FromQuery] GetInfoVoucherAutoQuery query)
        {
            MethodResult<VoucherModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create a Voucher
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<VoucherModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> Create([FromForm] CreateVoucherCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            MethodResult<VoucherModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update a voucher
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(MethodResult<VoucherModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> Update([FromForm] UpdateVoucherCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            MethodResult<VoucherModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Delete Vouchers
        /// </summary>
        [HttpPost("delete")]
        [ProducesResponseType(typeof(MethodResult<VoucherModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> Delete([FromBody] DeleteVouchersCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get list User voucher
        /// </summary>
        [HttpGet("get-current-vouchers")]
        [ProducesResponseType(typeof(MethodResult<IList<UserVoucherModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get()
        {
            MethodResult<IList<UserVoucherModel>> commandResult = await _mediator.Send(new GetListUserVoucherQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Search history voucher
        /// </summary>
        [HttpGet("search-history")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<HistoryVoucherModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> SearchHistory([FromQuery] SearchHistoryVoucherQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search history voucher
        /// </summary>
        [HttpGet("search-history-voucher-auto")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<HistoryVoucherModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> SearchHistoryVoucherAuto([FromQuery] SearchDetailHistoryVoucherAutoQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// check voucher
        /// </summary>
        [HttpGet("check-voucher")]
        [ProducesResponseType(typeof(MethodResult<CheckVoucherModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Student))]
        public async Task<IActionResult> CheckVoucher([FromQuery] CheckVoucherCommand query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create vouchers for MA
        /// </summary>
        [HttpPost("create-vouchers-for-master-agency")]
        [ProducesResponseType(typeof(MethodResult<CheckVoucherModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateVouchersForMA([FromBody] CreateVoucherForMasterAgencyCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search history voucher
        /// </summary>
        [HttpGet("get-user-voucher-lock-by-user")]
        [ProducesResponseType(typeof(MethodResult<UserVoucherLockModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Student))]
        public async Task<IActionResult> GetUserVoucherLockByUser()
        {
            var queryResult = await _mediator.Send(new GetUserVoucherLockByUserQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Export detail history voucher auto
        /// </summary>
        [HttpGet("export-detail-history-voucher-auto")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> ExportDetailHistoryVoucherAuto([FromQuery] ExportDetailHistoryVoucherAutoCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "details of automatic voucher exchange history.xlsx");
        }

        /// <summary>
        /// Export detail history voucher auto
        /// </summary>
        [HttpGet("export-voucher-fsel")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> ExportVoucherFSEL([FromQuery] ExportVoucherFSELCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "FSEL voucher list.xlsx");
        }

        /// <summary>
        /// Export detail history voucher auto
        /// </summary>
        [HttpGet("export-voucher-auto")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> ExportVoucherAuto([FromQuery] ExportVoucherAutoCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "Auto voucher list.xlsx");
        }
    }
}
