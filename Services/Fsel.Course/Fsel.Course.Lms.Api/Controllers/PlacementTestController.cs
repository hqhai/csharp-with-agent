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
    public class PlacementTestController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PlacementTestController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// get PlacementTest
        /// </summary>
        [HttpGet("level")]
        [Authorize(Roles = nameof(EnumRole.Student))]
        [ProducesResponseType(typeof(MethodResult<PlacementTestBankModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] GetPlacementTestQuery command)
        {
            MethodResult<PlacementTestBankModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Check Result by StudentId
        /// </summary>
        [HttpGet("check-result/{studentId}")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CheckResultByStudentId([FromRoute] Guid studentId)
        {
            MethodResult<bool> queryResult = await _mediator.Send(new CheckResultByStudentIdQuery { StudentId = studentId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// count result by StudentId
        /// </summary>
        [HttpGet("count-result/{studentId}")]
        [ProducesResponseType(typeof(MethodResult<int>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CountResultByStudentId([FromRoute] Guid studentId)
        {
            MethodResult<int> queryResult = await _mediator.Send(new GetCountPlacementTestResultQuery { StudentId = studentId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get PlacementTest Result
        /// </summary>
        [HttpGet("get-result")]
        [Authorize(Roles = nameof(EnumRole.Student))]
        [ProducesResponseType(typeof(MethodResult<PlacementTestResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetResult()
        {
            MethodResult<PlacementTestResultModel> queryResult = await _mediator.Send(new GetPlacementTestResultQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get list PlacementTest Result
        /// </summary>
        [HttpGet("get-list-result")]
        [Authorize(Roles = nameof(EnumRole.Student))]
        [ProducesResponseType(typeof(MethodResult<IList<PlacementTestResultModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListResult()
        {
            MethodResult<IList<PlacementTestResultModel>> queryResult = await _mediator.Send(new GetListPlacementTestResultQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create PlacementTest Answers
        /// </summary>
        [HttpPost("create-answers")]
        [Authorize(Roles = nameof(EnumRole.Student))]
        [ProducesResponseType(typeof(MethodResult<IList<PlacementTestResultModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateAnswer([FromBody] CreatePlacementTestAnswerCommand command)
        {
            MethodResult<IList<PlacementTestResultModel>> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
