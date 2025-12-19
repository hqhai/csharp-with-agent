// Copyright (c) Atlantic. All rights reserved.
using System.Net;
using Asp.Versioning;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Lms.Application.Commands.MockTestCmd;
using Fsel.Course.Lms.Application.Queries.MockTestQuery;
using Fsel.Shared.Attributes;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Course.Lms.Api.Controllers
{
    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/mock-test")]
    [ApiController]
   [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
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
        [MapToApiVersion(ApiSettings.APIVersion1)]
        public async Task<IActionResult> CreateMockTestAnswers([FromBody] CreateMockTestAnswerCommand command)
        {
            MethodResult<MockTestResultModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get MockTest
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<MockTestModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] GetMockTestByIdQuery query)
        {
            MethodResult<MockTestModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Sections
        /// </summary>
        [HttpGet("sections")]
        [ProducesResponseType(typeof(MethodResult<SectionGroupDtoModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetSections([FromQuery] GetSectionBySectionGroupIdQuery query)
        {
            MethodResult<SectionGroupDtoModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
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