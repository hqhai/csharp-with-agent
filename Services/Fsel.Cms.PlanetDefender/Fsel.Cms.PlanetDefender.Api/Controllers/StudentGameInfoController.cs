// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Api.Controllers
{
    using System.Net;
    using Fsel.Cms.PlanetDefender.Application.Queries.StudentGameInfoQuery;
    using Fsel.Cms.PlanetDefender.Application.Services.UserServices.Models;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/cms-planet-defender")]
    [ApiController]
    public class StudentGameInfoController : BaseController
    {
        private readonly IMediator _mediator;

        public StudentGameInfoController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search students in platform
        /// </summary>
        [HttpGet("search-students-in-platform")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<StudentInPlatformModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLevelOfStudentsByStudentids([FromQuery] SearchStudentsInPlatformQuery query)
        {
            SetQuery(query);
            MethodResult<PagingItemsModel<StudentInPlatformModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
