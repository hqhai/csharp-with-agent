// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers.Campus
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Application.Commands.AdminCmd;
    using Fsel.Identity.Application.Commands.CampusCmd;
    using Fsel.Identity.Application.Commands.CampusCmd.Classes;
    using Fsel.Identity.Application.Queries.CampusQuery.Classes;
    using Fsel.Identity.Domain.Models;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/campus/school-class")]
    [ApiController]
    public class SchoolClassController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SchoolClassController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// delete students in class
        /// </summary>
        [HttpPost("delete-students-in-class")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(SchoolClassCampusManagement.DeleteStudents)]
        public async Task<IActionResult> DeleteStudentsInClass([FromBody] DeleteStudentsInClassCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// save class
        /// </summary>
        [HttpPost("save")]
        [ProducesResponseType(typeof(MethodResult<SchoolClassModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(SchoolClassCampusManagement.Update)]
        [Permission(SchoolClassCampusManagement.Add)]
        public async Task<IActionResult> Save([FromBody] SaveSchoolClassCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// save class
        /// </summary>
        [HttpPost("delete")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(SchoolClassCampusManagement.Delete)]
        public async Task<IActionResult> Delete([FromBody] DeleteSchoolClassCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// search
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<SchoolClassModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(SchoolClassCampusManagement.View)]
        public async Task<IActionResult> Search([FromQuery] SearchSchoolClassQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// search
        /// </summary>
        [HttpGet("get-by-id")]
        [ProducesResponseType(typeof(MethodResult<SchoolClassModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(SchoolClassCampusManagement.View)]
        [ServerCache(CacheSettings.TimeCache.TenMinutes)]
        public async Task<IActionResult> GetSchoolClassById([FromQuery] GetSchoolClassByIdQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Add students to class
        /// </summary>
        [HttpPost("add-students-to-class")]
        [ProducesResponseType(typeof(MethodResult<AddStudentIntoSchoolClassCommandModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(SchoolClassCampusManagement.AddStudents)]
        public async Task<IActionResult> ImportStudentsIntoPlatform([FromForm] AddStudentIntoSchoolClassCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null || commandResult.Result.Stream == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result.Stream, Settings.Excels.ContentType, "Add_Students_To_Class_Error.xlsx");
        }

        /// <summary>
        /// search teacher campus
        /// </summary>
        [HttpGet("search-teacher-campus")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<TeacherModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(SchoolClassCampusManagement.View)]
        public async Task<IActionResult> SearchTeacher([FromQuery] SearchTeacherCampusQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// export template add students to curriculum
        /// </summary>
        [HttpPost("export-template-add-students-to-school-class")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(SchoolClassCampusManagement.AddStudents)]
        public async Task<IActionResult> ExportTemplateAddStudentsToSchoolClass()
        {
            MethodResult<Stream> commandResult = await _mediator.Send(new ExportTemplateAddStudentsToSchoolClassCommand { }).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "Template_Add_Student_To_Class.xlsx");
        }

        /// <summary>
        /// search create users info school class
        /// </summary>
        [HttpGet("search-created-users-info-school-class")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<EntityModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(SchoolClassCampusManagement.View)]
        public async Task<IActionResult> SearchCreateUsersInfoSchoolClass([FromQuery] SearchCreateUsersInfoSchoolClassQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update Student
        /// </summary>
        [HttpPut("update-student-info/{studentId}")]
        [ProducesResponseType(typeof(MethodResult<StudentModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(StudentCampusManagement.Update)]
        public async Task<IActionResult> UpdateStudent([FromRoute] Guid studentId, [FromBody] UpdateStudentByAdminCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = studentId;
            MethodResult<StudentModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Change Password
        /// </summary>
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(StudentCampusManagement.Update)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordUserCommand command)
        {
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Search Class By School
        /// </summary>
        [HttpGet("search-class-by-school")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<SchoolClassModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission]
        public async Task<IActionResult> Search([FromQuery] SearchClassBySchoolQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update info students campus
        /// </summary>
        [HttpPost("update-info-students-campus")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(StudentCampusManagement.Update)]
        public async Task<IActionResult> UpdateInfoStudentCampus([FromForm] UpdateInfoStudentCampusCommand command)
        {
            MethodResult<Stream> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "File_Lỗi.xlsx");
        }
    }
}
