// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Core.Base.BaseModels;
using Fsel.Identity.Application.Queries.TeacherQuery;
using Fsel.Identity.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Fsel.Shared.Constants;

namespace Fsel.Identity.Api.Controllers
{
    [ApiVersion(ApiSettings.APIVersion1)][ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/teacher")]
    [ApiController]
    public class TeacherController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TeacherController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get list teacher by Ids
        /// </summary>
        [HttpPost("get-by-ids")]
        [ProducesResponseType(typeof(MethodResult<IList<TeacherModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetByIds([FromBody] GetTeacherByIdsQuery query)
        {
            MethodResult<IList<TeacherModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get teacher by Id
        /// </summary>
        [HttpGet("get-by-id/{id}")]
        [ProducesResponseType(typeof(MethodResult<TeacherModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            MethodResult<TeacherModel> commandResult = await _mediator.Send(new GetTeacherByIdQuery { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get teacher by UserId
        /// </summary>
        [HttpGet("get-by-user-id/{id}")]
        [ProducesResponseType(typeof(MethodResult<TeacherModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetByUserId([FromRoute] Guid id)
        {
            MethodResult<TeacherModel> commandResult = await _mediator.Send(new GetTeacherByUserIdQuery { UserId = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Search Teacher
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<TeacherModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission]
        public async Task<IActionResult> SearchTeacher([FromQuery] SearchTeacherQuery query)
        {
            MethodResult<PagingItemsModel<TeacherModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get all teacher
        /// </summary>
        [HttpGet("get-all")]
        [ProducesResponseType(typeof(MethodResult<IList<TeacherModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAllTeacher()
        {
            MethodResult<IList<TeacherModel>> commandResult = await _mediator.Send(new GetAllTeacherQuery { }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get teachers by keyword
        /// </summary>
        [HttpGet("get-teachers-by-keyword/{keyword}")]
        [ProducesResponseType(typeof(MethodResult<IList<TeacherModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetTeachersByKeyword([FromRoute] string? keyword)
        {
            MethodResult<IList<TeacherModel>> commandResult = await _mediator.Send(new GetTeachersByKeywordQuery { Keyword = keyword }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
