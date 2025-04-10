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
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;
        private readonly IStudentRepository _studentRepository;

        public GetDashboardEventConfigQueryHandler(ICompetitionEventsRepository competitionEventsRepository,
                                                   AuthContext authContext,
                                                   IStudentCompetitionEventsRepository studentCompetitionEventsRepository,
                                                   IStudentRepository studentRepository)
        {
            _competitionEventsRepository = competitionEventsRepository;
            _authContext = authContext;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<DashboardEventConfig>> Handle(GetDashboardEventConfigQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<DashboardEventConfig> methodResult = new MethodResult<DashboardEventConfig>();

            var student = await _studentRepository.Queryable.FirstOrDefaultAsync(x => x.Human != null && x.Human.UserId == _authContext.CurrentUserId, cancellationToken);
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student), student);
                return methodResult;
            }

            var studentCompetitionEvent = await _studentCompetitionEventsRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == student.Id, cancellationToken);
            if (studentCompetitionEvent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentCompetitionEvent), studentCompetitionEvent);
                return methodResult;
            }

            // lấy config ở event to nhất
            var dashboardEventConfig = await ParentEventAsync(studentCompetitionEvent.CompetitionEventId, cancellationToken);

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
                await ParentEventAsync(competitionEvent.ParentEventId.Value, cancellationToken);
            }

            return competitionEvent?.DashboardEventConfig;
        }

    }
}
