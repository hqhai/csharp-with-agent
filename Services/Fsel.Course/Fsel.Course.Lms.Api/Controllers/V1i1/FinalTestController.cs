// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.V1i1
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.FinalTestCmd.V1i1;
    using Fsel.Course.Lms.Application.Queries.FinalTestQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/final-test")]
    [ApiController]
   [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
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

        /// <summary>
        /// Get FinalTest
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<FinalTestModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetFinalTest([FromQuery] GetFinalTestByIdQuery query)
        {
            MethodResult<FinalTestModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
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
    }
}
