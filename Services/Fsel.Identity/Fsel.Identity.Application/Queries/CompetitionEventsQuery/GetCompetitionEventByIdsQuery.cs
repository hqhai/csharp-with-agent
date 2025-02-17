// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CompetitionEventsQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCompetitionEventByIdsQuery : IRequest<MethodResult<IList<CompetitionEventsModel>>>
    {
        public IList<Guid>? Ids { get; set; }
    }

    public class GetCompetitionEventByIdsQueryHandler : IRequestHandler<GetCompetitionEventByIdsQuery, MethodResult<IList<CompetitionEventsModel>>>
    {
        private readonly ICompetitionEventsRepository _competitionEventsRepository;

        public GetCompetitionEventByIdsQueryHandler(ICompetitionEventsRepository competitionEventsRepository)
        {
            _competitionEventsRepository = competitionEventsRepository;
        }

        public async Task<MethodResult<IList<CompetitionEventsModel>>> Handle(GetCompetitionEventByIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.Ids);
            MethodResult<IList<CompetitionEventsModel>> methodResult = new MethodResult<IList<CompetitionEventsModel>>();

            var competitionEvents = await _competitionEventsRepository.Queryable.Where(x => request.Ids.Contains(x.Id)).ToListAsync(cancellationToken);
            if (competitionEvents == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(competitionEvents), competitionEvents);
                return methodResult;
            }

            List<CompetitionEventsModel> competitionEventResults = new List<CompetitionEventsModel>();

            foreach (var competitionEvent in competitionEvents)
            {
                competitionEventResults.Add(new CompetitionEventsModel
                {
                    Id = competitionEvent.Id,
                    EventCode = competitionEvent.EventCode
                });
            }

            methodResult.Result = competitionEventResults;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
