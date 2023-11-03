// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Api.Controllers
{
    using System.Net;
    using Fsel.Cms.PlanetDefender.Application.Queries.StudentTagNameQuery;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/student-tag-name")]
    [ApiController]
    public class StudentTagNameController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StudentTagNameController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// get list avatar image
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<StudentTagNameModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetList()
        {
            var commandResult = await _mediator.Send(new GetListStudentTagNameQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
