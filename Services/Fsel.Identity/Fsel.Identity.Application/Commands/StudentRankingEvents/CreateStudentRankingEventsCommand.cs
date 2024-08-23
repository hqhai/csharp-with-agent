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

    public class CreateStudentRankingEventsCommand : IRequest<MethodResult<IList<StudentRankingEventsModel>>>
    {
        public IList<string>? Emails { get; set; }

        public string? EventCode { get; set; }
    }

    public class CreateStudentRankingEventsCommandHandler : IRequestHandler<CreateStudentRankingEventsCommand, MethodResult<IList<StudentRankingEventsModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IStudentRankingEventsRepository _studentRankingEventsRepository;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IStudentRepository _studentRepository;
        public CreateStudentRankingEventsCommandHandler(IMapper mapper, IStudentRankingEventsRepository studentRankingEventsRepository, IStudentRepository studentRepository, ICompetitionEventsRepository competitionEventsRepository)
        {
            _mapper = mapper;
            _studentRankingEventsRepository = studentRankingEventsRepository;
            _studentRepository = studentRepository;
            _competitionEventsRepository = competitionEventsRepository;
        }

        public async Task<MethodResult<IList<StudentRankingEventsModel>>> Handle(CreateStudentRankingEventsCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<StudentRankingEventsModel>> methodResult = new MethodResult<IList<StudentRankingEventsModel>>();

            var studentResultIds = _studentRepository.Queryable.Where(x => x.Human != null && x.Human!.Email != null && request.Emails!.Contains(x.Human.Email)).Select(x => x.Id).ToList();

            var competitionEvents = _competitionEventsRepository.Queryable.FirstOrDefault(x => x.EventCode == request.EventCode);

            if (competitionEvents == null)
            {
                methodResult.AddError(nameof(EnumSystemErrorCode.DataNotExist), nameof(competitionEvents));
                return methodResult;
            }

            IList<StudentRankingEvents> studentRankingEvents = new List<StudentRankingEvents>();

            studentResultIds.ForEach(item =>
            {
                StudentRankingEvents studentRankingEvent = new StudentRankingEvents
                {
                    StudentId = item,
                    CompetitionRankingId = competitionEvents.Id
                };
                studentRankingEvents.Add(studentRankingEvent);
            });


            await _studentRankingEventsRepository.ExecuteTransactionAsync(async () =>
            {
                await _studentRankingEventsRepository.AddList(studentRankingEvents);
                await _studentRankingEventsRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<List<StudentRankingEventsModel>>(studentRankingEvents);
                return methodResult;
            });


            return methodResult;
        }


    }
}
