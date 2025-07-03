// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lcms.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Application.Commands.TestConfigCmd;
    using Fsel.Course.Application.Commands.TestConfigSectionCmd;
    using Fsel.Course.Application.Queries.TestConfigQuery;
    using Fsel.Course.Domain.Models.EntityModels.TestConfig;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/test-config")]
    [ApiController]
    //[Common.Attributes.Permission(role: nameof(EnumRole.MasterAdmin))]
    public class TestConfigController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TestConfigController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Test Config
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<TestConfigModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchTestConfigQuery query)
        {
            MethodResult<PagingItemsModel<TestConfigModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Test Config
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<TestConfigModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            MethodResult<TestConfigModel> queryResult = await _mediator.Send(new GetTestConfigQuery { Id = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create a Test Config
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<TestConfigModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateTestConfigCommand command)
        {
            MethodResult<TestConfigModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update a Test Config
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MethodResult<TestConfigModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateTestConfigCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = id;
            MethodResult<TestConfigModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Delete a Test Config Section
        /// </summary>
        [HttpDelete("/section/{id}")]
        [ProducesResponseType(typeof(MethodResult<TestConfigModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DeleteSection([FromRoute] Guid id)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new DeleteTestConfigSectionCommand { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
