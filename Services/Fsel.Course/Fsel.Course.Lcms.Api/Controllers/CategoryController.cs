// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lcms.Api.Controllers
{
    using System.Net;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Common.Models;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Application.Commands.CategoryCmd;
    using Fsel.Course.Application.Commands.FlowCmd;
    using Fsel.Course.Application.Commands.ProgramCmd;
    using Fsel.Course.Application.Queries.CategoryQuery;
    using Fsel.Course.Application.Queries.LevelQuery;
    using Fsel.Course.Application.Queries.ProgramQuery;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/category")]
    [ApiController]
    [AllowAnonymous]
    public class CategoryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Course Levels
        /// </summary>
        [HttpGet("course-level")]
        [ProducesResponseType(typeof(MethodResult<IList<EnumCourseLevel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetEnumCourseLevelsAsync([FromQuery] EnumCourseType? courseType)
        {
            var queryResult = await _mediator.Send(new GetEnumCourseLevelQuery { CourseType = courseType }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get All PlacementTest type and Course level
        /// </summary>
        [HttpGet("all-course-skill")]
        [ProducesResponseType(typeof(MethodResult<object>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetEnumCourseSkillsAsync()
        {
            var queryResult = await _mediator.Send(new GetAllEnumPlacementTestSkillsQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get All Course type and Course level
        /// </summary>
        [HttpGet("all-course-level")]
        [ProducesResponseType(typeof(MethodResult<object>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetEnumCourseLevelsAsync()
        {
            var queryResult = await _mediator.Send(new GetAllEnumCourseLevelQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search Course Source Data
        /// </summary>
        [HttpGet("all-work-flow")]
        [ProducesResponseType(typeof(MethodResult<IList<EnumModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetEnumWorkFlowDatesAsync([FromQuery] GetEnumWorkFlowQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search Course Source Data
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<EnumModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        public async Task<IActionResult> GetEnumCourseSourceDatasAsync([FromQuery] EnumCourseSourceData courseSource)
        {
            var queryResult = await _mediator.Send(new GetEnumQuery { EnumCourseSourceData = courseSource }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Review Question Type
        /// </summary>
        [HttpGet("review-question-type")]
        [ProducesResponseType(typeof(MethodResult<object>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetReviewQuestionType([FromQuery] GetEnumReviewQuestionTypeQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Save flows
        /// </summary>
        [HttpPost("save-flows")]
        [ProducesResponseType(typeof(MethodResult<CategoryModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SaveFlows([FromBody] SaveFlowsCommand command)
        {
            var methodResult = await _mediator.Send(command).ConfigureAwait(false);
            return methodResult.GetActionResult();
        }

        /// <summary>
        /// Create category
        /// </summary>
        [HttpPost("create-category")]
        [ProducesResponseType(typeof(MethodResult<CategoryModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.MasterAdmin))]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand command)
        {
            var methodResult = await _mediator.Send(command).ConfigureAwait(false);
            return methodResult.GetActionResult();
        }

        /// <summary>
        /// update category
        /// </summary>
        [HttpPut("update-category")]
        [ProducesResponseType(typeof(MethodResult<CategoryModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.MasterAdmin))]
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryCommand command)
        {
            var methodResult = await _mediator.Send(command).ConfigureAwait(false);
            return methodResult.GetActionResult();
        }

        /// <summary>
        /// delete category
        /// </summary>
        [HttpDelete("delete-category/{id}")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.MasterAdmin))]
        public async Task<IActionResult> DeleteCategory([FromRoute] Guid id)
        {
            var methodResult = await _mediator.Send(new DeleteCategoryCommand { Id = id }).ConfigureAwait(false);
            return methodResult.GetActionResult();
        }

        /// <summary>
        /// Archive category
        /// </summary>
        [HttpPut("archive-category/{id}")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.MasterAdmin))]
        public async Task<IActionResult> ArchiveCategory([FromRoute] Guid id)
        {
            var methodResult = await _mediator.Send(new ArchiveCategoryCommand { Id = id }).ConfigureAwait(false);
            return methodResult.GetActionResult();
        }

        /// <summary>
        /// get category by id
        /// </summary>
        [HttpGet("category/{id}")]
        [ProducesResponseType(typeof(MethodResult<CategoryModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.MasterAdmin))]
        public async Task<IActionResult> GetCategoryById([FromRoute] Guid id)
        {
            var methodResult = await _mediator.Send(new GetCategoryByIdQuery { Id = id }).ConfigureAwait(false);
            return methodResult.GetActionResult();
        }

        /// <summary>
        /// get category tree
        /// </summary>
        [HttpGet("category-tree")]
        [ProducesResponseType(typeof(MethodResult<IList<CategoryTreeModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.MasterAdmin))]
        public async Task<IActionResult> GetCategoryTree([FromQuery] GetCategoryTreeQuery command)
        {
            var methodResult = await _mediator.Send(command).ConfigureAwait(false);
            return methodResult.GetActionResult();
        }

        /// <summary>
        /// get subject tree
        /// </summary>
        [HttpGet("subject-tree")]
        [ProducesResponseType(typeof(MethodResult<IList<CategoryTreeModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.MasterAdmin))]
        public async Task<IActionResult> GetSubjectTree()
        {
            var methodResult = await _mediator.Send(new GetSubjectTreeQuery()).ConfigureAwait(false);
            return methodResult.GetActionResult();
        }

        /// <summary>
        /// search category tree
        /// </summary>
        [HttpGet("search-category")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<CategoryModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.MasterAdmin))]
        public async Task<IActionResult> SearchCategory([FromQuery] SearchCategoryQuery command)
        {
            var methodResult = await _mediator.Send(command).ConfigureAwait(false);
            return methodResult.GetActionResult();
        }

        /// <summary>
        /// create program
        /// </summary>
        [HttpPost("create-program")]
        [ProducesResponseType(typeof(MethodResult<ProgramModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.MasterAdmin))]
        public async Task<IActionResult> CreateProgram([FromBody] CreateProgramCommand command)
        {
            var methodResult = await _mediator.Send(command).ConfigureAwait(false);
            return methodResult.GetActionResult();
        }

        /// <summary>
        /// update program
        /// </summary>
        [HttpPut("update-program")]
        [ProducesResponseType(typeof(MethodResult<ProgramModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.MasterAdmin))]
        public async Task<IActionResult> UpdateProgram([FromBody] UpdateProgramCommand command)
        {
            var methodResult = await _mediator.Send(command).ConfigureAwait(false);
            return methodResult.GetActionResult();
        }

        /// <summary>
        /// get program by id
        /// </summary>
        [HttpGet("program/{programId}")]
        [ProducesResponseType(typeof(MethodResult<ProgramModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.MasterAdmin))]
        public async Task<IActionResult> GetProgramById([FromRoute] Guid programId)
        {
            var methodResult = await _mediator.Send(new GetProgramByIdQuery { ProgramId = programId }).ConfigureAwait(false);
            return methodResult.GetActionResult();
        }

        /// <summary>
        /// search program
        /// </summary>
        [HttpGet("program")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<CategoryModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.MasterAdmin))]
        public async Task<IActionResult> GetProgram([FromQuery] GetProgramQuery query)
        {
            var methodResult = await _mediator.Send(query).ConfigureAwait(false);
            return methodResult.GetActionResult();
        }
    }
}
