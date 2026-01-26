// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Domain.Enums;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.PlacementTestCmd;
    using Fsel.Course.Lms.Application.Queries.CategoryQuery;
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
        [Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
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
        [Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
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
        [Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
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
        [Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
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
        [Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
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
        [Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
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
        [ProducesResponseType(typeof(MethodResult<EnumResultStatus>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(roles: new string[] { nameof(EnumRole.Admin), nameof(EnumRole.AdminSchool), nameof(EnumRole.CSO), nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        public async Task<IActionResult> CheckDonePtByStudentId([FromRoute] Guid studentId)
        {
            var queryResult = await _mediator.Send(new CheckDonePtByStudentIdQuery { StudentId = studentId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [HttpGet("get-subjects")]
        [Permission(roles: new[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        public async Task<IActionResult> GetSubjects()
        {
            var getProgramQuery = new GetAllSubjectsQuery();
            var queryResult = await _mediator.Send(getProgramQuery).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [HttpGet("get-section-result-detail/{id:guid}")]
        public async Task<IActionResult> GetTestSectionResultDetail(Guid id)
        {
            var getSectionResultDetailQuery = new GetSectionResultDetailQuery { SectionResultId = id };
            var queryResult = await _mediator.Send(getSectionResultDetailQuery).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [HttpGet("get-test-result-detail/{id:guid}")]
        public async Task<IActionResult> GetTestResultDetail(Guid id)
        {
            var getTestResultDetailQuery = new GetTestResultDetailQuery { TestResultId = id };
            var queryResult = await _mediator.Send(getTestResultDetailQuery).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [HttpPost("select/{programId:guid}")]
        [Permission(roles: new[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        public async Task<IActionResult> SelectPtFlowByProgramId(Guid programId)
        {
            var chosePtFlowCommand = new ChosePtFlowCommand(programId);
            var queryResult = await _mediator.Send(chosePtFlowCommand).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [HttpPost("continue/{studentId:guid}")]
        [Permission(roles: new[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        public async Task<IActionResult> GetPtFlowForStudent(Guid studentId)
        {
            var continueCommand = new ContinuePTCommand { StudentId = studentId };
            var queryResult = await _mediator.Send(continueCommand).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [HttpPost("submit-answer")]
        [Permission(roles: new[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        public async Task<IActionResult> GetPtState([FromBody] SubmitAnswerCommand submitCommand)
        {
            var queryResult = await _mediator.Send(submitCommand).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [HttpGet("get-levels-by-selected-program")]
        [Permission(roles: new[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        public async Task<IActionResult> GetLevelsBySelectedProgram([FromQuery] GetLevelsByProgramQuery request)
        {
            var queryResult = await _mediator.Send(request).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
