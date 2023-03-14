using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Common.Enums;
using Fsel.Common.Helpers;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Application.Commands.CourseCmd;
using Fsel.Course.Application.Queries.CourseQuery;
using Fsel.Course.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Course.Lcms.Api.Controllers
{
    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/course")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.MasterAdmin))]
    public class CourseController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CourseController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Course
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<CourseModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Search([FromQuery] SearchCourseQuery query)
        {
            try
            {
                MethodResult<PagingItemsModel<CourseModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
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
        /// Create a Course
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<CourseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateCourseCommand command)
        {
            try
            {
                MethodResult<CourseModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
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
        /// Update a Course
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MethodResult<CourseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCourseCommand command)
        {
            try
            {
                command.Id = id;
                MethodResult<CourseModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
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
        /// Delete a Course
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(MethodResult<CourseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> delete([FromRoute] Guid id)
        {
            try
            {
                MethodResult<bool> commandResult = await _mediator.Send(new DeleteCourseCommand { Id = id }).ConfigureAwait(false);
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
        /// Get Course
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<CourseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            try
            {
                MethodResult<CourseModel> commandResult = await _mediator.Send(new GetCourseQuery { Id = id }).ConfigureAwait(false);
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
