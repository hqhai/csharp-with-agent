// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Admin
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi;
    using Fsel.Course.Lms.Application.Queries.ReportEventHaNoiQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/admin/report-event-hanoi")]
    [ApiController]
    public class ReportEventHaNoiController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportEventHaNoiController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get 
        /// </summary>
        [HttpGet("evaluat-input-result")]
        [ProducesResponseType(typeof(MethodResult<EvaluateInputResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListUnitByCourse()
        {
            var queryResult = await _mediator.Send(new EvaluateInputResultCityQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
