// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lcms.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Application.Commands.MockTestCmd;
    using Fsel.Course.Application.Queries.MockTestQuery;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MassTransit.Mediator;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/test-config")]
    [Common.Attributes.Permission(role: nameof(EnumRole.MasterAdmin))]
    [ApiController]
    public class TestConfigController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TestConfigController(IMediator mediator)
        {
            _mediator = mediator;
        }

        ///// <summary>
        ///// Search Test Config
        ///// </summary>
        //[HttpGet]
        //[ProducesResponseType(typeof(MethodResult<PagingItemsModel<MockTestModel>>), (int)HttpStatusCode.OK)]
        //[ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        //public async Task<IActionResult> Search([FromQuery] SearchMockTestQuery query)
        //{
        //    MethodResult<PagingItemsModel<TestConfigSearchModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
        //    return queryResult.GetActionResult();
        //}

        ///// <summary>
        ///// Get Test Config
        ///// </summary>
        //[HttpGet("{id}")]
        //[ProducesResponseType(typeof(MethodResult<MockTestModel>), (int)HttpStatusCode.OK)]
        //[ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        //public async Task<IActionResult> Get([FromRoute] Guid id)
        //{
        //    MethodResult<MockTestModel> queryResult = await _mediator.Send(new GetMockTestQuery { Id = id }).ConfigureAwait(false);
        //    return queryResult.GetActionResult();
        //}

        ///// <summary>
        ///// Create a Test Config
        ///// </summary>
        //[HttpPost]
        //[ProducesResponseType(typeof(MethodResult<MockTestModel>), (int)HttpStatusCode.OK)]
        //[ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        //public async Task<IActionResult> Create([FromBody] CreateMockTestCommand command)
        //{
        //    MethodResult<MockTestModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
        //    return commandResult.GetActionResult();
        //}

        ///// <summary>
        ///// Update a Test Config
        ///// </summary>
        //[HttpPut("{id}")]
        //[ProducesResponseType(typeof(MethodResult<MockTestModel>), (int)HttpStatusCode.OK)]
        //[ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        //public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMockTestCommand command)
        //{
        //    ArgumentNullException.ThrowIfNull(command);
        //    command.Id = id;
        //    MethodResult<MockTestModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
        //    return commandResult.GetActionResult();
        //}

        ///// <summary>
        ///// Delete a Test Config
        ///// </summary>
        //[HttpDelete("{id}")]
        //[ProducesResponseType(typeof(MethodResult<MockTestModel>), (int)HttpStatusCode.OK)]
        //[ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        //public async Task<IActionResult> Delete([FromRoute] Guid id)
        //{
        //    MethodResult<bool> commandResult = await _mediator.Send(new DeleteMockTestCommand { Id = id }).ConfigureAwait(false);
        //    return commandResult.GetActionResult();
        //}
    }
}
