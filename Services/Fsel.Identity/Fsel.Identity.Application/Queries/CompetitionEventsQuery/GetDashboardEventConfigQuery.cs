// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CompetitionEventsQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetDashboardEventConfigQuery : IRequest<MethodResult<DashboardEventConfig>>
    {
    }

    public class GetDashboardEventConfigQueryHandler : IRequestHandler<GetDashboardEventConfigQuery, MethodResult<DashboardEventConfig>>
    {
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly AuthContext _authContext;
        private readonly IEventManagerRepository _eventManagerRepository;

        public GetDashboardEventConfigQueryHandler(ICompetitionEventsRepository competitionEventsRepository,
                                                   AuthContext authContext,
                                                   IEventManagerRepository eventManagerRepository)
        {
            _competitionEventsRepository = competitionEventsRepository;
            _authContext = authContext;
            _eventManagerRepository = eventManagerRepository;
        }

        public async Task<MethodResult<DashboardEventConfig>> Handle(GetDashboardEventConfigQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<DashboardEventConfig> methodResult = new MethodResult<DashboardEventConfig>();

            var eventManager = await _eventManagerRepository.Queryable.FirstOrDefaultAsync(x => x.UserId == _authContext.CurrentUserId, cancellationToken);
            if (eventManager == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(eventManager));
                return methodResult;
            }

            // lấy config ở event to nhất
            var dashboardEventConfig = await ParentEventAsync(eventManager.CompetitionEventId, cancellationToken);

            methodResult.Result = dashboardEventConfig;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<DashboardEventConfig?> ParentEventAsync(Guid parentEventId, CancellationToken cancellationToken)
        {
            var competitionEvent = await _competitionEventsRepository.Queryable.FirstOrDefaultAsync(x => x.Id == parentEventId, cancellationToken);
            if (competitionEvent?.ParentEventId == null)
            {
                return competitionEvent?.DashboardEventConfig;
            }
            else
            {
                return await ParentEventAsync(competitionEvent.ParentEventId.Value, cancellationToken);
            }
        }

    }
}
