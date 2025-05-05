// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentRanking
{
    using System.Collections.Generic;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetEventsByUserIdQuery : IRequest<MethodResult<IList<CompetitionEventsModel>>>
    {
        public Guid? UserId { get; set; }
        public EnumSchoolEventRuleAction? Action { get; set; }
    }

    public class GetEventsByUserIdQueryHandler : IRequestHandler<GetEventsByUserIdQuery, MethodResult<IList<CompetitionEventsModel>>>
    {
        private readonly AuthContext _authContext;
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IMapper _mapper;

        public GetEventsByUserIdQueryHandler(AuthContext authContext, IStudentCompetitionEventsRepository studentCompetitionEventsRepository, IStudentRepository studentRepository, IMapper mapper)
        {
            _authContext = authContext;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
            _studentRepository = studentRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<CompetitionEventsModel>>> Handle(GetEventsByUserIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<CompetitionEventsModel>>();

            var userId = request.UserId ?? _authContext.CurrentUserId;

            var student = await _studentRepository.Queryable.Include(p => p.Human).FirstOrDefaultAsync(x => x.Human != null && x.Human.UserId == userId, cancellationToken);
            var studentRankingEvents = await _studentCompetitionEventsRepository.Queryable
                .Include(x => x.CompetitionEvents)
                .Where(x => student != null && x.StudentId == student.Id)
                .ToListAsync(cancellationToken);

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);
            var competitionEvents = studentRankingEvents.Where(x => x.CompetitionEvents != null &&
            x.CompetitionEvents.EventContent != null &&
            ((!x.CompetitionEvents.EventContent.StartDate.HasValue && !x.CompetitionEvents.EventContent.EndDate.HasValue) ||
            (x.CompetitionEvents.EventContent.StartDate.HasValue &&
            x.CompetitionEvents.EventContent.EndDate.HasValue &&
            x.CompetitionEvents.EventContent.EndDate.Value.Date >= currentDate.Date)))
                .Select(x => x.CompetitionEvents);

            if (request.Action.HasValue)
            {
                competitionEvents = competitionEvents.Where(p => p != null && p.EventContent != null && p.EventContent.Actions != null && p.EventContent.Actions.Contains(request.Action.Value) && (p.EventContent.ActionConfigs == null || !p.EventContent.ActionConfigs.Any(m => m.Action == request.Action.Value && m.EndDate.HasValue && m.EndDate.Value < currentDate))).ToList();
            }

            methodResult.Result = _mapper.Map<IList<CompetitionEventsModel>>(competitionEvents);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
