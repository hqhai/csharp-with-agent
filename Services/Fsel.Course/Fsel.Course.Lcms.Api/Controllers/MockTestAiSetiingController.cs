// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using MediatR;
using Asp.Versioning;
using Fsel.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using Fsel.Course.Domain.Models.CommandModels.Sections;
using Fsel.Course.Application.Queries.MockTestAiSettingQuery;

namespace Fsel.Course.Lcms.Api.Controllers
{
    [ApiVersion(ApiSettings.APIVersion1)][ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/mock-test-ai-setting")]
    [ApiController]
    //[Common.Attributes.Permission(role: nameof(EnumRole.MasterAdmin))]
    public class MockTestAiSetiingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MockTestAiSetiingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Video
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<MockTestAISettingModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            MethodResult<MockTestAISettingModel> queryResult = await _mediator.Send(new GetMockTestAiSettingQuery { Id = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
