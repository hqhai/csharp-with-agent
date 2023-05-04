// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.HomeWorkResultCmd;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/home-work-result")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.Student))]
    public class HomeWorkResultController : ControllerBase
    {
        private readonly IMediator _mediator;

        public HomeWorkResultController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create review lession HomeWork
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<HomeWorkResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ReviewLessionHomeWork([FromBody] ReviewLessonHomeWorkCommand command)
        {
            MethodResult<HomeWorkResultModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
