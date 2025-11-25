// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.PlacementTestCmd.V1i1;
    using Fsel.Course.Lms.Application.Queries.PlacementTestResultQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/placement-test-result")]
    [ApiController]
    [Common.Attributes.Permission]
    public class PlacementTestResultController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PlacementTestResultController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Level By Student
        /// </summary>
        [HttpGet("level")]
       [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        [ProducesResponseType(typeof(MethodResult<PlacementTestDtoModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLevelByStudentAsync()
        {
            var queryResult = await _mediator.Send(new GetPlacementTestByLevelQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Sections By SectionGroup
        /// </summary>
        [HttpGet("sections")]
       [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        [ProducesResponseType(typeof(MethodResult<SectionGroupDtoModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetSectionsBySectionGroup([FromQuery] GetSectionBySectionGroupIdQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create PlacementTest Answers
        /// </summary>
        [HttpPost("create-answers")]
       [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        [ProducesResponseType(typeof(MethodResult<PlacementTestResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateAnswer([FromBody] CreatePlacementTestAnswerBySectionGroupCommand command)
        {
            MethodResult<PlacementTestResultModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Export PlacementTest
        /// </summary>
        [HttpGet("export")]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> Export([FromQuery] ExportPlacementTestQuery query)
        {
            MethodResult<Stream> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "placementTest_export.xlsx");
        }

        /// <summary>
        /// Export PlacementTest
        /// </summary>
        [HttpPost("export-file-pts")]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> Export([FromQuery] ExportPlacementTestByStudentsQuery query)
        {
            MethodResult<Stream> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "Export_File_PlacementTests.xlsx");
        }

        /// <summary>
        /// Export PlacementTest
        /// </summary>
        [HttpPost("export-placement-test")]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> Export([FromForm] ExportModulePlacementTestsToStudentQuery query)
        {
            MethodResult<Stream> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "Export_Module_PlacementTests.xlsx");
        }

        /// <summary>
        /// send mail pt
        /// </summary>
        [HttpPost("send-mail-pt")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SendMailPT([FromQuery] SendPTCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Save choose Level PT
        /// </summary>
        [HttpPost("choose-level-pt")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SavePlacementTestGroupResult([FromBody] SavePlacementTestGroupResultCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Config Placement Test
        /// </summary>
        [HttpGet("get-config-placement-test")]
        [ProducesResponseType(typeof(MethodResult<PlacementTestReportOveallModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetConfigPlacementTest()
        {
            var queryResult = await _mediator.Send(new GetConfigPlacementTestQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Placement Test By Student Id
        /// </summary>
        [HttpGet("placement-test/{studentId}")]
        [ProducesResponseType(typeof(MethodResult<GetPlacementTestResultByStudentModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPlacementTestResultByStudentId([FromRoute] Guid studentId)
        {
            var queryResult = await _mediator.Send(new GetPlacementTestResultByStudentIdQuery { StudentId = studentId }).ConfigureAwait(false);
            return queryResult.GetActionResult();


        }

        /// <summary>
        /// Get Placement Test Menu - Assessment Tree
        /// </summary>
        [HttpGet("process-tree")]
        [ProducesResponseType(typeof(MethodResult<AssessmentTreeModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPlacementTestMenu([FromQuery] Guid? studentId)
        {
            var queryResult = await _mediator.Send(new GetPlacementTestMenuQuery { StudentId = studentId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }

}
