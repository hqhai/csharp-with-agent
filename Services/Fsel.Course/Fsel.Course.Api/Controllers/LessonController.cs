using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Common.Helpers;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Application.Commands.LessonCmd;
using Fsel.Course.Application.Queries.LessonQuery;
using Fsel.Course.Common.Models.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Fsel.Course.Api.Controllers
{
    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/lesson")]
    [ApiController]
    [Authorize]
    public class LessonController : ControllerBase
    {
        private readonly IMediator _mediator;
        public LessonController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Lesson
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpGet(Name ="GetSearch")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<LessonModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Search([FromQuery] SearchLessonQuery query)
        {
            try
            {
                MethodResult<PagingItemsModel<LessonModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
                return commandResult.GetActionResult();
            }
            catch (Exception ex)
            {
                VoidMethodResult errorCommandResult = new VoidMethodResult();
                errorCommandResult.AddErrorMessage(MethodHelper.GetExceptionMessage(ex));
                return errorCommandResult.GetActionResult();
            }
        }

        [HttpGet("{Id:Guid}")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<LessonModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetById(Guid Id)
        {
            try
            {
                MethodResult<PagingItemsModel<LessonModel>> commandResult = await _mediator.Send(new GetLessonQuery { Id = Id }).ConfigureAwait(false);
                return commandResult.GetActionResult();
            }
            catch (Exception ex)
            {
                VoidMethodResult errorCommandResult = new VoidMethodResult();
                errorCommandResult.AddErrorMessage(MethodHelper.GetExceptionMessage(ex));
                return errorCommandResult.GetActionResult();
            }
        }

        /// <summary>
        /// Create a Lesson
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<LessonModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateLessonCommand query)
        {
            try
            {
                MethodResult<LessonModel> commandResult = await _mediator.Send(query).ConfigureAwait(false);
                return commandResult.GetActionResult();
            }
            catch (Exception ex)
            {
                VoidMethodResult errorCommandResult = new VoidMethodResult();
                errorCommandResult.AddErrorMessage(MethodHelper.GetExceptionMessage(ex));
                return errorCommandResult.GetActionResult();
            }
        }
        /// <summary>
        /// Create a Lesson
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPut]
        [ProducesResponseType(typeof(MethodResult<LessonModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Update([FromBody] UpdateLessonCommand query)
        {
            try
            {
                MethodResult<LessonModel> commandResult = await _mediator.Send(query).ConfigureAwait(false);
                return commandResult.GetActionResult();
            }
            catch (Exception ex)
            {
                VoidMethodResult errorCommandResult = new VoidMethodResult();
                errorCommandResult.AddErrorMessage(MethodHelper.GetExceptionMessage(ex));
                return errorCommandResult.GetActionResult();
            }
        }
        /// <summary>
        /// Create a Lesson
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpDelete("{Id:Guid}")]
        [ProducesResponseType(typeof(MethodResult<LessonModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Delete(Guid Id)
        {
            try
            {
                MethodResult<Guid> commandResult = await _mediator.Send(new DeleteLessonCommand { Id = Id}).ConfigureAwait(false);
                return commandResult.GetActionResult();
            }
            catch (Exception ex)
            {
                VoidMethodResult errorCommandResult = new VoidMethodResult();
                errorCommandResult.AddErrorMessage(MethodHelper.GetExceptionMessage(ex));
                return errorCommandResult.GetActionResult();
            }
        }
    }
}
