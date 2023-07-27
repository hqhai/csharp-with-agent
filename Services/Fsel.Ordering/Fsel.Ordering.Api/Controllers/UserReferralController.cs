// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Ordering.Application.Queries.UserRefferalQuery;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/user-referral")]
    [ApiController]
    public class UserReferralController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserReferralController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get list User voucher
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<UserReferralModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get()
        {
            MethodResult<IList<UserReferralModel>> commandResult = await _mediator.Send(new GetListUserReferralQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
