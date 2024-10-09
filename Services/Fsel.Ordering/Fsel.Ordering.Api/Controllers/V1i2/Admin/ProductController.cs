using Fsel.Common.ActionResults;
using System.Net;
using Fsel.Common.Constants;
using Fsel.Shared.Attributes;
using Fsel.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Fsel.Ordering.Domain.Models.EntityModels;
using Fsel.Ordering.Application.Commands.Products;
using Fsel.Ordering.Application.Queries.Products;
using Fsel.Core.Base.BaseModels;
using Fsel.Shared.Enums;

namespace Fsel.Ordering.Api.Controllers.V1i2.Admin
{
    [ApiVersions(ApiSettings.APIVersion1i2)]
    [Route(Settings.APIDefaultRoute + "/product")]
    [ApiController]
    [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
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
        public async Task<IActionResult> Delete([FromBody] DeleteProductsCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
