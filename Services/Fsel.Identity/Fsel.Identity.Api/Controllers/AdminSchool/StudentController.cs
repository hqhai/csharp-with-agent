// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers.AdminSchool
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Identity.Application.Commands.StudentCmd;
    using Fsel.Identity.Application.Queries.AdminQuery;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/admin-school/student")]
    [Common.Attributes.Permission(role: nameof(EnumRole.AdminSchool))]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IUserSchoolRepository _userSchoolRepository;

        public StudentController(IMediator mediator, IUserSchoolRepository userSchoolRepository)
        {
            _mediator = mediator;
            _userSchoolRepository = userSchoolRepository;
        }

        /// <summary>
        /// Get SchoolId
        /// </summary>
        [HttpGet("schoolId")]
        [ProducesResponseType(typeof(MethodResult<Guid>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetSchoolId()
        {
            MethodResult<Guid> methodResult = new MethodResult<Guid>();
            methodResult.Result = await _userSchoolRepository.GetSchoolIdAsync();
            return methodResult.GetActionResult();
        }

        /// <summary>
        /// Get Student
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<StudentModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get()
        {
            MethodResult<IList<StudentModel>> methodResult = new MethodResult<IList<StudentModel>>();
            methodResult.Result = await _userSchoolRepository.GetStudentsByRoleAdminSchoolAsync();
            return methodResult.GetActionResult();
        }

        [RequestSizeLimit(1 * 1024 * 1024)] // 1 MB
        [RequestFormLimits(MultipartBodyLengthLimit = 1 * 1024 * 1024)]
        [HttpPost("create-students-to-event-from-file")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateStudentsFromFile([FromForm] CreateStudentsFromFileCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// export template create students to event
        /// </summary>
        [HttpPost("export-template-create-students-to-event-from-file")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ExportTemplateStudentsIntoPlatform()
        {
            MethodResult<Stream> commandResult = await _mediator.Send(new ExportTemplateCreateStudentsToEventFromFileCommand { }).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "Template_Import_Students.xlsx");
        }

        /// <summary>
        /// cập nhật expired date cho students
        /// </summary>
        [HttpDelete("delete-students")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DeleteStudents([FromBody] DeleteStudentsInEventByAdminSchoolCommand cmd)
        {
            var commandResult = await _mediator.Send(cmd).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Export students
        /// </summary>
        [HttpGet("export-students")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ExportStudents([FromQuery] ExportStudentsByAdminSchoolQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            if (!queryResult.IsOK || queryResult.Result == null)
            {
                return queryResult.GetActionResult();
            }
            return File(queryResult.Result, Settings.Excels.ContentType, "students_report.xlsx");
        }

        /// <summary>
        /// cập nhật expired date cho students
        /// </summary>
        [HttpPut("update-expired-date-for-students")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateCompetitionEvents([FromBody] UpdateExpiredDateForStudentsEventCommand cmd)
        {
            var commandResult = await _mediator.Send(cmd).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
