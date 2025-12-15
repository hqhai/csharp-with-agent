// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.V1i2
{
    using System.Net;
    using Application.Commands.TestCmd;
    using Application.Queries.TestQuery;
    using Common.ActionResults;
    using Common.Attributes;
    using Common.Constants;
    using Domain.Models.EntityModels.PlacementTestModels;
    using Domain.Models.EntityModels.TestModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Attributes;
    using Shared.Constants;
    using Shared.Enums;

    [ApiVersions(ApiSettings.APIVersion1i2)]
    [Route(Settings.APIDefaultRoute + "/test")]
    [ApiController]
    [Permission(role: nameof(EnumRole.Student))]
    public class TestController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TestController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [ProducesResponseType(typeof(MethodResult<SectionStateModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [HttpGet("get-test-section-result-detail/{id:guid}")]
        public async Task<IActionResult> GetTestSectionResultDetail(Guid id)
        {
            var getSectionResultDetailQuery = new GetTestSectionResultDetailQuery { SectionResultId = id };
            var queryResult = await _mediator.Send(getSectionResultDetailQuery).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [ProducesResponseType(typeof(MethodResult<TestStateModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [HttpGet("get-test-result-detail/{id:guid}")]
        public async Task<IActionResult> GetTestResultDetail(Guid id)
        {
            var getTestResultDetailQuery = new GetTestResultDetailQuery { TestResultId = id };
            var queryResult = await _mediator.Send(getTestResultDetailQuery).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [ProducesResponseType(typeof(MethodResult<SingleTestStateModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [HttpPost("start-or-continue/{testResultId:guid}")]
        public async Task<IActionResult> SelectTestProgramId(Guid testResultId, [FromQuery]Guid projectId)
        {
            var command = new ChoseTestCommand { TestResultId = testResultId,  ProjectId = projectId};
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [ProducesResponseType(typeof(MethodResult<SingleTestStateModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [HttpPost("submit-answer")]
        public async Task<IActionResult> GetTestState([FromBody] TestSubmitAnswerCommand submitCommand)
        {
            var queryResult = await _mediator.Send(submitCommand).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
