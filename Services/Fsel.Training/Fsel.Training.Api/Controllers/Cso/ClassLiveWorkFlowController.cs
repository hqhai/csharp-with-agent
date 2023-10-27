// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Api.Controllers.Cso
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Training.Application.Commands.ClassLiveWorkFlowCmd;
    using Fsel.Training.Application.Queries.ClassLiveWorkFlowQuery;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/cso/work-flow")]
    [ApiController]
    [Common.Attributes.Permission(role: nameof(EnumRole.CSO))]
    public class ClassLiveWorkFlowController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClassLiveWorkFlowController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// search alternative calendar
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<ChangeTeacherLiveModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchChangeTeacherLivesByCsoQuery query)
        {
            MethodResult<PagingItemsModel<ChangeTeacherLiveModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Replace Teacher by Cso.
        /// </summary>
        [HttpPut("asign-new-teacher")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ReplaceTeacher([FromBody] ReplaceTeacherByCsoCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get alternative calendar
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<ClassLiveWorkFlowInfoModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            MethodResult<ClassLiveWorkFlowInfoModel> queryResult = await _mediator.Send(new GetClassLiveWorkFlowInfoQuery { Id = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
