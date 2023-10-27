// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers.Admin
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Identity.Application.Queries.CSOQuery;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/cso/admin")]
    [ApiController]
    public class CSOController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CSOController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// get list cso by ids
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<IList<HumanModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCSOByIds([FromBody] IList<Guid>? ids)
        {
            MethodResult<IList<HumanModel>> commandResult = await _mediator.Send(new GetCsoByIdsQuery { Ids = ids }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get all cso
        /// </summary>
        [HttpGet("get-all")]
        [ProducesResponseType(typeof(MethodResult<IList<CSOModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAllCSO()
        {
            MethodResult<IList<CSOModel>> commandResult = await _mediator.Send(new GetAllCsoQuery { }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
