// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers.Campus
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Application.Commands.CampusCmd;
    using Fsel.Identity.Application.Commands.StudentCmd;
    using Fsel.Identity.Application.Queries.CampusQuery;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels.CampusModel;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/campus")]
    [ApiController]
    public class CampusController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CampusController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Update expired date for students
        /// </summary>
        [HttpPost("update-expired-date-for-students")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(CurriculumManagement.Update)]
        public async Task<IActionResult> UpdateExpiredDateForStudentsCampus([FromBody] UpdateExpiredDateForStudentsCampusCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search students by student ids
        /// </summary>
        [HttpPost("search-students-by-student-ids")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<StudentCampusModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(StudentCampusManagement.View)]
        public async Task<IActionResult> SearchStudentsByStudentIds([FromBody] SearchStudentsByStudentIdsQuery command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Add course id for student
        /// </summary>
        [HttpPost("add-course-id-for-students")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(CurriculumManagement.Update)]
        public async Task<IActionResult> AddCourseIdForStudents([FromBody] AddCourseIdForStudentsCampusCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Search students by student ids
        /// </summary>
        [HttpGet("search-students")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<StudentCampusModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(StudentCampusManagement.View)]
        public async Task<IActionResult> SearchStudents([FromQuery] SearchStudentsQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Export students
        /// </summary>
        [HttpPost("export-students")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ExportStudents([FromBody] ExportStudentsInfoQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            if (!queryResult.IsOK || queryResult.Result == null)
            {
                return queryResult.GetActionResult();
            }
            return File(queryResult.Result, Settings.Excels.ContentType, "Thông tin học sinh.xlsx");
        }

        /// <summary>
        /// Get list student by student ids
        /// </summary>
        [HttpPost("add-students-to-curriculum")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(CurriculumManagement.AddStudents)]
        public async Task<IActionResult> ImportStudentsIntoPlatform([FromForm] AddStudentsToCurriculumCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            MethodResult<Stream> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "Add_Students_To_Curriculum_Error.xlsx");
        }

        /// <summary>
        /// export template add students to curriculum
        /// </summary>
        [HttpPost("export-template-add-students-to-curriculum")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(CurriculumManagement.AddStudents)]
        public async Task<IActionResult> ExportTemplateAddStudentToCurriculum()
        {
            MethodResult<Stream> commandResult = await _mediator.Send(new ExportTemplateAddStudentsToCurriculumCommand { }).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "Template_Add_Student_To_Curriculum.xlsx");
        }
    }
}