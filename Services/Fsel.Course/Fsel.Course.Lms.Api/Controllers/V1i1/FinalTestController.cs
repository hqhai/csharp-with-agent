// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.V1i1
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.FinalTestAnswerV1i1Cmd;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Asp.Versioning;
    using Fsel.Shared.Constants;

    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/final-test")]
    [ApiController]
    [Common.Attributes.Permission(role: nameof(EnumRole.Student))]
    public class FinalTestController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FinalTestController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create FinalTestAnswers
        /// </summary>
        [HttpPost("final-test-answers")]
        [ProducesResponseType(typeof(MethodResult<SectionGroupResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateAnswers([FromBody] CreateFinalTestAnswerBySectionGroupCommand command)
        {
            MethodResult<SectionGroupResultModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
