// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using Fsel.Common.ActionResults;
    using System.Net;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Course.Lms.Application.Queries.MockTestResultQuery;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/mock-test-result")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.Student))]
    public class MockTestResultController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MockTestResultController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get List Mock tesk result
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<MockTestResultModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListLessonByUnitId([FromQuery] GetMockTestReportQuery query)
        {
            MethodResult<IList<MockTestResultModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
