// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.PlacementTestCmd;
    using Fsel.Course.Lms.Application.Queries.PlacementTestQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
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
        /// Get Levels By Student
        /// </summary>
        [HttpGet("levels")]
       [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        [ProducesResponseType(typeof(MethodResult<object>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLevelsByStudentsAsync([FromQuery] GetLevelsByStudentQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Choose Student Course
        /// </summary>
        [HttpPost("choose-student-course")]
       [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ChooseStudentCourse([FromBody] ChooseStudentLevelToCourseCommand command)
        {
            MethodResult<bool> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get PlacementTest
        /// </summary>
        [HttpGet("level")]
       [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        [ProducesResponseType(typeof(MethodResult<PlacementTestBankModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] GetPlacementTestQuery query)
        {
            MethodResult<PlacementTestBankModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
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
       [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
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
       [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
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
       [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        [ProducesResponseType(typeof(MethodResult<IList<PlacementTestResultModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        public async Task<IActionResult> CreateAnswer([FromBody] CreatePlacementTestAnswerCommand command)
        {
            MethodResult<IList<PlacementTestResultModel>> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Check done pt by StudentId
        /// </summary>
        [HttpGet("check-done-pt/{studentId}")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Admin), nameof(EnumRole.AdminSchool), nameof(EnumRole.CSO) })]
        public async Task<IActionResult> CheckDonePTByStudentId([FromRoute] Guid studentId)
        {
            MethodResult<bool> queryResult = await _mediator.Send(new CheckDonePTByStudentIdQuery { StudentId = studentId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [HttpPost("select/{programId}")]
        public async Task<IActionResult> SelectPTFlowByProgramId(Guid programId)
        {
            var chosePtFlowCommand = new ChosePTFlowCommand(programId);
            var queryResult = await _mediator.Send(chosePtFlowCommand).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [HttpPost("continue/{studentId}")]
        public async Task<IActionResult> GetPTFlowForStudent(Guid studentId)
        {
            var continueCommand = new ContinuePTCommand
            {
                StudentId = studentId
            };
            var queryResult = await _mediator.Send(continueCommand).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [HttpPost("submit-answer")]
        public async Task<IActionResult> GetPtState([FromBody] SubmitAnswerCommand submitCommand)
        {
            var queryResult = await _mediator.Send(submitCommand).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
