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
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/class-forum-result")]
    [ApiController]
    [Permission]
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
        [HttpGet("execute-query")]
        [ProducesResponseType(typeof(MethodResult<ClassForumResultInfoModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Execute([FromQuery] BaseQueryModel query)
        {
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
    }
}
