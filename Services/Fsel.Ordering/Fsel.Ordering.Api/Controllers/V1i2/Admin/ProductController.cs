using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Attributes;
using Fsel.Common.Constants;
using Fsel.Core.Base.BaseModels;
using Fsel.Ordering.Application.Commands.Products;
using Fsel.Ordering.Application.Queries.Products;
using Fsel.Ordering.Domain.Models.EntityModels;
using Fsel.Shared.Attributes;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Ordering.Api.Controllers.V1i2.Admin
{
    [ApiVersions(ApiSettings.APIVersion1i2)]
    [Route(Settings.APIDefaultRoute + "/admin/product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create product
        /// </summary>
        [HttpPost("save")]
        [ProducesResponseType(typeof(MethodResult<ProductModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(new[] { GiftManagement.Add, GiftManagement.Update })]
        public async Task<IActionResult> Create([FromBody] SaveProductCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get product by id
        /// </summary>
        [HttpGet("get-by-id")]
        [ProducesResponseType(typeof(MethodResult<ProductModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(GiftManagement.View)]
        public async Task<IActionResult> Get([FromQuery] GetProductByIdQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Search product
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<OrderSearchModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(GiftManagement.View)]
        public async Task<IActionResult> Search([FromQuery] SearchProductQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Delete products
        /// </summary>
        [HttpPost("delete")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(GiftManagement.Delete)]
        public async Task<IActionResult> Delete([FromBody] DeleteProductsCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Search history redeem
        /// </summary>
        [HttpGet("search-history-redeem")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(GiftManagement.View)]
        public async Task<IActionResult> SearchHistoryRedeem([FromQuery] SearchHistoryRedeemByAdminQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Export history redeem
        /// </summary>
        [HttpGet("export-history-redeem")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(GiftManagement.Export)]
        public async Task<IActionResult> ExportHistoryRedeem([FromQuery] ExportHistoryRedeemProductCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "Product_exchange_history.xlsx");
        }
    }
}
