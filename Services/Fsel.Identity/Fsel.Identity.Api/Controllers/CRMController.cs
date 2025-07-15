using System.Net;
using Asp.Versioning;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Identity.Application.Queries.UserReferrals;
using Fsel.Identity.Infrastructure.ValueSettings;
using Fsel.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Identity.Api.Controllers
{
    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/crm")]
    [ApiController]
    public class CRMController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly AppSetting _appSetting;
        private const string HeaderKey = "InternalSecretKey";

        public CRMController(IMediator mediator, AppSetting appSetting)
        {
            _mediator = mediator;
            _appSetting = appSetting;
        }

        /// <summary>
        /// Search User Referral
        /// </summary>
        [HttpGet("search-user-referral")]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchUserReferral([FromQuery] SearchUserReferralForCRMQuery query)
        {
            if (!Request.Headers.TryGetValue(HeaderKey, out var providedKey) || string.IsNullOrEmpty(_appSetting.CRMConfig?.SecretKey) || providedKey != _appSetting.CRMConfig?.SecretKey)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Invalid InternalSecretKey");
            }

            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
