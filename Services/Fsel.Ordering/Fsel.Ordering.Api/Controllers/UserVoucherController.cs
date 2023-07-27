// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Api.Controllers
{
    using Fsel.Common.ActionResults;
    using System.Net;
    using Fsel.Common.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Ordering.Application.Queries.UserVoucher;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/user-voucher")]
    [ApiController]
    public class UserVoucherController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserVoucherController(IMediator mediator)
        {
            _mediator = mediator;
        }
        /// <summary>
        /// Get list User voucher
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<UserVoucherModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get()
        {
            MethodResult<IList<UserVoucherModel>> commandResult = await _mediator.Send(new GetListUserVoucherQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
