// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.System.Application.Commands.FeatureAccessTimeCmd;
    using Fsel.System.Application.Queries.FeatureAccessTimeQuery;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/feature-access-time")]
    [ApiController]
    public class FeatureAccessTimeController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IFeatureAccessTimeRepository _featureAccessTimeRepository;

        public FeatureAccessTimeController(IMediator mediator, IFeatureAccessTimeRepository featureAccessTimeRepository)
        {
            _mediator = mediator;
            _featureAccessTimeRepository = featureAccessTimeRepository;
        }

        /// <summary>
        /// execute list query
        /// </summary>
        [HttpGet("execute-list-query")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ReceiveToken([FromQuery] BaseQueryModel query)
        {
            SetQuery(query);
            var commandResult = await _featureAccessTimeRepository.GetListResultAsync<FeatureAccessTimeModel>(query);
            return commandResult.GetActionResult();
        }

        [HttpPost("get-feature-accesstime")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetFeatureAccessTime([FromBody] GetAccessTimeByUserAndFeatureCommand cmd)
        {
            var commandResult = await _mediator.Send(cmd).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Feature Access Time Detail
        /// </summary>
        [HttpGet("get-detail")]
        [ProducesResponseType(typeof(MethodResult<FeatureAccessTimeModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetDetail([FromQuery] GetFeatureAccessTimeQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Feature Access Times
        /// </summary>
        [HttpPost("gets")]
        [ProducesResponseType(typeof(MethodResult<IList<FeatureAccessTimeModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Gets([FromBody] GetFeatureAccessTimesQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Feature Access Times
        /// </summary>
        [HttpPost("get-to-modules")]
        [ProducesResponseType(typeof(MethodResult<IList<FeatureAccessTimeModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Gets([FromBody] GetFeatureAccessTimeModulesQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Save Feature Access Time
        /// </summary>
        [HttpPost("save")]
        [ProducesResponseType(typeof(MethodResult<FeatureAccessTimeModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Student))]
        public async Task<IActionResult> Save([FromBody] SaveFeatureAccessTimeCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Student Feature Access Time
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpGet("access-time-chart")]
        [ProducesResponseType(typeof(MethodResult<IList<FeatureAcessTimeChartModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Student))]
        public async Task<IActionResult> GetStudentFeatureAccessTime([FromQuery] GetFeatureAccessTimeChartQuery query)
        {
            MethodResult<IList<FeatureAcessTimeChartModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get feature access business
        /// </summary>
        [HttpGet("get-feature-access-time-business")]
        [ProducesResponseType(typeof(MethodResult<IList<FeatureAccessTimeBusinessModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetFeatureAccessBusiness([FromQuery] GetFeatureAccessTimeBusinessQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get feature access business
        /// </summary>
        [HttpPost("get-feature-access-time-by-userIds")]
        [ProducesResponseType(typeof(MethodResult<IList<FeatureAccessTimeBusinessModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetFeatureAccessTimeByUserId([FromBody] IList<Guid> userIds)
        {
            var queryResult = await _mediator.Send(new GetFeatureAccessTimeByUserIdQuery { UserIds = userIds }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get feature access business
        /// </summary>
        [HttpPost("get-feature-access-time-to-modules")]
        [ProducesResponseType(typeof(MethodResult<IList<FeatureAccessTimeBusinessModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromBody] GetFeatureAccessTimeStudentToExportQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [HttpPost("get-feature-access-time-by-user-ids")]
        [ProducesResponseType(typeof(MethodResult<List<FeatureAccessTimeModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> GetFeatureAccessTimeByUserIds([FromBody] GetFeatureAccessTimesByUserIdsQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        [HttpPost("get-last-feature-access-by-user-ids")]
        [ProducesResponseType(typeof(MethodResult<List<FeatureAccessTimeModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> GetLastFeatureAccessByUserIds([FromBody] GetUserLastAccessByUserIdsQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
