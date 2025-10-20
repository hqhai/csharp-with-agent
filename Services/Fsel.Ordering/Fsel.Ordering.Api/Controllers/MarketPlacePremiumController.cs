using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Attributes;
using Fsel.Common.Constants;
using Fsel.Core.Base.BaseModels;
using Fsel.Ordering.Application.Commands.MarketplacePremiumCmd;
using Fsel.Ordering.Application.Queries.MarketplacePremiumQuery;
using Fsel.Ordering.Domain.Models.EntityModels;
using Fsel.Shared.Attributes;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Ordering.Api.Controllers
{
    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/marketplace-premium")]
    [ApiController]
    [Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
    public class MarketPlacePremiumController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MarketPlacePremiumController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search product
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<ProductModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchProductPremiumQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get detail product
        /// </summary>
        [HttpGet("get")]
        [ProducesResponseType(typeof(MethodResult<ProductModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] GetProductPremiumQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Redeem product
        /// </summary>
        [HttpPost("redeem")]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> RedeemProduct([FromBody] RedeemProductPremiumCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get histories
        /// </summary>
        [HttpGet("get-histories")]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetHistories()
        {
            var commandResult = await _mediator.Send(new GetExchangeHistoriesQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// check user
        /// </summary>
        [HttpGet("check")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<bool>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Check()
        {
            var commandResult = await _mediator.Send(new CheckShowMarketplacePremiumQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        ///// <summary>
        ///// Redeem product
        ///// </summary>
        //[HttpPost("spam-redeem")]
        //[ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        //[ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        //public async Task<IActionResult> SpamRedeemProduct([FromBody] SpamRedeemProductPremiumCommand command)
        //{
        //    var commandResult = await _mediator.Send(command).ConfigureAwait(false);
        //    return commandResult.GetActionResult();
        //}
    }
}
