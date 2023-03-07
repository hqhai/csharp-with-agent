using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Common.Helpers;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Application.Commands.Course;
using Fsel.Course.Application.Commands.UnitCmd;
using Fsel.Course.Application.Queries.CourseQuery;
using Fsel.Course.Application.Queries.UnitQuery;
using Fsel.Course.Common.Models.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Fsel.Course.Api.Controllers
{
    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/course")]
    [ApiController]
    [Authorize]
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
    }
}