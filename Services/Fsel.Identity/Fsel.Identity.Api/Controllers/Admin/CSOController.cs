// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Identity.Application.Queries.TeacherQuery;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/cso/admin")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.Admin))]
    public class CSOController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CSOController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// get teacher and cso
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<IList<HumanModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetTeacherAndCSOByIds([FromBody] IList<Guid>? ids)
        {
            MethodResult<IList<HumanModel>> commandResult = await _mediator.Send(new GetTeacherAndCSOByIdsQuery { Ids = ids}).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
