// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Api.Controllers.V1i2
{
    using Fsel.Common.ActionResults;
    using System.Net;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Ordering.Application.Queries.Products;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Ordering.Application.Commands.Products;
    using Microsoft.AspNetCore.Authorization;

    [ApiVersions(ApiSettings.APIVersion1i2)]
    [Route(Settings.APIDefaultRoute + "/product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get product by id
        /// </summary>
        [HttpGet("get-by-id")]
        [ProducesResponseType(typeof(MethodResult<ProductModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
       [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        public async Task<IActionResult> Get([FromQuery] GetProductByIdQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Search product
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<ProductModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
       [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        public async Task<IActionResult> Search([FromQuery] SearchProductQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Redeem product
        /// </summary>
        [HttpPost("redeem-product")]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
       [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        public async Task<IActionResult> RedeemProduct([FromBody] RedeemProductCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// History redeem Product
        /// </summary>
        [HttpGet("history-redeem-product")]
        [ProducesResponseType(typeof(MethodResult<HistoryRedeemProductModels>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
       [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        public async Task<IActionResult> HistoryRedeemProduct()
        {
            var commandResult = await _mediator.Send(new GetHistoryRedeemProductQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Confirm receipt of product
        /// </summary>
        [HttpPost("confirm-receipt-of-product")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmReceiptOfProduct([FromBody] ConfirmReceiptOfProductCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
