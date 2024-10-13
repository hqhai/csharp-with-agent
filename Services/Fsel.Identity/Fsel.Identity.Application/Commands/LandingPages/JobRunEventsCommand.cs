// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.LandingPages
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class JobRunEventsCommand : IRequest<MethodResult<bool>>
    {
    }

    public class JobRunEventsCommandHandler : IRequestHandler<JobRunEventsCommand, MethodResult<bool>>
    {
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IEventRegistrationRepository _eventRegistrationRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public JobRunEventsCommandHandler(ICompetitionEventsRepository competitionEventsRepository, IEventRegistrationRepository eventRegistrationRepository, IMediator mediator, IMapper mapper)
        {
            _competitionEventsRepository = competitionEventsRepository;
            _eventRegistrationRepository = eventRegistrationRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<MethodResult<bool>> Handle(JobRunEventsCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var currentDate = DateTime.UtcNow;

            var @events = await _competitionEventsRepository.Queryable.ToListAsync(cancellationToken);

            @events = @events.Where(p => p.EventContent != null && p.EventContent.StartDate.HasValue && p.EventContent.StartDate.Value.Date == currentDate.Date && (p.EventContent.Actions == null || p.EventContent.Actions.Count == 0 || !p.EventContent.Actions.Any(p => p == EnumSchoolEventRuleAction.RegisterAndCreateUser))).OrderByDescending(p => p.CreatedDate).ToList();

            var eventIds = @events.Select(e => e.Id).ToList();

            var eventRegistrations = await _eventRegistrationRepository.Queryable.Where(p => eventIds != null &&
            eventIds.Contains(p.CompetitionEventId) &&
            p.StudentId == null).ToListAsync(cancellationToken);

            foreach (var @event in @events)
            {
                var studentEvents = eventRegistrations.Where(p => p.CompetitionEventId == @event.Id).ToList();
                foreach (var studentEvent in studentEvents)
                {
                    await _mediator.Send(new RegisterStudentForEventCommand()
                    {
                        EventCode = @event.EventCode,
                        FirstName = studentEvent.FirstName,
                        LastName = studentEvent.LastName,
                        Email = studentEvent.Email,
                        BirthDay = studentEvent.BirthDay,
                        PhoneNumber = studentEvent.PhoneNumber,
                        School = studentEvent.School,
                        SchoolClass = studentEvent.SchoolClass,
                        SchoolGrade = studentEvent.SchoolGrade,
                        SchoolId = studentEvent.SchoolId,
                        SchoolStudentCode = studentEvent.SchoolStudentCode
                    }, cancellationToken);
                }
            }

            return methodResult;
        }
    }
}
