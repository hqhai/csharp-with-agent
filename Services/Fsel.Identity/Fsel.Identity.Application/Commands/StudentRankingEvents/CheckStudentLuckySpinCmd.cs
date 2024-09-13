// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentRankingEvents

{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CheckStudentLuckySpinCmd : IRequest<MethodResult<IList<CompetitionEventsModel>?>>
    {
        public Guid? UserId { get; set; }
    }

    public class CheckStudentLuckySpinCmdHandler : IRequestHandler<CheckStudentLuckySpinCmd, MethodResult<IList<CompetitionEventsModel>?>>
    {
        private readonly AuthContext _authContext;
        private readonly IStudentCompetitionEventsRepository _studentRankingEventsRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IMapper _mapper;

        public CheckStudentLuckySpinCmdHandler(AuthContext authContext, IStudentCompetitionEventsRepository studentRankingEventsRepository, IStudentRepository studentRepository, IMapper mapper)
        {
            _authContext = authContext;
            _studentRankingEventsRepository = studentRankingEventsRepository;
            _studentRepository = studentRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<CompetitionEventsModel>?>> Handle(CheckStudentLuckySpinCmd request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<CompetitionEventsModel>?>();

            var userId = request.UserId ?? _authContext.CurrentUserId;

            var student = await _studentRepository.Queryable.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

            var studentRankingEvents = await _studentRankingEventsRepository.Queryable
                .Include(x => x.CompetitionEvents)
                .Where(x => student != null && x.StudentId == student.Id)
                .ToListAsync(cancellationToken);

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            var competitionEvents = studentRankingEvents.Where(x => x.CompetitionEvents != null && x.CompetitionEvents.EventContent != null && x.CompetitionEvents.EventContent.LuckySpin && x.CompetitionEvents.EventContent.StartDate.HasValue && x.CompetitionEvents.EventContent.EndDate.HasValue && x.CompetitionEvents.EventContent.StartDate.Value.Date <= currentDate.Date && x.CompetitionEvents.EventContent.EndDate.Value.Date >= currentDate.Date)
                .Select(x => x.CompetitionEvents);

            methodResult.Result = _mapper.Map<IList<CompetitionEventsModel>?>(competitionEvents);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
