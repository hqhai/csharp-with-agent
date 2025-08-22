// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Asp.Versioning;
using Fsel.Common.ActionResults;
using Fsel.Common.Attributes;
using Fsel.Common.Constants;
using Fsel.Core.Base.BaseModels;
using Fsel.Identity.Application.Commands.AdminCmd;
using Fsel.Identity.Application.Commands.AuthCmd;
using Fsel.Identity.Application.Commands.StudentCmd;
using Fsel.Identity.Application.Queries.AdminQuery;
using Fsel.Identity.Application.Queries.UserQuery;
using Fsel.Identity.Application.Queries.UserReferrals;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Models.EntityModels;
using Fsel.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Identity.Api.Controllers.Admin
{
    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/admin/user")]
    [ApiController]
    [Permission]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create User
        /// </summary>
        [HttpPost("create-user")]
        [ProducesResponseType(typeof(MethodResult<UserModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(UserManagement.Add)]
        public async Task<IActionResult> Create([FromBody] CreateUserCommand command)
        {
            MethodResult<UserModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Create User to LMS Admin platform
        /// </summary>
        [HttpPost("create-user/lms-admin")]
        [ProducesResponseType(typeof(MethodResult<UserModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(UserManagement.Add)]
        public async Task<IActionResult> CreateToLmsAdminPlat([FromBody] CreateUserToLmsAdminPlatCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update User in LMS Admin platform
        /// </summary>
        [HttpPut("update-user/{id}/lms-admin")]
        [ProducesResponseType(typeof(MethodResult<UserModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(UserManagement.Update)]
        public async Task<IActionResult> UpdateInLmsAdminPlat([FromRoute] Guid id, [FromBody] UpdateUserInLmsAdminCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = id;
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get user by Id in platform
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<UserModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(UserManagement.View)]
        public async Task<IActionResult> GetUserInPlat([FromRoute] Guid id)
        {
            var commandResult = await _mediator.Send(new GetUserInPlatformByUserIdQuery() { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update User
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MethodResult<UserModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(UserManagement.Update)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateUserCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = id;
            MethodResult<UserModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Delete User
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(UserManagement.Delete)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new Application.Commands.AdminCmd.DeleteUserCommand { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Delete List of Users
        /// </summary>
        [HttpPost("delete-user")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(UserManagement.Delete)]
        public async Task<IActionResult> DeleteList([FromBody] DeleteListUsersCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Search User
        /// </summary>
        [HttpGet("search-user")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<UserSearchModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [EncryptResponse]
        [Permission(UserManagement.View)]
        public async Task<IActionResult> SearchUser([FromQuery] SearchUserQuery query)
        {
            MethodResult<PagingItemsModel<UserSearchModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Search User
        /// </summary>
        [HttpGet("search-user/lms-admin")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<UserSearchModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(permissionCodes: new[] { UserGroupManagement.View, UserManagement.View })]
        public async Task<IActionResult> SearchUserInLmsPlat([FromQuery] SearchUsersQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Profile User Manage
        /// </summary>
        [HttpGet("users-by-roles")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<UserModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(permissionCodes: new[] { UserGroupManagement.View, UserManagement.View })]
        public async Task<IActionResult> GetUsersByRoles([FromQuery] SearchUsersByRolesQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get User
        /// </summary>
        [HttpGet("get-user/{id}")]
        [ProducesResponseType(typeof(MethodResult<UserProfileModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(UserManagement.View)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            MethodResult<UserProfileModel> commandResult = await _mediator.Send(new GetUserQuery { UserId = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Active user
        /// </summary>
        [HttpPost("active-user")]
        [ProducesResponseType(typeof(MethodResult<UserModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(UserManagement.Update)]
        public async Task<IActionResult> ActiveUser([FromBody] UpdateStatusUserCommand command)
        {
            MethodResult<UserModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Request teacher bank
        /// </summary>
        [HttpGet("request-teacher-bank")]
        [ProducesResponseType(typeof(MethodResult<UserModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(UserManagement.Update)]
        public async Task<IActionResult> RequestUpdateTeacherBank([FromQuery] ApproveTeacherBankCommand command)
        {
            MethodResult<UserModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        [HttpGet("token/{id}")]
        [ProducesResponseType(typeof(MethodResult<TokenModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(StudentProgressManagement.LoginAsUser)]
        public async Task<IActionResult> GetJWT([FromRoute] Guid id)
        {
            MethodResult<TokenModel> queryResult = await _mediator.Send(new GenerateTokenCommand { Id = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create Student
        /// </summary>
        [HttpPost("create-student")]
        [ProducesResponseType(typeof(MethodResult<UserModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(StudentManagement.Add)]
        public async Task<IActionResult> CreateStudent([FromBody] CreateUserStudentToAdminCommand command)
        {
            MethodResult<UserModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Create Students
        /// </summary>
        [HttpPost("create-students")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(StudentManagement.Add)]
        public async Task<IActionResult> CreateStudents([FromQuery] CreateUserStudentsToAdminCommand command)
        {
            MethodResult<Stream> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "user_import.xlsx");
        }

        /// <summary>
        /// Create Students
        /// </summary>
        [HttpPost("export-template-create-students")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(StudentManagement.Add)]
        public async Task<IActionResult> ExportTemplate()
        {
            MethodResult<Stream> commandResult = await _mediator.Send(new ExportTemplateCreateStudentCommand()).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "export_template_create_student.xlsx");
        }

        /// <summary>
        /// Search User referral
        /// </summary>
        [HttpGet("search-referral-code")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<SearchReferralCodeModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(ReferralCodeManagement.View)]
        public async Task<IActionResult> SearchUserReferral([FromQuery] SearchReferralCodeQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Search detail user referral
        /// </summary>
        [HttpGet("search-detail-referral-code")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<SearchDetailReferralCodeModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(ReferralCodeManagement.View)]
        public async Task<IActionResult> SearchDetailUserReferral([FromQuery] SearchDetailReferralCodeQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Create user by admin
        /// </summary>
        [HttpPost("create-user-by-admin")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(UserManagement.Add)]
        public async Task<IActionResult> CreateUserAndOrder([FromBody] CreateStudentByAdminCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Export template Admin School
        /// </summary>
        [HttpGet("export-template-admin-school")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(UserManagement.Export)]
        public async Task<IActionResult> Export()
        {
            MethodResult<Stream> commandResult = await _mediator.Send(new ExportTemplateCreateAdminSchoolCommand()).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "export_teamplate_admin-school.xlsx");
        }

        /// <summary>
        /// Import File Admin School
        /// </summary>
        [HttpPost("import-admin-school")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(UserManagement.Add)]
        public async Task<IActionResult> Import([FromForm] ImportFileAdminSchoolsCommand command)
        {
            MethodResult<Stream> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "import_file_admin-school.xlsx");
        }

        /// <summary>
        /// Change Password
        /// </summary>
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(UserManagement.Update)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordUserCommand command)
        {
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Tool get otp of student
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpGet("tool-get-otp")]
        [ProducesResponseType(typeof(MethodResult<UserOtpCodeModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(UserManagement.View)]
        public async Task<IActionResult> ToolGetOtp([FromQuery] ToolGetOtpQuery query)
        {
            MethodResult<UserOtpCodeModel> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Import Account Admin School
        /// </summary>
        [HttpPost("import-account-admin-school")]
        [ProducesResponseType(typeof(MethodResult<ImportAccountAdminSchoolModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(UserManagement.Add)]
        public async Task<IActionResult> ImportAccountDashboard([FromForm] ImportAccountAdminSchoolCommand command)
        {
            MethodResult<ImportAccountAdminSchoolModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null || commandResult.Result.Stream == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result.Stream, Settings.Excels.ContentType, "Template_ErrorTaikhoan_AdminSchool.xlsx");
        }

        /// <summary>
        /// Get Account Admin School
        /// </summary>
        [HttpGet("get-account-admin-school")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<AccountAdminSchoolModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(UserManagement.View)]
        public async Task<IActionResult> GetAccountDashboard([FromQuery] GetAccountAdminSchoolCommand query)
        {
            MethodResult<PagingItemsModel<AccountAdminSchoolModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Export Account Admin School
        /// </summary>
        [HttpPost("export-account-admin-school")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(UserManagement.Export)]
        public async Task<IActionResult> ExportAccountDashboard([FromQuery] ExportAcountAdminSchoolCommand command)
        {
            MethodResult<Stream> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "Template_Taikhoan_ExportAccount.xlsx");
        }

        /// <summary>
        /// Active users
        /// </summary>
        [HttpPost("active-users")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(UserManagement.Update)]
        public async Task<IActionResult> ActiveUsers([FromBody] UpdateStatusUsersCommand command)
        {
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
