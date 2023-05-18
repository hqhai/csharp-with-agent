// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.PlacementTestCmd;
    using Fsel.Course.Lms.Application.Queries.PlacementTestQuery;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/placement-test")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.Student))]
    public class PlacmentTestController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PlacmentTestController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// get PlacementTest
        /// </summary>
        [HttpGet("placement-test")]
        [ProducesResponseType(typeof(MethodResult<PlacementTestBankModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] GetPlacementTestQuery command)
        {
            MethodResult<PlacementTestBankModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create PlacementTest Answer
        /// </summary>
        [HttpPost("placement-test-answer")]
        [ProducesResponseType(typeof(MethodResult<PlacementTestResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateAnswer([FromBody] CreatePlacementTestAnswerCommand command)
        {
            MethodResult<PlacementTestResultModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Update PlacementTest Result
        /// </summary>
        [HttpPost("placement-test-result")]
        [ProducesResponseType(typeof(MethodResult<List<PlacementTestResultModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateResult([FromQuery] UpdatePlacementTestResultCommand command)
        {
            MethodResult<List<PlacementTestResultModel>> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Update Student in Level
        /// </summary>
        [HttpPost("student-by-level")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateStudentByLevel([FromQuery] UpdateCourseLevelStudentCommand command)
        {
            MethodResult<bool> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
