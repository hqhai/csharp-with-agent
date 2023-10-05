// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Application.Queries.PlatformQuery;
    using Fsel.Identity.Application.Queries.StudentQuery;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/platform")]
    [ApiController]
    public class PlatformController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PlatformController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all platform
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<PlatformModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            MethodResult<IList<PlatformModel>> commandResult = await _mediator.Send(new GetAllPlatformQuery { }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get platforms by type
        /// </summary>
        [HttpGet("get-platforms-by-type")]
        [ProducesResponseType(typeof(MethodResult<IList<PlatformModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPlatformByType([FromQuery] GetPlatformsByTypeQuery query)
        {
            MethodResult<IList<PlatformModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get all platform
        /// </summary>
        [HttpGet("search-user-platform")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<StudentInPlatformModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] SearchStudentInPlatformQuery query)
        {
            MethodResult<PagingItemsModel<StudentInPlatformModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
