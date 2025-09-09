// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lcms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Application.Commands.CurriculumCmd;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/curriculum")]
    [ApiController]
    public class CurriculumsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public CurriculumsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create a curriculum
        /// </summary>
        [HttpPost("create-curriculum")]
        [ProducesResponseType(typeof(MethodResult<CurriculumModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateCurriculumCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
