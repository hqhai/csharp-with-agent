// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CompetitionEventsQuery
{
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using static System.Runtime.InteropServices.JavaScript.JSType;

    public class GetCompetitionEventsQuery : IRequest<MethodResult<CompetitionEventsModel>>
    {
        public string? EventCode { get; set; }
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
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
