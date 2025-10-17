// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers.Admin
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Application.Commands.AdminCmd;
    using Fsel.Identity.Application.Commands.StudentCmd;
    using Fsel.Identity.Application.Commands.UserCmd;
    using Fsel.Identity.Application.Queries.AdminQuery;
    using Fsel.Identity.Application.Queries.ManagerReportQuery;
    using Fsel.Identity.Application.Queries.ParentQuery;
    using Fsel.Identity.Application.Queries.StudentEditHistoryQuery;
    using Fsel.Identity.Application.Queries.StudentQuery;
    using Fsel.Identity.Application.Queries.UserQuery;
    using Fsel.Identity.Application.Queries.UserOtpCodeQuery;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/admin/student")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StudentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Students
        /// </summary>
        [HttpGet("search-students")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<StudentSearchAdminModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [EncryptResponse]
        [Permission(StudentManagement.View)]
        public async Task<IActionResult> SearchStudent([FromQuery] SearchStudentsByAdminQuery query)
        {
            MethodResult<PagingItemsModel<StudentSearchAdminModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search Class Forum
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<SearchStudentsInClassModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(StudentManagement.View)]
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
        [Permission(permissionCodes: new[] { StudentManagement.Update, SchoolStudentManagement.Update })]
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
        [EncryptResponse]
        [Permission(permissionCodes: new[] { StudentManagement.View, SchoolStudentManagement.View })]
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
        [Permission(permissionCodes: new[] { StudentManagement.View, SchoolStudentManagement.View })]
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
        [Permission(permissionCodes: new[] { StudentManagement.View, SchoolStudentManagement.View })]
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
        [Permission(permissionCodes: new[] { StudentManagement.View, SchoolStudentManagement.View })]
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
        [Permission(permissionCodes: new[] { StudentManagement.View, SchoolStudentManagement.View })]
        [EncryptResponse]
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
        [Permission(permissionCodes: new[] { StudentManagement.Update, SchoolStudentManagement.Update })]
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
        [Permission(permissionCodes: new[] { StudentManagement.View, SchoolStudentManagement.View })]
        public async Task<IActionResult> GetByUserId([FromRoute] Guid id)
        {
            MethodResult<StudentModel> commandResult = await _mediator.Send(new GetStudentByUserIdQuery { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Student by UserId
        /// </summary>
        [HttpGet("tool-update-beginner-guide")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(StudentManagement.Update)]
        public async Task<IActionResult> ToolUpdateBeginnerGuide([FromQuery] Guid? id)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new ToolUpdateBeginnerGuideStudentCommand { StudentId = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Search Student
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<StudentDtoModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(permissionCodes: new[] { StudentManagement.View, SchoolStudentManagement.View })]
        public async Task<IActionResult> Gets([FromQuery] SearchStudentQuery query)
        {
            MethodResult<PagingItemsModel<StudentDtoModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Student
        /// </summary>
        [HttpGet("gets")]
        [ProducesResponseType(typeof(MethodResult<IList<StudentDtoModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(permissionCodes: new[] { StudentManagement.View, SchoolStudentManagement.View })]
        public async Task<IActionResult> Gets([FromQuery] GetStudentsQuery query)
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
        [Permission(permissionCodes: new[] { StudentManagement.View, SchoolStudentManagement.View })]
        public async Task<IActionResult> Gets([FromQuery] GetSchoolClassBySchoolGradeQuery query)
        {
            MethodResult<IList<string>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Student
        /// </summary>
        [HttpGet("get-dashboards")]
        [ProducesResponseType(typeof(MethodResult<IList<StudentDtoModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(HomeDashboard.ViewDashboard)]
        public async Task<IActionResult> Gets([FromQuery] GetStudentsDashboardQuery query)
        {
            MethodResult<IList<StudentDtoModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// delete student by userid
        /// </summary>
        [HttpDelete("delete-user-by-userid/{userId}")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        [Permission(permissionCodes: new[] { StudentManagement.Update, SchoolStudentManagement.Update })]
        public async Task<IActionResult> DeleteStudentByStudentId([FromRoute] Guid userId)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new DeleteStudentByUserIdCommand { UserId = userId }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Search school grades classes
        /// </summary>
        [HttpGet("get-school-grades-classes")]
        [ProducesResponseType(typeof(MethodResult<StudentSearchAdminModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(permissionCodes: new[] { StudentManagement.View, SchoolStudentManagement.View })]
        public async Task<IActionResult> GetSchoolGradeClass()
        {
            var queryResult = await _mediator.Send(new GetSchoolGradesAndClassesQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Tính năng này mới chỉ dùng để cập nhật thời gian export
        /// </summary>
        [HttpPut("update-event-content")]
        [ProducesResponseType(typeof(MethodResult<StudentSearchAdminModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(roles: new string[] { nameof(EnumRole.Admin), nameof(EnumRole.AdminSchool), nameof(EnumRole.CSO) })]
        public async Task<IActionResult> UpdateEventContentTime([FromBody] UpdateEventExportTimeCommand cmd)
        {
            var queryResult = await _mediator.Send(cmd).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// change school admin
        /// </summary>
        [HttpPut("change-school")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(StudentManagement.Update)]
        public async Task<IActionResult> ChangeSchoolByAdmin([FromBody] ChangeSchoolByAdminCommand command)
        {
            MethodResult<bool> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get parent by student id
        /// </summary>
        [HttpGet("get-parent-by-student-id/{studentId}")]
        [ProducesResponseType(typeof(MethodResult<ParentProfileModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(permissionCodes: new[] { StudentManagement.View, SchoolStudentManagement.View })]
        public async Task<IActionResult> GetParentByStudentId([FromRoute] Guid studentId)
        {
            var queryResult = await _mediator.Send(new GetParentByStudentIdQuery { StudentId = studentId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get otp for student
        /// </summary>
        [HttpGet("get-otp-for-student")]
        [ProducesResponseType(typeof(MethodResult<OTPModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(permissionCodes: new[] { StudentManagement.View, SchoolStudentManagement.View })]
        public async Task<IActionResult> GetOTPForStudent([FromQuery] GetOTPForStudentQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search school grades classes
        /// </summary>
        [HttpPost("add-student-event")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(StudentManagement.Update)]
        public async Task<IActionResult> AddStudentToEvent([FromBody] AddStudentToEventCommand command)
        {
            var methodResult = await _mediator.Send(command).ConfigureAwait(false);
            return methodResult.GetActionResult();
        }

        /// <summary>
        /// Restore deleted account
        /// </summary>
        [HttpPost("restore-deleted-account")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(StudentManagement.Update)]
        public async Task<IActionResult> RestoreDeletedAccount([FromBody] RestoreDeleteAccountCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// cập nhật expired date cho students
        /// </summary>
        [HttpPut("update-expired-date-for-student")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(StudentManagement.Update)]
        public async Task<IActionResult> UpdateExpiredDateStudentHasValue([FromBody] UpdateExpiredDateStudentHasValueCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Create user and order
        /// </summary>
        [HttpPost("create-students-and-orders")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(StudentManagement.Add)]
        public async Task<IActionResult> CreateUserAndOrder([FromBody] CreateStudentsAndOrdersByAdminCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Create users and orders from gg sheet
        /// </summary>
        [HttpPost("create-students-and-orders-from-gg-sheet")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(StudentManagement.Add)]
        public async Task<IActionResult> CreateUsersAndOrdersAndGGSheet([FromBody] CreateStudentsAndOrdersFromGGSheetCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Tool Synchronous Parent Info
        /// Update Profile User
        /// </summary>
        [HttpPut("update-profile-user")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(StudentManagement.Update)]
        public async Task<IActionResult> UpdateProfileUser([FromBody] UpdateProfileUserCommand command)
        {
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Profile User Manage
        /// </summary>
        [HttpGet("get-user-manage")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<UserManageModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(StudentManagement.View)]
        public async Task<IActionResult> UpdateProfileUser([FromQuery] GetUserManagesByRoleQuery query)
        {
            MethodResult<PagingItemsModel<UserManageModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Profile User Manage
        /// </summary>
        [HttpGet("users-by-roles")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<UserModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(StudentManagement.View)]
        public async Task<IActionResult> GetUsersByRoles([FromQuery] GetUsersByRolesQuery query)
        {
            MethodResult<PagingItemsModel<UserModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search school grades classes
        /// </summary>
        [HttpPost("tool-synchronous-parent-info")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(StudentManagement.Update)]
        public async Task<IActionResult> ToolSynchronousParentInfo()
        {
            var queryResult = await _mediator.Send(new ToolSynchronousParentInfoCommand()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get student edit histories
        /// </summary>
        [HttpGet("get-student-edit-histories")]
        [ProducesResponseType(typeof(MethodResult<IList<StudentEditHistoryModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(StudentManagement.View)]
        public async Task<IActionResult> GetStudentEditHistories([FromQuery] GetStudentEditHistoriesByStudentIdQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// search students by user ids
        /// </summary>
        [HttpPost("search-students-by-user-ids")]
        [ProducesResponseType(typeof(MethodResult<IList<StudentModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchStudentsByUserIds([FromBody] SearchStudentsByUserIdsQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
