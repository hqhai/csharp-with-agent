// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Api.Controllers.Admin
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Training.Application.Commands.ClassCmd;
    using Fsel.Training.Application.Queries.ClassQuery;
    using Fsel.Training.Application.Queries.ClassQuery.Admin;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Training.Domain.Models.EntityModels;
    using Fsel.Training.Application.Commands.ClassCmd;
    using Fsel.Training.Application.Queries.ClassQuery.Admin;
    using Fsel.Training.Application.Commands.ClassStudentCmd;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/admin/class")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.Admin))]
    public class ClassController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClassController(IMediator mediator)
        {
            _mediator = mediator;
        }
        /// <summary>
        /// get list studentId by classId
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<List<Guid>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            MethodResult<List<Guid>?> commandResult = await _mediator.Send(new GetStudentIdsInClassQuery { ClassId = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
        /// <summary>
        /// create class
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<ClassModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateClass([FromBody] CreateClassCommand command)
        {
            MethodResult<ClassModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
        /// <summary>
        /// active class
        /// </summary>
        [HttpPut("active-class/{id}")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ActiveClass([FromRoute] Guid id)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new ActiveClassCommand { Id = id}).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
        /// <summary>
        /// get class by id
        /// </summary>
        [HttpGet("get-by-id/{id}")]
        [ProducesResponseType(typeof(MethodResult<ClassModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            MethodResult<ClassModel> commandResult = await _mediator.Send(new GetClassByIdQuery { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
        /// <summary>
        /// update class
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MethodResult<ClassModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateClass([FromRoute] Guid id, [FromBody] UpdateClassCommand command)
        {
            command.Id = id;
            MethodResult<ClassModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        [HttpGet("get-classes-equal-level-package/{id}")]
        [ProducesResponseType(typeof(MethodResult<IList<ClassModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetClassesEqualLevelAndPackage([FromRoute] Guid id)
        {
            MethodResult<IList<ClassModel>> commandResult = await _mediator.Send(new GetClassesEqualLevelAndPackageQuery { ClassId = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        [HttpPut]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> TransferStudent([FromQuery] TransferStudentCommand command)
        {
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }


        [HttpGet("get-students-by-class-id/{id}")]
        [ProducesResponseType(typeof(MethodResult<IList<ClassStudentModel>?>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetStudentsByClassId([FromRoute] Guid id)
        {
            MethodResult<IList<ClassStudentModel>?> commandResult = await _mediator.Send(new GetStudentsByClassIdQuery { ClassId = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
