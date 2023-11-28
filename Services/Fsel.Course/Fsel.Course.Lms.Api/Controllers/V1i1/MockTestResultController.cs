// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.V1i1
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.MockTestResultQuery;
    using Fsel.Course.Lms.Application.Queries.VideoTimeCodeResultQuery;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/mock-test-result")]
    [ApiController]
    [Common.Attributes.Permission(role: nameof(EnumRole.Student))]
    public class MockTestResultController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MockTestResultController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// get mock test ranking
        /// </summary>
        [HttpGet("mock-test-ranking")]
        [ProducesResponseType(typeof(MethodResult<IList<MockTestResultRankingModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] GetMockTestRankingQuery command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
