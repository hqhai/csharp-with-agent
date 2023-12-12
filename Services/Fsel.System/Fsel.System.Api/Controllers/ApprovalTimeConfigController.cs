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
    using Fsel.System.Application.Queries.ApprovalTimeConfigQuery;
    using Fsel.System.Application.Commands.ApprovalTimeConfigCmd;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/approval-time-config")]
    [ApiController]
    public class ApprovalTimeConfigController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IApprovalTimeConfigRepository _approvalTimeConfigRepository;

        public ApprovalTimeConfigController(IMediator mediator, IApprovalTimeConfigRepository approvalTimeConfigRepository)
        {
            _mediator = mediator;
            _approvalTimeConfigRepository = approvalTimeConfigRepository;
        }


        /// <summary>
        /// Execute-list-query
        /// </summary>
        [HttpGet("execute-list-query")]
        [ProducesResponseType(typeof(MethodResult<IList<ApprovalTimeConfigModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission]
        public async Task<IActionResult> ExecuteList([FromQuery] BaseQueryModel query)
        {
            var result = await _approvalTimeConfigRepository.GetListResultAsync<ApprovalTimeConfigModel>(query);
            return result.GetActionResult();
        }

        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<ApprovalTimeConfigModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] GetListApprovalTimeConfigQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }


        /// <summary>
        /// Save course time config
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SaveList([FromBody] CreateApprovalTimeConfigCommand command)
        {
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
