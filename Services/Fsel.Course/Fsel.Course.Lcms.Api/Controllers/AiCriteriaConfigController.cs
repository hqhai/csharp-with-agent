// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lcms.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Application.Commands.AiCriteriaConfigCmd;
    using Fsel.Course.Application.Commands.AiFeatureConfigCmd;
    using Fsel.Course.Application.Queries.AiPromptConfigQuery;
    using Fsel.Course.Domain.Models.EntityModels.AiPromptManagerModels;
    using Fsel.Course.Domain.Models.QueryModels.AiModelFeature;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

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
        /// Get ai criteria by id
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<AICriteriaConfigsModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            MethodResult<AICriteriaConfigsModel> queryResult = await _mediator.Send(new GetAiCriteriaConfigQuery { Id = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create ai criteria
        /// </summary>
        [HttpPost()]
        [ProducesResponseType(typeof(MethodResult<AICriteriaConfigsModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateAiCriteriaConfigCommand command)
        {
            MethodResult<AICriteriaConfigsModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update a ai criteria
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MethodResult<AICriteriaConfigsModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateAiModelFeatureCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = id;
            MethodResult<AICriteriaConfigsModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update a Setting ai criteria
        /// </summary>
        [HttpPut("setting/{id}")]
        [ProducesResponseType(typeof(MethodResult<AICriteriaConfigsModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateSetting([FromRoute] Guid id, [FromBody] UpdateSettingAiModelFeature command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = id;
            MethodResult<AICriteriaConfigsModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
