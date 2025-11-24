// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lcms.Api.Controllers
{
    using System.Net;
    using Application.Commands.AiCriteriaConfigCmd;
    using Application.Queries.AiCriteriaConfigQuery;
    using Asp.Versioning;
    using Common.ActionResults;
    using Common.Constants;
    using Domain.Enums;
    using Domain.Models.EntityModels.AiPromptManagerModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Constants;

    [ApiController]
    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/ai-criteria-config")]
    //[Common.Attributes.Permission(role: nameof(EnumRole.MasterAdmin))]
    public class AiCriteriaConfigController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AiCriteriaConfigController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get ai criteria
        /// </summary>
        [HttpGet("{projectId}")]
        [ProducesResponseType(typeof(MethodResult<AICriteriaConfigsModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid projectId,[FromHeader] GetAiCriteriaConfigQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);
            query.ProjectId = projectId;
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create ai criteria
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<AICriteriaConfigsModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateAiCriteriaConfigCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update a ai criteria
        /// </summary>
        [HttpPut("{projectId}")]
        [ProducesResponseType(typeof(MethodResult<AICriteriaConfigsModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] Guid projectId, [FromBody] UpdateAiModelFeatureCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Project = projectId;
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update a Setting ai criteria
        /// </summary>
        [HttpPut("setting/{projectId}")]
        [ProducesResponseType(typeof(MethodResult<AICriteriaConfigsModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateSetting([FromRoute] Guid projectId, [FromBody] UpdateSettingAiModelFeature command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.ProjectId = projectId;
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
