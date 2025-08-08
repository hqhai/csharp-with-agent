// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lcms.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Application.Queries.KeyboardLayoutQuery;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/keyboard-layout")]
    [ApiController]
    public class KeyboardLayoutController : ControllerBase
    {
        private readonly IMediator _mediator;

        public KeyboardLayoutController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Keyboard layout
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<KeyboardLayoutModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchKeyboardText([FromQuery] SearchKeyboardLayoutQuery query)
        {
            MethodResult<PagingItemsModel<KeyboardLayoutModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
