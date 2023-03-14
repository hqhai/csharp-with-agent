using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Common.Enums;
using Fsel.Common.Helpers;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Application.Commands.VideoCmd;
using Fsel.Course.Application.Queries.VideoQuery;
using Fsel.Course.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Course.Lcms.Api.Controllers
{
    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/videos")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.MasterAdmin))]
    public class VideoController : ControllerBase
    {
        private readonly IMediator _mediator;

        public VideoController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Video
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<VideoModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Search([FromQuery] SearchVideoQuery query)
        {
            try
            {
                MethodResult<PagingItemsModel<VideoModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
                return queryResult.GetActionResult();
            }
            catch (Exception ex)
            {
                VoidMethodResult errorResult = new VoidMethodResult();
                errorResult.AddErrorMessage(MethodHelper.GetExceptionMessage(ex));
                return errorResult.GetActionResult();
            }
        }

        /// <summary>
        /// Get Video
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<VideoModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            try
            {
                MethodResult<VideoModel> queryResult = await _mediator.Send(new GetVideoQuery { Id = id }).ConfigureAwait(false);
                return queryResult.GetActionResult();
            }
            catch (Exception ex)
            {
                VoidMethodResult errorResult = new VoidMethodResult();
                errorResult.AddErrorMessage(MethodHelper.GetExceptionMessage(ex));
                return errorResult.GetActionResult();
            }
        }

        /// <summary>
        /// Create a Video
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<VideoModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateVideoCommand command)
        {
            try
            {
                MethodResult<VideoModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
                return commandResult.GetActionResult();
            }
            catch (Exception ex)
            {
                VoidMethodResult errorResult = new VoidMethodResult();
                errorResult.AddErrorMessage(MethodHelper.GetExceptionMessage(ex));
                return errorResult.GetActionResult();
            }
        }

        /// <summary>
        /// Update a Video
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MethodResult<VideoModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateVideoCommand command)
        {
            try
            {
                command.Id = id;
                MethodResult<VideoModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
                return commandResult.GetActionResult();
            }
            catch (Exception ex)
            {
                VoidMethodResult errorResult = new VoidMethodResult();
                errorResult.AddErrorMessage(MethodHelper.GetExceptionMessage(ex));
                return errorResult.GetActionResult();
            }
        }

        /// <summary>
        /// Delete a Video
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(MethodResult<VideoModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            try
            {
                MethodResult<bool> commandResult = await _mediator.Send(new DeleteVideoCommand { Id = id }).ConfigureAwait(false);
                return commandResult.GetActionResult();
            }
            catch (Exception ex)
            {
                VoidMethodResult errorResult = new VoidMethodResult();
                errorResult.AddErrorMessage(MethodHelper.GetExceptionMessage(ex));
                return errorResult.GetActionResult();
            }
        }
    }
}
