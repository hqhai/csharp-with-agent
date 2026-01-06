// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Application.Queries.CategoryQuery;
    using Common.ActionResults;
    using Common.Constants;
    using Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Attributes;
    using Shared.Constants;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// get subject detail
        /// </summary>
        [HttpGet("get-subject-detail/{id}")]
        [ProducesResponseType(typeof(MethodResult<IList<CategoryTreeModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCategoryTree(Guid id)
        {
            var query = new GetCategoryTreeQuery { Id = id };
            var methodResult = await _mediator.Send(query).ConfigureAwait(false);
            return methodResult.GetActionResult();
        }

        [HttpGet("get-subjects")]
        public async Task<IActionResult> GetSubjects()
        {
            var getProgramQuery = new GetAllSubjectsQuery();
            var queryResult = await _mediator.Send(getProgramQuery).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [HttpGet("get-program-match-student/{studentId:guid}")]
        public async Task<IActionResult> GetProgramsMatchingStudent(Guid studentId)
        {
            var getProgramQuery = new GetProgramsMatchUserQuery
            {
                StudentId = studentId
            };
            var queryResult = await _mediator.Send(getProgramQuery).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get category tress
        /// </summary>
        [HttpGet("get-category-trees")]
        [ProducesResponseType(typeof(MethodResult<IList<CategoryTreeDtoModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCategoryTrees()
        {
            var methodResult = await _mediator.Send(new GetCategoryTreesQuery()).ConfigureAwait(false);
            return methodResult.GetActionResult();
        }
    }
}
