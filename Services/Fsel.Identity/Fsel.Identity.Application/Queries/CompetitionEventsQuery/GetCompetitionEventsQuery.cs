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

    public class GetCompetitionEventsQuery : IRequest<MethodResult<CompetitionEventsModel>>
    {
        public string? EventCode { get; set; }

        public bool IsLeaderBoard { get; set; }
    }
    public class GetCompetitionEventsQueryHandler : IRequestHandler<GetCompetitionEventsQuery, MethodResult<CompetitionEventsModel>>
    {
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IMapper _mapper;
        public GetCompetitionEventsQueryHandler(ICompetitionEventsRepository competitionEventsRepository, IMapper mapper)
        {
            _competitionEventsRepository = competitionEventsRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<CompetitionEventsModel>> Handle(GetCompetitionEventsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CompetitionEventsModel>();

            var competition = _competitionEventsRepository.Queryable.FirstOrDefault(x => x.EventCode == request.EventCode);
            methodResult.Result = _mapper.Map<CompetitionEventsModel>(competition);

            if (request.IsLeaderBoard && competition != null)
            {
                methodResult.Result.EventContent = competition.LBConfig;
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
