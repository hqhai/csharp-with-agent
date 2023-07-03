// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using Fsel.Common.ActionResults;
    using System.Net;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.MockTestResultQuery;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Course.Lms.Application.Queries.FinalTestResultQuery;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/final-test-result")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.Student))]
    public class FinalTestResultController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FinalTestResultController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get List final test result
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<FinalTestResultModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListFinalTestResultById([FromQuery] GetFinalTestReportQuery query)
        {
            MethodResult<IList<FinalTestResultModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
