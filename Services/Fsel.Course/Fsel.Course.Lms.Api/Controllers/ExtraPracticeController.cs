// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.ExtraPracticeQuery;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/extraPractice")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.Student))]
    public class ExtraPracticeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ExtraPracticeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Filter
        /// </summary>
        [HttpGet("level-units")]
        [ProducesResponseType(typeof(MethodResult<object>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLevelByUnits()
        {
            MethodResult<object> queryResult = await _mediator.Send(new GetListLevelByUnitQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
