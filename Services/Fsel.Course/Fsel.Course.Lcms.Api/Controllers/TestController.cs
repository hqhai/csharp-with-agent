// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lcms.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Application.Commands.TestCmd;
    using Fsel.Course.Application.Queries.TestQuery;
    using Fsel.Course.Domain.Models.EntityModels.TestModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/test")]
    [ApiController]
    [Common.Attributes.Permission(role: nameof(EnumRole.MasterAdmin))]
    public class TestController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TestController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Test
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<TestModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchTestQuery query)
        {
            MethodResult<PagingItemsModel<TestSearchModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Test
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<TestModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            MethodResult<TestModel> queryResult = await _mediator.Send(new GetTestQuery { Id = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create a Test
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<TestModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateTestCommand command)
        {
            MethodResult<TestModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update a Test
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MethodResult<TestModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateTestCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = id;
            MethodResult<TestModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Delete a Test
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new DeleteTestCommand { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Test Histories
        /// </summary>
        [HttpGet("histories")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<TestModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetTestHistory([FromQuery] GetTestHistoryQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
