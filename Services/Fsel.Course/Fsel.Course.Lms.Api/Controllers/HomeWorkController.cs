// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.HomeWorkCmd;
    using Fsel.Course.Lms.Application.Queries.HomeWorkQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/home-work")]
    [ApiController]
    [Common.Attributes.Permission(role: nameof(EnumRole.Student))]
    public class HomeWorkController : ControllerBase
    {
        private readonly IMediator _mediator;

        public HomeWorkController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Home Work
        /// </summary>
        [MapToApiVersion(ApiSettings.APIVersion1)]
        [MapToApiVersion(ApiSettings.APIVersion1i1)]
        [EncryptResponse]
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<HomeWorkModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] GetHomeWorkQuery query)
        {
            MethodResult<HomeWorkModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get lesson homework score
        /// </summary>
        [HttpGet("get-list-homework")]
        [ProducesResponseType(typeof(MethodResult<IList<LessonHomeWorkResultModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListHomeWork([FromQuery] GetListHomeworkQuery query)
        {
            MethodResult<IList<LessonHomeWorkResultModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create HomeWork Answer
        /// </summary>
        [MapToApiVersion(ApiSettings.APIVersion1)]
        [HttpPost("create-home-work-answer")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateHomeWorkAnswer([FromBody] CreateHomeWorkAnswerCommand query)
        {
            MethodResult<bool> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
