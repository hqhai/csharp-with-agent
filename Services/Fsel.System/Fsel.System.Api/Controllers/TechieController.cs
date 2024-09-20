// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Constants;
    using Fsel.System.Application.Commands.Chatbots;
    using Fsel.System.Application.Commands.TechieCmd;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/techie")]
    [ApiController]
    public class TechieController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ISchoolRepository _schoolRepository;

        public TechieController(IMediator mediator, ISchoolRepository schoolRepository)
        {
            _mediator = mediator;
            _schoolRepository = schoolRepository;
        }

        /// <summary>
        /// Execute-list-query
        /// </summary>
        [HttpPost("execute-list-query")]
        [ProducesResponseType(typeof(MethodResult<IList<SchoolModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission]
        public async Task<IActionResult> ExecuteList([FromBody] BaseQueryModel cmd)
        {
            SetQuery(cmd);
            var result = await _schoolRepository.GetListResultAsync<SchoolModel>(cmd);
            return result.GetActionResult();
        }


        [HttpPost("student-techies")]
        [ProducesResponseType(typeof(MethodResult<StudentTechieModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SaveStudentTechie([FromBody] CreateStudentTechieCommand cmd)
        {
            MethodResult<StudentTechieModel> commandResult = await _mediator.Send(cmd).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Lưu action của techie
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost("techie-action")]
        [ProducesResponseType(typeof(MethodResult<TechieActionModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SaveTechieAction([FromBody] CreateTechieActionCommand cmd)
        {
            MethodResult<TechieActionModel> commandResult = await _mediator.Send(cmd).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
