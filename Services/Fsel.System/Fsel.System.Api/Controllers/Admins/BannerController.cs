// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers.Admins
{
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Constants;
    using Fsel.System.Application.Commands.BannerCmd;
    using Fsel.System.Application.Commands.BannerSettingCmd;
    using Fsel.System.Application.Queries.BannerQuery;
    using Fsel.System.Application.Queries.BannerSettingQuery;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/admin/banner")]
    [ApiController]
    public class BannerController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BannerController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<BannerModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(BannerManagement.View)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            MethodResult<BannerModel> commandResult = await _mediator.Send(new GetBannerQuery { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Search
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<BannerModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(BannerManagement.View)]
        public async Task<IActionResult> Get([FromQuery] SearchBannerQuery query)
        {
            MethodResult<PagingItemsModel<BannerModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Create
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<BannerModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(BannerManagement.Add)]
        public async Task<IActionResult> Create([FromBody] CreateBannerCommand command)
        {
            MethodResult<BannerModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(MethodResult<BannerModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(BannerManagement.Update)]
        public async Task<IActionResult> Update([FromBody] UpdateBannerCommand command)
        {
            MethodResult<BannerModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Delete
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(BannerManagement.Update)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new DeleteBannerCommand { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Create Banner Setting
        /// </summary>
        [HttpPost("banner-setting")]
        [ProducesResponseType(typeof(MethodResult<BannerSettingModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(BannerManagement.Add)]
        public async Task<IActionResult> Create([FromBody] CreateBannerSettingCommand command)
        {
            MethodResult<BannerSettingModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Setting
        /// </summary>
        [HttpGet("banner-setting")]
        [ProducesResponseType(typeof(MethodResult<BannerSettingModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(BannerManagement.View)]
        public async Task<IActionResult> GetBannerSetting()
        {
            MethodResult<BannerSettingModel> commandResult = await _mediator.Send(new GetBannerSettingQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Check Banner Priority Existence
        /// </summary>
        [HttpPost("check-priority")]
        [ProducesResponseType(typeof(MethodResult<BannerPriorityExistenceModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(BannerManagement.View)]
        public async Task<IActionResult> CheckBannerPriorityExistence([FromBody] CheckBannerPriorityExistenceCommand command)
        {
            MethodResult<BannerPriorityExistenceModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Search
        /// </summary>
        [HttpGet("preview")]
        [ProducesResponseType(typeof(MethodResult<IList<BannerModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(BannerManagement.Preview)]
        public async Task<IActionResult> PreviewBannerByDate([FromQuery] PreviewBannerByDateQuery query)
        {
            MethodResult<IList<BannerModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// banner trong ngày
        /// </summary>
        [HttpGet("banner-in-day")]
        [ProducesResponseType(typeof(MethodResult<IList<BannerModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(BannerManagement.View)]
        public async Task<IActionResult> GetBannerInDateQuery([FromQuery] GetBannerInDateQuery query)
        {
            MethodResult<IList<BannerInDayModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
