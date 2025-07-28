// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Application.Commands.StudentCmd;
    using Fsel.Identity.Application.Commands.StudentCmd.StudentEventCmd;
    using Fsel.Identity.Application.Commands.UserOtpCodeCmd;
    using Fsel.Identity.Application.Queries.StudentQuery;
    using Fsel.Identity.Application.Queries.StudentQuery.StudentEventQuery;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.UserOtpCodes;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/student")]
    [ApiController]
    [Permission]
    public class StudentController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IStudentRepository _studentRepository;

        public StudentController(IMediator mediator, IStudentRepository studentRepository)
        {
            _mediator = mediator;
            _studentRepository = studentRepository;
        }

        /// <summary>
        /// Execute-list-query
        /// </summary>
        [HttpGet("execute-list-query")]
        [ProducesResponseType(typeof(MethodResult<IList<StudentModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission]
        public async Task<IActionResult> ExecuteList([FromQuery] BaseQueryModel query)
        {
            SetQuery(query);
            var result = await _studentRepository.GetListResultAsync<StudentModel>(query);
            return result.GetActionResult();
        }

        /// <summary>
        /// Get Student by UserId
        /// </summary>
        [HttpGet("get-by-user-id/{id}")]
        //[ServerCache(CacheSettings.TimeCache.TwoMinutes)]
        [ProducesResponseType(typeof(MethodResult<StudentModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetByUserId([FromRoute] Guid id)
        {
            MethodResult<StudentModel> commandResult = await _mediator.Send(new GetStudentByUserIdQuery { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get student by class id
        /// </summary>
        [HttpGet("get-student-by-class-id/{id}")]
        [ProducesResponseType(typeof(MethodResult<IList<StudentModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetStudentByClassId([FromRoute] Guid id)
        {
            MethodResult<IList<StudentModel>> commandResult = await _mediator.Send(new GetStudentByClassIdQuery { ClassId = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get check class more than 12 students
        /// </summary>
        [HttpGet("get-class-has-too-many-students/{id}")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetStudentByClassIdCheck([FromRoute] Guid id)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new GetStudentByClassIdCheckQuery { ClassId = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update Student By Class Id
        /// </summary>
        [HttpPut("update-student-class")]
        [ProducesResponseType(typeof(MethodResult<StudentModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateStudentByClassId([FromBody] UpdateStudentByClassCommand query)
        {
            MethodResult<StudentModel> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update Student By Level
        /// </summary>
        [HttpPut("update-student-level")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateStudentByLevel([FromBody] UpdateStudentByCourseLevelCommand command)
        {
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update Student By Token
        /// </summary>
        [HttpPut("update-student-token")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateStudentByToken([FromBody] UpdateStudentByTokenCommand command)
        {
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get list user by Ids
        /// </summary>
        [HttpPost("get-by-user-ids")]
        [ProducesResponseType(typeof(MethodResult<IList<StudentModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetByIds([FromBody] IList<Guid> ids)
        {
            MethodResult<IList<StudentModel>> commandResult = await _mediator.Send(new GetStudentByUserIdsQuery { UserIds = ids }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get list student by student ids
        /// </summary>
        [HttpPost("get-by-student-ids")]
        [ProducesResponseType(typeof(MethodResult<IList<StudentModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetStudentByIds([FromBody] IList<Guid> ids)
        {
            MethodResult<IList<StudentModel>> commandResult = await _mediator.Send(new GetStudentByIdsQuery { StudentIds = ids }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Delete Student from Class
        /// </summary>
        [HttpPut("delete-student-from-class/{id}")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DeleteStudentFromClass([FromRoute] Guid id)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new DeleteStudentFromClassCommand { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get list student by student ids
        /// </summary>
        [HttpPost("import-students-into-platform")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> ImportStudentsIntoPlatform([FromForm] ImportStudentsIntoPlatformCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            MethodResult<Stream> commandResult = await _mediator.Send(new ImportStudentsIntoPlatformCommand { FormFile = command.FormFile }).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "import-student-into-platform.xlsx");
        }

        /// <summary>
        /// Get list student by student ids
        /// </summary>
        [HttpPost("export-template-students-into-platform")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ExportTemplateStudentsIntoPlatform()
        {
            MethodResult<Stream> commandResult = await _mediator.Send(new ExportTemplateCreateAccountStudentCommand { }).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "Template_Create_Account_Student.xlsx");
        }

        /// <summary>
        /// Update Student By Class Id
        /// </summary>
        [HttpPut("update-beginner-guide")]
        [ProducesResponseType(typeof(MethodResult<StudentModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateBeginnerGuide([FromBody] UpdateStudentBeginnerGuideCommand query)
        {
            MethodResult<StudentModel> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get student by email
        /// </summary>
        [HttpGet("get-student-by-email")]
        [ProducesResponseType(typeof(MethodResult<StudentModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetStudentByEmail([FromQuery] string email)
        {
            MethodResult<StudentModel> commandResult = await _mediator.Send(new GetStudentByEmailQuery { Email = email }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get student by email
        /// </summary>
        [HttpPost("get-student-by-emails")]
        [ProducesResponseType(typeof(MethodResult<IList<StudentModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetStudentByEmails([FromBody] IList<string> emails)
        {
            MethodResult<IList<StudentModel>> commandResult = await _mediator.Send(new GetStudentByEmailsQuery { Emails = emails }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get student by FullNames
        /// </summary>
        [HttpPost("get-student-by-full-names")]
        [ProducesResponseType(typeof(MethodResult<IList<StudentModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetStudentByFullNames([FromBody] IList<string> fullNames)
        {
            MethodResult<IList<StudentModel>> commandResult = await _mediator.Send(new GetStudentByFullNamesQuery { FullNames = fullNames }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update Course To Student
        /// </summary>
        [HttpPut("update-course-to-student/{courseId}")]
        [ProducesResponseType(typeof(MethodResult<StudentModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateCourseToStudent([FromRoute] Guid courseId)
        {
            MethodResult<StudentModel> commandResult = await _mediator.Send(new UpdateStudentByCourseCommand { CourseId = courseId }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update Profile Student
        /// </summary>
        [HttpPut("update-profile")]
        [ProducesResponseType(typeof(MethodResult<UserModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileStudentCommand command)
        {
            MethodResult<UserModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get student by email
        /// </summary>
        [HttpGet("get-student/{eventCode}")]
        [ProducesResponseType(typeof(MethodResult<IList<StudentModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetStudentByEventCode([FromRoute] string eventCode)
        {
            MethodResult<IList<StudentModel>> commandResult = await _mediator.Send(new GetStudentByEventCodeQuery { EventCode = eventCode }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Student by SchoolId
        /// </summary>
        [HttpGet("get-by-school-id")]
        [ProducesResponseType(typeof(MethodResult<IList<StudentModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetBySchoolId()
        {
            MethodResult<IList<StudentModel>> commandResult = await _mediator.Send(new GetStudentBySchoolIdQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Send Otp SMS
        /// </summary>
        [HttpPost("send-otp-sms")]
        [ProducesResponseType(typeof(MethodResult<SaveOTPForUserEventHaNoiCommandModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SendOtpSMS([FromBody] SendOtpForPhoneVerificationCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Verify Otp SMS
        /// </summary>
        [HttpPost("verify-otp-sms")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> VerifyOtpSMS([FromBody] VerifyOtpUserToSMSCommand command)
        {
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Student Info Event
        /// </summary>
        [HttpGet("student-info-event")]
        [ProducesResponseType(typeof(MethodResult<StudentInfoEventModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetStudentInfoEvent()
        {
            MethodResult<StudentInfoEventModel> commandResult = await _mediator.Send(new GetStudentInfoEventQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Verify Otp SMS
        /// </summary>
        [HttpPut("student-info-event")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateStudentInfoEvent([FromBody] UpdateStudentInfoEventCommand command)
        {
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// deduct coin of student
        /// </summary>
        [HttpPost("deduct-coin-of-student")]
        [ProducesResponseType(typeof(MethodResult<StudentModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(role: nameof(EnumRole.Student))]
        public async Task<IActionResult> DeductCoinOfStudent([FromBody] DeductCoinOfStudentCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
