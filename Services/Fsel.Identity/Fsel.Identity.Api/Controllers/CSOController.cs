// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Identity.Application.Queries.CsoQuery;
using Fsel.Identity.Application.Queries.CSOQuery;
using Fsel.Identity.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Fsel.Shared.Constants;

namespace Fsel.Identity.Api.Controllers
{
    [ApiVersion(ApiSettings.APIVersion1)][ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/cso")]
    [ApiController]
    public class CSOController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CSOController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get cso by Id
        /// </summary>
        [HttpGet("get-by-id/{id}")]
        [ProducesResponseType(typeof(MethodResult<CSOModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            MethodResult<CSOModel> commandResult = await _mediator.Send(new GetCsoByIdQuery { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get cso by UserId
        /// </summary>
        [HttpGet("get-by-user-id/{id}")]
        [ProducesResponseType(typeof(MethodResult<CSOModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetByUserId([FromRoute] Guid id)
        {
            MethodResult<CSOModel> commandResult = await _mediator.Send(new GetCsoByUserIdQuery { UserId = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get cso by userid
        /// </summary>
        [HttpPost("get-cso-by-userIds")]
        [ProducesResponseType(typeof(MethodResult<IList<CSOModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCsosByUserIds([FromBody] IList<Guid> userIds)
        {
            MethodResult<IList<CSOModel>> commandResult = await _mediator.Send(new GetCsoByUserIdsQuery { UserIds = userIds }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
