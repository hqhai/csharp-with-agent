// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Commands.UserRefferalCmd;
    using Fsel.Ordering.Application.Queries.UserRefferalQuery;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/user-referral")]
    [ApiController]
    public class UserReferralController : BaseController
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

        /// <summary>
        /// Create User Referral
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateUserReferralCommand command)
        {
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
