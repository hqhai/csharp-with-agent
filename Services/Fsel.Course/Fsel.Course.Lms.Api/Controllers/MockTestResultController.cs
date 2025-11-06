// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.MockTestResultCmd;
    using Fsel.Course.Lms.Application.Queries.MockTestResultQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/mock-test-result")]
    [ApiController]
    [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
    public class MockTestResultController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MockTestResultController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>s
        /// Get Mock tesk result
        /// </summary>
        [HttpGet("{mockTestResultId}")]
        [ProducesResponseType(typeof(MethodResult<MockTestResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetMockTestResultById([FromRoute] Guid mockTestResultId)
        {
            MethodResult<MockTestResultModel> queryResult = await _mediator.Send(new GetMockTestReportQuery { MockTestResultId = mockTestResultId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>s
        /// ReportFeedback Mock tesk result
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<StudentFeedbackModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ReportFeedback([FromBody] ReportFeedbackMockTestCommand command)
        {
            MethodResult<StudentFeedbackModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
