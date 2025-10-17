// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.V1i1
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.MockTestCmd.V1i1;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1i1)]
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
        [ProducesResponseType(typeof(MethodResult<SectionGroupResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateAnswers([FromBody] CreateMockTestAnswerBySectionGroupCommand command)
        {
            MethodResult<SectionGroupResultModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create MockTestAnswers
        /// </summary>
        [HttpPost("test-azure")]
        [ProducesResponseType(typeof(MethodResult<PronunciationAssessmentModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> TestAzurePronunciation([FromBody] AzurePronTestCmd command)
        {
            MethodResult<PronunciationAssessmentModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
