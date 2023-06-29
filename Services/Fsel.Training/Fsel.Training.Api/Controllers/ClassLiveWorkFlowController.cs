// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Training.Application.Commands.ClassLiveWorkFlowCmd;
    using Fsel.Training.Application.Queries.ClassLiveWorkFlowQuery;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/work-flow")]
    [ApiController]
    public class ClassLiveWorkFlowController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClassLiveWorkFlowController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create ClassLiveWorkFlow
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<ClassLiveWorkFlowModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateClassLiveWorkFlowCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create ClassLiveWorkFlow
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MethodResult<ClassLiveWorkFlowModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateClassLiveWorkFlowCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = id;
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search ClassLiveWorkFlow
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<ClassLiveWorkFlowModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchClassLiveWorkFlowByTeacherIdQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
