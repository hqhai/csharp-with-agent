// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CompetitionEventsQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCompetitionEventsByDateQuery : IRequest<MethodResult<IList<CompetitionEventsModel>>>
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }

    public class GetCompetitionEventsByDateQueryHandler : IRequestHandler<GetCompetitionEventsByDateQuery, MethodResult<IList<CompetitionEventsModel>>>
    {
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IMapper _mapper;

        public GetCompetitionEventsByDateQueryHandler(ICompetitionEventsRepository competitionEventsRepository, IMapper mapper)
        {
            _competitionEventsRepository = competitionEventsRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<CompetitionEventsModel>>> Handle(GetCompetitionEventsByDateQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<CompetitionEventsModel>> methodResult = new MethodResult<IList<CompetitionEventsModel>>();

            var competitions = await _competitionEventsRepository.Queryable.ToListAsync(cancellationToken);
            if (competitions == null)
            {
                methodResult.Result = null;
                return methodResult;
            }

            competitions = competitions.Where(x => request.StartDate <= x.EventContent?.EndDate && request.EndDate >= x.EventContent?.StartDate).ToList();

            methodResult.Result = _mapper.Map<IList<CompetitionEventsModel>>(competitions);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
