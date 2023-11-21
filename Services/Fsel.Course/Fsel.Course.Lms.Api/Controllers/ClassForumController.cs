// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.ClassForumQuery;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/class-forum")]
    [ApiController]
    [Common.Attributes.Permission(role: nameof(EnumRole.Student))]
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
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<ClassForumByStudentModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] GetClassForumQuery command)
        {
            MethodResult<ClassForumByStudentModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
