// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.FinalTestCmd;
    using Fsel.Course.Lms.Application.Queries.FinalTestQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
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
        /// get FinalTest
        /// </summary>
        [HttpGet("final-test")]
        [ProducesResponseType(typeof(MethodResult<FinalTestModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] GetFinalTestQuery command)
        {
            MethodResult<FinalTestModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create FinalTest Answers
        /// </summary>
        [HttpPost("final-test-answers")]
        [ProducesResponseType(typeof(MethodResult<FinalTestResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        public async Task<IActionResult> CreateAnswer([FromBody] CreateFinalTestAnswerCommand command)
        {
            MethodResult<FinalTestResultModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
