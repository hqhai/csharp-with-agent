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

            var studentResultIds = _studentRepository.Queryable.Where(x => x.User!.Email != null && request.Emails!.Contains(x.User.Email)).Select(x => x.Id).ToList();

            var competitionEvents = _competitionEventsRepository.Queryable.FirstOrDefault(x => x.EventCode == request.EventCode);

            if (competitionEvents == null)
            {
                methodResult.AddError(nameof(EnumSystemErrorCode.DataNotExist), nameof(competitionEvents));
                return methodResult;
            }

            IList<StudentCompetitionEvent> studentRankingEvents = new List<StudentCompetitionEvent>();

            studentResultIds.ForEach(item =>
            {
                StudentCompetitionEvent studentRankingEvent = new StudentCompetitionEvent
                {
                    StudentId = item,
                    CompetitionEventId = competitionEvents.Id
                };
                studentRankingEvents.Add(studentRankingEvent);
            });


            await _studentCompetitionEventsRepository.ExecuteTransactionAsync(async () =>
            {
                await _studentCompetitionEventsRepository.AddList(studentRankingEvents);
                await _studentCompetitionEventsRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<List<StudentCompetitionEventsModel>>(studentRankingEvents);
                return methodResult;
            });


            return methodResult;
        }


    }
}
