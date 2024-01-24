// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Ordering.Application.Commands.Payoo;
    using Fsel.Ordering.Application.Queries.PayooQuery;
    using Fsel.Ordering.Application.Services.PayooService.Models;
    using Fsel.Ordering.Domain.Models.EntityModels.Payoo;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/payoo")]
    [ApiController]
    public class PayooController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PayooController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// shop back url
        /// </summary>
        [HttpGet("shop-back-url")]
        [ProducesResponseType(typeof(MethodResult<ShopBackUrlModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ShopBackUrl([FromQuery] ShopBackUrlQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// shop back url
        /// </summary>
        [HttpGet("create")]
        [ProducesResponseType(typeof(MethodResult<PayooModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreatePayoo()
        {
            var commandResult = await _mediator.Send(new PaymentWithPayooCommand ()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
