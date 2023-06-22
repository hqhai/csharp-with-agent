// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Api.Controllers.Student
{
    using Fsel.Common.ActionResults;
    using System.Net;
    using Fsel.Common.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Interaction.Application.Commands.ActionCmd;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Interaction.Application.Queries.PostQuery;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/student/post")]
    [ApiController]
    public class StudentPostController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StudentPostController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create Post
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<PostModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateStudentPost([FromBody] CreateStudentPostsCommand command)
        {
            MethodResult<PostModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Update Post
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MethodResult<PostModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateStudentPost([FromRoute] Guid id, [FromBody] UpdateStudentPostsCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = id;
            MethodResult<PostModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Delete a Post from Student
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(MethodResult<PostModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DeleteStudentPost([FromRoute] Guid id)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new DeleteStudentPostsCommand { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get posts by student ID
        /// </summary>
        [HttpGet("posts-by-student/{id}/{status}")]
        [ProducesResponseType(typeof(MethodResult<List<PostModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPostsByStudent([FromRoute] Guid id, EnumPostStatus status)
        {
            var queryResult = await _mediator.Send(new GetPostsByStudentQuery { StudentId = id, Status = status }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Detail Post
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<PostModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPostDetail([FromRoute] Guid id)
        {
            var queryResult = await _mediator.Send(new GetPostDetailQuery { PostId = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Active Post List
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<PostSearchModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetActivePostList([FromQuery] GetActivePostListQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Top Post 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("top-posts/by-student-level")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<PostSearchModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetTopPostByStudentLevel([FromQuery] GetTopPostsListQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Top Trending
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("top-trending")]
        [ProducesResponseType(typeof(MethodResult<List<TopicTagModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetTopTrending([FromQuery] GetTopTrendingsQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }


    }
}
