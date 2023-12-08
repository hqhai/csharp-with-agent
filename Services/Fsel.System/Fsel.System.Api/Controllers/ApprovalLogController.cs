// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Asp.Versioning;
    using Fsel.Shared.Constants;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Application.Commands.ApprovalLogCmd;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/approval-log")]
    [ApiController]
    public class ApprovalLogController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICourseTimeConfigRepository _courseTimeConfigRepository;

        public ApprovalLogController(IMediator mediator, ICourseTimeConfigRepository courseTimeConfigRepository)
        {
            _mediator = mediator;
            _courseTimeConfigRepository = courseTimeConfigRepository;
        }


        /// <summary>
        /// Execute-list-query
        /// </summary>
        [HttpPost("execute-list-query")]
        [ProducesResponseType(typeof(MethodResult<IList<ApprovalTimeConfig>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission]
        public async Task<IActionResult> ExecuteList([FromBody] BaseQueryModel query)
        {
            var result = await _courseTimeConfigRepository.GetListResultAsync<ApprovalTimeConfigModel>(query);
            return result.GetActionResult();
        }

        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SaveList([FromBody] CreateApprovalLogCommand command)
        {
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }



    }
}
