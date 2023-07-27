// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.System.Application.Commands.ReferralDiscountConfigCmd;
    using Fsel.System.Application.Commands.TeachingCostCmd;
    using Fsel.System.Application.Queries.ReferralDiscountConfigQuery;
    using Fsel.System.Application.Querys.LiveTimeFrameQuery;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/referral-discount-config")]
    [ApiController]
    public class ReferralDiscountConfigController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReferralDiscountConfigController(IMediator mediator)
        {
            _mediator = mediator;
        }
        /// <summary>
        /// Save Referral Discount Config
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<ReferralDiscountConfigModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Save([FromBody] SaveReferralDiscountConfigCommand command)
        {
            MethodResult<ReferralDiscountConfigModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get list Referral Discount Config
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<ReferralDiscountConfigModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get()
        {
            MethodResult<IList<ReferralDiscountConfigModel>> commandResult = await _mediator.Send(new GetListReferralDiscountConfigQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
