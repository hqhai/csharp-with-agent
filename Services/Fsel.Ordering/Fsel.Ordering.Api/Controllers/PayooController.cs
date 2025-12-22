// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Commands.Payoo;
    using Fsel.Ordering.Application.Services.PayooService.Models;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/payoo")]
    [ApiController]
    public class PayooController : BaseController
    {
        private readonly IMediator _mediator;

        public PayooController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// create link payment
        /// </summary>
        [HttpPost("create")]
        [ProducesResponseType(typeof(MethodResult<PayooModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreatePayoo([FromBody] PaymentWithPayooCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// notify url
        /// </summary>
        [HttpPost("notify-url")]
        [ProducesResponseType(typeof(NotifyUrlModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> NotifyUrl([FromBody] NotifyUrlCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return new ObjectResult(commandResult.Result)
            {
                StatusCode = commandResult.StatusCode
            };
        }

        /// <summary>
        /// create link payment gtel
        /// </summary>
        [HttpPost("create-gtel")]
        [ProducesResponseType(typeof(MethodResult<PayooModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreatePayoo([FromBody] PaymentWithPayooGtelCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// notify url gtel
        /// </summary>
        [HttpPost("notify-url-gtel")]
        [ProducesResponseType(typeof(NotifyUrlModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> NotifyUrl([FromBody] NotifyUrlGtelCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return new ObjectResult(commandResult.Result)
            {
                StatusCode = commandResult.StatusCode
            };
        }
    }
}
