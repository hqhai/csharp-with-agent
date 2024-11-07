// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers.Admin
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Application.Commands.AdminCmd;
    using Fsel.Identity.Application.Commands.StudentCmd;
    using Fsel.Identity.Application.Queries.AdminQuery;
    using Fsel.Identity.Application.Queries.ManagerReportQuery;
    using Fsel.Identity.Application.Queries.StudentQuery;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/admin/student")]
    [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Admin), nameof(EnumRole.AdminSchool) })]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StudentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Class Forum
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<SearchStudentsInClassModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchStudentsInClassQuery query)
        {
            MethodResult<PagingItemsModel<SearchStudentsInClassModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Update User Student
        /// </summary>
        [HttpPut("{studentId}")]
        [ProducesResponseType(typeof(MethodResult<StudentModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateStudent([FromRoute] Guid studentId, [FromBody] UpdateStudentByAdminCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = studentId;
            MethodResult<StudentModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Profile Student
        /// </summary>
        [HttpGet("profile/{studentId}")]
        [ProducesResponseType(typeof(MethodResult<StudentModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProfileStudent([FromRoute] Guid studentId)
        {
            MethodResult<StudentModel> queryResult = await _mediator.Send(new GetStudentProfileQuery { StudentId = studentId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Survey Student
        /// </summary>
        [HttpGet("survey/{studentId}")]
        [ProducesResponseType(typeof(MethodResult<IList<StudentSurveyQuestionModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetSurveyStudent([FromRoute] Guid studentId)
        {
            MethodResult<IList<StudentSurveyQuestionModel>> queryResult = await _mediator.Send(new GetStudentSurveyQuestionQuery { StudentId = studentId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search Course Student
        /// </summary>
        [HttpGet("course")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<StudentCourseModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchStudentCourse([FromQuery] SearchStudentCourseQuery query)
        {
            MethodResult<PagingItemsModel<StudentCourseModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search Student Lesson Comment
        /// </summary>
        [HttpGet("lesson-comment")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<StudentLessonCommentModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchStudentLessonComment([FromQuery] SearchStudentLessonCommentQuery query)
        {
            MethodResult<PagingItemsModel<StudentLessonCommentModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search Student
        /// </summary>
        [HttpGet("management")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<StudentSearchAdminModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchStudent([FromQuery] SearchStudentsQuery query)
        {
            MethodResult<PagingItemsModel<StudentSearchAdminModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// delete
        /// </summary>
        [HttpDelete("delete-user/{id}")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> DeleteStudentFromClass([FromRoute] Guid id)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new DeleteListDataStudentCommand { UserId = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Student by UserId
        /// </summary>
        [HttpGet("get-by-user-id/{id}")]
        [ProducesResponseType(typeof(MethodResult<StudentModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetByUserId([FromRoute] Guid id)
        {
            MethodResult<StudentModel> commandResult = await _mediator.Send(new GetStudentByUserIdQuery { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Search Student
        /// </summary>
        [HttpPost("search")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<StudentDtoModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Gets([FromBody] SearchStudentQuery query)
        {
            MethodResult<PagingItemsModel<StudentDtoModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Student
        /// </summary>
        [HttpPost("gets")]
        [ProducesResponseType(typeof(MethodResult<IList<StudentDtoModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Gets([FromBody] GetStudentsQuery query)
        {
            MethodResult<IList<StudentDtoModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get SchoolClass
        /// </summary>
        [HttpGet("get-school-class")]
        [ProducesResponseType(typeof(MethodResult<IList<string>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Gets([FromQuery] GetSchoolClassBySchoolGradeQuery query)
        {
            MethodResult<IList<string>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
