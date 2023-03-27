// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Identity.Application.Commands.StudentCmd;
    using Fsel.Identity.Application.Queries.StudentQuery;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/student")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StudentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Student by UserId
        /// </summary>
        [HttpGet("get-by-user-id/{id}")]
        [ProducesResponseType(typeof(MethodResult<StudentModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetByUserId([FromRoute] string? id)
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
        public async Task<IActionResult> GetStudentByClassId([FromRoute] string? id)
        {
            MethodResult<IList<StudentModel>> commandResult = await _mediator.Send(new GetStudentByClassIdQuery { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get check class more than 12 students
        /// </summary>
        [HttpGet("get-class-has-too-many-students/{id}")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetStudentByClassIdCheck([FromRoute] string? id)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new GetStudentByClassIdCheckQuery { Id = id }).ConfigureAwait(false);
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
    }
}
