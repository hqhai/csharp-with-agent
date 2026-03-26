// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.LevelQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/level")]
    [ApiController]
    public class LevelController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LevelController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("get-by-ids")]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        [ProducesResponseType(typeof(MethodResult<List<LevelModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [ApiVersion(ApiSettings.APIVersion1)]
        public async Task<IActionResult> GetLevels([FromQuery] List<Guid> levelIds)
        {
            var queryResult = await _mediator.Send(new GetLevelsQuery
            {
                LevelIds = levelIds
            }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
