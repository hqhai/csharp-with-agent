// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.UnitQuery;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/unit-result")]
    [Authorize(Roles = nameof(EnumRole.Student))]
    [ApiController]
    public class UnitResultController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UnitResultController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get List Unit Result
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<UnitResultModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListLessonByUnitId([FromQuery] GetUnitScoreQuery query)
        {
            MethodResult<IList<UnitResultModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
