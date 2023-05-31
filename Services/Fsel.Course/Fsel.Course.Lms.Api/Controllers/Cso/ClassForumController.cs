// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Cso
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.ClassForumQuery;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/class-forum")]
    [Authorize(Roles = nameof(EnumRole.MasterAdmin))]
    [ApiController]
    public class ClassForumController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClassForumController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Class Forum by lesson id
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<ClassForumModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            var commandResult = await _mediator.Send(new GetClassForumByLessonQuery { LessonId = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
