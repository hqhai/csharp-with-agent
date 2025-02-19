// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CompetitionEventsQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCompetitionEventsToEventCodeStrQuery : IRequest<MethodResult<IList<CompetitionEventsModel>>>
    {
        public string? EventCodeStr { get; set; }

        public IList<string>? EventCodes
        {
            get
            {
                return EventCodeStr.ToList<string>();
            }
        }
    }

    public class GetCompetitionEventsToEventCodeStrQueryHandler : IRequestHandler<GetCompetitionEventsToEventCodeStrQuery, MethodResult<IList<CompetitionEventsModel>>>
    {
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IMapper _mapper;

        public GetCompetitionEventsToEventCodeStrQueryHandler(ICompetitionEventsRepository competitionEventsRepository, IMapper mapper)
        {
            _competitionEventsRepository = competitionEventsRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<CompetitionEventsModel>>> Handle(GetCompetitionEventsToEventCodeStrQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<CompetitionEventsModel>>();
            var competitionEvents = await _competitionEventsRepository.Queryable.Where(x => !string.IsNullOrEmpty(x.EventCode) && request.EventCodes != null && request.EventCodes.Contains(x.EventCode)).ToListAsync(cancellationToken);
            methodResult.Result = _mapper.Map<IList<CompetitionEventsModel>>(competitionEvents);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
