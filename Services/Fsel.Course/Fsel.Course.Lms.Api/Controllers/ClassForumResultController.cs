// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.ClassForumCmd;
    using Fsel.Course.Lms.Application.Commands.ClassForumResultCmd;
    using Fsel.Course.Lms.Application.Queries.ClassForumResultQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using CreateClassForumResultCommand = Fsel.Course.Lms.Application.Commands.ClassForumCmd.V1i2.CreateClassForumResultCommand;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/class-forum-result")]
    [ApiController]
    //[Permission]
    public class ClassForumResultController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IClassForumResultRepository _classForumResultRepository;

        public ClassForumResultController(IMediator mediator, IClassForumResultRepository classForumResultRepository)
        {
            _mediator = mediator;
            _classForumResultRepository = classForumResultRepository;
        }

        /// <summary>
        /// Execute-list-query
        /// </summary>
        [HttpPost("execute-list-query")]
        [ProducesResponseType(typeof(MethodResult<IList<ClassForumResultModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission]
        public async Task<IActionResult> ExecuteList([FromBody] BaseQueryModel cmd)
        {
            var result = await _classForumResultRepository.GetListResultAsync<ClassForumResultModel>(cmd);
            return result.GetActionResult();
        }

        /// <summary>
        /// Execute-list-query
        /// </summary>
        [HttpGet("execute-query")]
        [ProducesResponseType(typeof(MethodResult<ClassForumResultInfoModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Execute([FromQuery] BaseQueryModel query)
        {
            SetQuery(query);
            var result = await _classForumResultRepository.GetResultAsync<ClassForumResultInfoModel>(BaseQuery ?? query);
            return result.GetActionResult();
        }

        /// <summary>
        /// Create a Class Forum Result
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<ClassForumResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateClassForumResultCommand command)
        {
            MethodResult<ClassForumResultModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Rate Class Forum Result
        /// </summary>
        [HttpPost("rate")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Rate([FromBody] RateClassForumResultCommand command)
        {
            MethodResult<StudentFeedbackModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Class Forum Result
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<ClassForumResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            var commandResult = await _mediator.Send(new GetClassForumResultQuery { ClassForumResultId = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update try again
        /// </summary>
        [HttpPut("retry/{id}")]
        [ProducesResponseType(typeof(MethodResult<ClassForumResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Retry([FromRoute] Guid id, [FromBody] RetryClassForumResultCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = id;
            MethodResult<ClassForumResultModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get student class forum result
        /// </summary>
        [HttpGet("get-current-class-forum")]
        [ProducesResponseType(typeof(MethodResult<ClassForumByStudentModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCurrentClassForum([FromQuery] GetCurrentClassForumQuery query)
        {
            MethodResult<ClassForumByStudentModel> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Search class forum result
        /// </summary>
        [HttpGet("get-relevant-class-forums")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<ClassForumResultModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchRelevantClassForums([FromQuery] SearchRelevantClassForumsQuery query)
        {
            MethodResult<PagingItemsModel<ClassForumResultModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get Class forum
        /// </summary>
        [HttpPost("check-ffmpeg")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CheckFFmpeg([FromQuery] CheckFFmpegCommand command)
        {
            MethodResult<bool> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get Class forum
        /// </summary>
        [HttpPost("update-class-forum-detail")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(roles: new string[] { nameof(EnumRole.Admin), nameof(EnumRole.CSO) })]
        public async Task<IActionResult> UpdateClassForumDetailResult([FromBody] UpdateClassForumDetailResultCommand command)
        {
            MethodResult<bool> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get Class forum detail results
        /// </summary>
        [HttpGet("get-class-forum-details")]
        [ProducesResponseType(typeof(MethodResult<IList<ClassForumDetailResultModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetClassForumDetailResults([FromQuery] GetClassForumDetailResultsQuery query)
        {
            MethodResult<IList<ClassForumDetailResultModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
