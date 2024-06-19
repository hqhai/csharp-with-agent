// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Teacher
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.ClassForumScoreCmd;
    using Fsel.Course.Lms.Application.Queries.ClassForumResultQuery;
    using Fsel.Shared.Enums;
    using MediatR;
    using Asp.Versioning;
    using Fsel.Shared.Constants;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)][ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/teacher/class-forum-result")]
    [ApiController]
    [Common.Attributes.Permission(role: nameof(EnumRole.Teacher))]
    public class ClassForumResultController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClassForumResultController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Class Forum Result
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<ClassForumResultSearchModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchClassForumResultByTeacherQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Class Forum Result
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<ClassForumResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            var commandResult = await _mediator.Send(new GetClassForumResultQuery { ClassForumResultId = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Grade Grade Class Forum
        /// </summary>
        [HttpPost("grade-class-forum")]
        [ProducesResponseType(typeof(MethodResult<List<ClassForumScoreModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GradeClassForum([FromBody] GradeClassForumCommand command)
        {
            MethodResult<List<ClassForumScoreModel>> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
