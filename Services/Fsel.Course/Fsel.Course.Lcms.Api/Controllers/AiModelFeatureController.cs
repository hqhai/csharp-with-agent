// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lcms.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Application.Commands.AiModelFeatureCmd;
    using Fsel.Course.Application.Queries.AiModelFeatureQuery;
    using Fsel.Course.Application.Queries.AiModelManagerQuery;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.EntityModels.AiManagerModels;
    using Fsel.Course.Domain.Models.QueryModels.AiModelFeature;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/ai-model-feature")]
    //[Common.Attributes.Permission(role: nameof(EnumRole.MasterAdmin))]
    public class AiModelFeatureController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AiModelFeatureController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Ai Model Featue by Id
        /// </summary>
        [HttpGet("{key}")]
        [ProducesResponseType(typeof(MethodResult<AiFeatureModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] EnumFeatureAi key)
        {
            MethodResult<AiFeatureModel> queryResult = await _mediator.Send(new GetAiModelFeatureQuery { Key = key }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create a Ai Model Feature
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<AiFeatureModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateAiModelFeatureCommand command)
        {
            MethodResult<AiFeatureModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Create a Ai Model Feature wwith sub
        /// </summary>
        [HttpPost("sub")]
        [ProducesResponseType(typeof(MethodResult<AiFeatureModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateAiModelHasSubFeatureCommand command)
        {
            MethodResult<AiFeatureModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update a Ai Model Feature
        /// </summary>
        [HttpPut("{key}")]
        [ProducesResponseType(typeof(MethodResult<AiFeatureModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] EnumFeatureAi key, [FromBody] UpdateAiModelFeatureCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Key = key;
            MethodResult<AiFeatureModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update a Setting Ai Model Feature
        /// </summary>
        [HttpPut("setting/{key}")]
        [ProducesResponseType(typeof(MethodResult<AiFeatureModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateSetting([FromRoute] EnumFeatureAi key, [FromBody] UpdateSettingAiModelFeature command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Key = key;
            MethodResult<AiFeatureModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get featue
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<GetFeatureAiModelQuery>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<IReadOnlyList<GetFeatureAiModelQuery>>> Get(CancellationToken ct)
            => Ok(await _mediator.Send(new GetFeatureQuery(), ct));
    }
}
