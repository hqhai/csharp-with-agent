// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Api.Controllers
{
    using System.Net;
    using Fsel.Cms.PlanetDefender.Application.Queries.StudentGameInfoQuery;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/cms-planet-defender")]
    [ApiController]
    public class StudentGameInfoController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StudentGameInfoController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get level of students by studentids
        /// </summary>
        [HttpPost("get-level-by-studentids")]
        [ProducesResponseType(typeof(MethodResult<IList<StudentGameInfoModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLevelOfStudentsByStudentids([FromBody] GetLevelOfStudentsByStudentIdsQuery query)
        {
            MethodResult<IList<StudentGameInfoModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
