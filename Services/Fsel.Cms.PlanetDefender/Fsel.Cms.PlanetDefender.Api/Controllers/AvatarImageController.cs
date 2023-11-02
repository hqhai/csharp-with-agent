// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Api.Controllers
{
    using Fsel.Cms.PlanetDefender.Application.Queries.GameplayRuleConfigs;
    using Fsel.Cms.PlanetDefender.Domain.Models.QueryModels.GameplayRuleConfigs;
    using Fsel.Common.ActionResults;
    using System.Net;
    using Fsel.Common.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Cms.PlanetDefender.Application.Queries.AvatarImageQuery;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/avatar-image")]
    [ApiController]
    public class AvatarImageController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AvatarImageController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// get list avatar image
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<AvatarImageModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetList([FromQuery] GetListAvatarImageQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
