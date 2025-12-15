// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.V1i2
{
    using Application.Commands.TestCmd;
    using Application.Queries.TestQuery;
    using Common.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Attributes;
    using Shared.Constants;

    [ApiVersions(ApiSettings.APIVersion1i2)]
    [Route(Settings.APIDefaultRoute + "/test")]
    [ApiController]
    //[Permission(role: nameof(EnumRole.Student))]
    public class TestController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TestController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("get-test-section-result-detail/{id:guid}")]
        public async Task<IActionResult> GetTestSectionResultDetail(Guid id)
        {
            var getSectionResultDetailQuery = new GetTestSectionResultDetailQuery { SectionResultId = id };
            var queryResult = await _mediator.Send(getSectionResultDetailQuery).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [HttpGet("get-test-result-detail/{id:guid}")]
        public async Task<IActionResult> GetTestResultDetail(Guid id)
        {
            var getTestResultDetailQuery = new GetTestResultDetailQuery { TestResultId = id };
            var queryResult = await _mediator.Send(getTestResultDetailQuery).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
        [HttpPost("start-or-continue/{testResultId:guid}")]
        public async Task<IActionResult> SelectTestProgramId(Guid testResultId, [FromQuery]Guid projectId)
        {
            var command = new ChoseTestCommand { TestResultId = testResultId,  ProjectId = projectId};
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [HttpPost("submit-answer")]
        public async Task<IActionResult> GetTestState([FromBody] TestSubmitAnswerCommand submitCommand)
        {
            var queryResult = await _mediator.Send(submitCommand).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
