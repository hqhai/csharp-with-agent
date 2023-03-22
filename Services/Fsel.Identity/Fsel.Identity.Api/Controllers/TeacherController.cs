using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Common.Enums;
using Fsel.Identity.Application.Commands.AuthCmd;
using Fsel.Identity.Application.Queries.TeacherQuery;
using Fsel.Identity.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Identity.Api.Controllers
{
    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/teacher")]
    [ApiController]
    public class TeacherController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TeacherController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get list teacher by Ids
        /// </summary>
        [HttpPost("get-by-ids")]
        [ProducesResponseType(typeof(MethodResult<IList<TeacherModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetByIds([FromBody] GetTeacherByIdsQuery query)
        {
            MethodResult<IList<TeacherModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
