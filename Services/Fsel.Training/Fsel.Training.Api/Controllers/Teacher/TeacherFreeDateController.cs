// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Api.Controllers.Teacher
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Training.Application.Commands.TeacherFreeDateCmd;
    using Fsel.Training.Application.Queries.TeacherFreeDateQuery;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/teacher-free-date")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.Teacher))]
    public class TeacherFreeDateController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TeacherFreeDateController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create Teacher Free Date
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<TeacherFreeDateModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateTeacherFreeDateCommand command)
        {
            MethodResult<TeacherFreeDateModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Search Teacher Free Date
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<TeacherFreeDateModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Search([FromQuery] SearchTeacherFreeDateQuery query)
        {
            MethodResult<PagingItemsModel<TeacherFreeDateModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Detail Teacher Free Date
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<TeacherFreeDateModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            MethodResult<TeacherFreeDateModel> commandResult = await _mediator.Send(new GetTeacherFreeDateQuery { TeacherFreeDateId = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
