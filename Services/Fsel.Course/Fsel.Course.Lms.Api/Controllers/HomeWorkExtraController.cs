// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.HomeWorkExtraCmd;
    using Fsel.Course.Lms.Application.Queries.HomeWorkExtraQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/home-work-extra")]
    [ApiController]
    [Permission(role: nameof(EnumRole.Student))]
    public class HomeWorkExtraController : ControllerBase
    {
        private readonly IMediator _mediator;

        public HomeWorkExtraController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Home Work Extra
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<HomeWorkExtraDtoModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] GetHomeWorkExtraQuery query)
        {
            MethodResult<HomeWorkExtraDtoModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Home Work Extra
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<HomeWorkExtraModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchHomeWorkExtraQuery query)
        {
            MethodResult<PagingItemsModel<HomeWorkExtraModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Home Work Extra Assigned
        /// </summary>
        [HttpGet("assigned")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<HomeWorkExtraModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchHomeWorkExtraAssignedQuery query)
        {
            MethodResult<PagingItemsModel<HomeWorkExtraModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create Answer
        /// </summary>
        [HttpPost("create-answer")]
        [ProducesResponseType(typeof(MethodResult<HomeWorkExtraDtoModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateAnswer([FromBody] CreateHomeWorkExtraPracticeAnswerCommand command)
        {
            MethodResult<HomeWorkExtraDtoModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Reset Answer
        /// </summary>
        [HttpPost("reset")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Reset([FromBody] ResetHomeWorkExtraCommand command)
        {
            MethodResult<bool> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Start HomeWork Extra
        /// </summary>
        [HttpPost("start")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Start([FromBody] StartHomeWorkExtraPracticeCommand command)
        {
            MethodResult<bool> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
