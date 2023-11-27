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
using Asp.Versioning;
using Fsel.Shared.Constants;

namespace Fsel.Identity.Api.Controllers
{
    [ApiVersion(ApiSettings.APIVersion1)][ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/student-ranking")]
    [ApiController]
    public class StudentRankingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StudentRankingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Lưu xếp hạng
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<List<StudentRankingModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateStudentRanking()
        {
            MethodResult<List<StudentRankingModel>> commandResult = await _mediator.Send(new CreateStudentRankingsCommand()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }


        /// <summary>
        /// Lấy ra list danh sách xếp hạng của học sinh
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<List<StudentRankingModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetStudentRanking([FromQuery] GetStudentRankingQuery query)
        {
            MethodResult<List<StudentRankingModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }




    }
}
