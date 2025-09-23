// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Course.Domain.Entities.SkillScoresConfigs;
using Fsel.Course.Lms.Application.Queries.CourseQuery;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Fsel.Shared.Constants;
using Fsel.Shared.Attributes;

namespace Fsel.Course.Lms.Api.Controllers
{
    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/course-result")]
    [ApiController]
    [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
    public class CourseResultController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CourseResultController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Unit-skill-diagram
        /// </summary>
        [HttpGet("unit-skill-diagram")]
        [ProducesResponseType(typeof(MethodResult<List<IList<SkillScores>>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUnitSkillDiagram([FromQuery] GetUnitSkillDiagramQuery query)
        {
            MethodResult<IList<SkillScores>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
