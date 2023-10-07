// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Core.Base.BaseModels;
using Fsel.Identity.Application.Commands.StudentRankingCmd;
using Fsel.Identity.Application.Queries.StudentRanking;
using Fsel.Identity.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Identity.Api.Controllers
{
    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/student-focus-time")]
    [ApiController]
    public class StudentFocusTimeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StudentFocusTimeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Lưu thông tin truy cập.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<List<StudentRankingModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateStudentFocusTime([FromBody] CreateStudentRankingsCommand query)
        {
            MethodResult<List<StudentRankingModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

    }
}
