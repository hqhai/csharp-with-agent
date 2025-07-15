// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentRankingEvents
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateStudentCompetitionEventsCommand : IRequest<MethodResult<IList<StudentCompetitionEventsModel>>>
    {
        public IList<string>? Emails { get; set; }

        public string? EventCode { get; set; }
    }

    public class CreateStudentCompetitionEventsCommandHandler : IRequestHandler<CreateStudentCompetitionEventsCommand, MethodResult<IList<StudentCompetitionEventsModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IStudentRepository _studentRepository;

        public CreateStudentCompetitionEventsCommandHandler(IMapper mapper, IStudentCompetitionEventsRepository studentRankingEventsRepository, IStudentRepository studentRepository, ICompetitionEventsRepository competitionEventsRepository)
        {
            _mapper = mapper;
            _studentCompetitionEventsRepository = studentRankingEventsRepository;
            _studentRepository = studentRepository;
            _competitionEventsRepository = competitionEventsRepository;
        }

        public async Task<MethodResult<IList<StudentCompetitionEventsModel>>> Handle(CreateStudentCompetitionEventsCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<StudentCompetitionEventsModel>> methodResult = new MethodResult<IList<StudentCompetitionEventsModel>>();
            request.Emails = request.Emails ?? new List<string>();

            var studentResultIds = await _studentRepository.Queryable.Where(x => x.Human != null && x.Human!.Email != null && request.Emails!.Contains(x.Human.Email)).Select(x => x.Id).ToListAsync(cancellationToken);
            var competitionEvent = await _competitionEventsRepository.Queryable.FirstOrDefaultAsync(x => x.EventCode == request.EventCode, cancellationToken);

            if (competitionEvent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.EventCode), request.EventCode);
                return methodResult;
            }

            IList<StudentCompetitionEvent> studentRankingEvents = new List<StudentCompetitionEvent>();
            studentResultIds.ForEach(item =>
            {
                StudentCompetitionEvent studentRankingEvent = new StudentCompetitionEvent
                {
                    StudentId = item,
                    CompetitionEventId = competitionEvent.Id
                };
                studentRankingEvents.Add(studentRankingEvent);
            });

            await _studentCompetitionEventsRepository.BulkMergeAsync(studentRankingEvents, bulk =>
            {
                bulk.ColumnPrimaryKeyExpression = entity => new { entity.CompetitionEventId, entity.StudentId };
            });
            methodResult.StatusCode = StatusCodes.Status201Created;
            methodResult.Result = _mapper.Map<List<StudentCompetitionEventsModel>>(studentRankingEvents);
            return methodResult;
        }
    }
}
