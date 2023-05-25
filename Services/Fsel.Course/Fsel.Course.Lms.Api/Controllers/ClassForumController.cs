// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using Fsel.Common.ActionResults;
    using System.Net;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Course.Lms.Application.Queries.ClassForumQuery;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/class-forum")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.MasterAdmin))]
    public class ClassForumController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClassForumController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// get Class forum
        /// </summary>
        [HttpGet("final-test")]
        [ProducesResponseType(typeof(MethodResult<ClassForumModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] GetClassForumQuery command)
        {
            MethodResult<ClassForumModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
