// Copyright (c) Atlantic. All rights reserved.
using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Lms.Application.Commands.MockTestCmd;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Course.Lms.Api.Controllers
{
    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/mock-test")]
    [ApiController]
    [Common.Attributes.Permission(role: nameof(EnumRole.Student))]
    public class MockTestController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MockTestController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create MockTestAnswers
        /// </summary>
        [HttpPost("create-mock-test-answer")]
        [ProducesResponseType(typeof(MethodResult<MockTestResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateMockTestAnswers([FromBody] CreateMockTestAnswerCommand command)
        {
            MethodResult<MockTestResultModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Start MockTest
        /// </summary>
        [HttpGet("start-mock-test")]
        [ProducesResponseType(typeof(MethodResult<MockTestModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> StartMockTest([FromQuery] StartMockTestCommand query)
        {
            MethodResult<MockTestModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
