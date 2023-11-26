// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Application.Commands.StudentCmd;
    using Fsel.Identity.Application.Queries.StudentQuery;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Asp.Versioning;
    using Fsel.Shared.Constants;

    [ApiVersion(ApiSettings.APIVersion1)][ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/student")]
    [ApiController]
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
        [HttpPost("import-student-to-course")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ImportStudent([FromForm] ImportStudentToCourseCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            MethodResult<Stream> commandResult = await _mediator.Send(new ImportStudentToCourseCommand { FormFile = command.FormFile }).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "import-student-to-course.xlsx");
        }

        /// <summary>
        /// Update Referral Code
        /// </summary>
        [HttpPut("update-referral-code")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateReferralCode([FromBody] UpdateReferralCodeStudentCommand command)
        {
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
