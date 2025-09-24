// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.CommandModels.RubyScope;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.RubyAnnotationCmd;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/ruby-annotation")]
    [ApiController]
    //[Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
    //[Common.Attributes.Permission(role: nameof(EnumRole.Teacher))]
    public class RubyAnnotationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RubyAnnotationController(IMediator mediator)
        {
            _mediator = mediator;
        }
        /// <summary>
        /// Create Annotation for scope
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<RubyAnnotationModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateRubyAnnotationCommand command)
        {
            MethodResult<CreateRubyByScopeCommandModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
