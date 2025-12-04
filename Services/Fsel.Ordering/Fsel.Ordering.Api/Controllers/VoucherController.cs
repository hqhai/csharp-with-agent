// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
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
        /// Get list User voucher
        /// </summary>
        [HttpGet("get-current-vouchers")]
        [ProducesResponseType(typeof(MethodResult<IList<UserVoucherModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        public async Task<IActionResult> Get()
        {
            MethodResult<IList<UserVoucherModel>> commandResult = await _mediator.Send(new GetListUserVoucherQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// check voucher
        /// </summary>
        [HttpGet("check-voucher")]
        [ProducesResponseType(typeof(MethodResult<CheckVoucherModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CheckVoucher([FromQuery] CheckVoucherCommand query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search history voucher
        /// </summary>
        [HttpGet("get-user-voucher-lock-by-user")]
        [ProducesResponseType(typeof(MethodResult<UserVoucherLockModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        public async Task<IActionResult> GetUserVoucherLockByUser()
        {
            var queryResult = await _mediator.Send(new GetUserVoucherLockByUserQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search voucher
        /// </summary>
        [HttpGet("search-voucher-by-user")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<VoucherModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        public async Task<IActionResult> GetUserVoucherLockByUser([FromQuery] SearchVoucherByUserQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
