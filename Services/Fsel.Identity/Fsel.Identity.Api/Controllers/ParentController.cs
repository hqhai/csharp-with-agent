using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Common.Enums;
using Fsel.Identity.Application.Commands.ParentCmd;
using Fsel.Identity.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Identity.Api.Controllers
{
    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/parent")]
    [ApiController]
    public class ParentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ParentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// create student
        /// </summary>
        [HttpPost("create-student")]
        [ProducesResponseType(typeof(MethodResult<UserModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateStudent([FromBody] CreateStudentByParentCommand command)
        {
            MethodResult<UserModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
