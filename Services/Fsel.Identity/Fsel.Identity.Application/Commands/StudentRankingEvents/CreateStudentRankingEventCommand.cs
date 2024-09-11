// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentRankingEvents
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.StudentRanking;
    using MediatR;

    public class CreateStudentRankingEventCommand : StudentRankingEventCommandModel, IRequest<MethodResult<StudentRankingEvent>>
    {

    }

    public class CreateStudentRankingEventCommandHandler : IRequestHandler<CreateStudentRankingEventCommand, MethodResult<StudentRankingEvent>>
    {
        private readonly IStudentRankingEventRepository _studentRankingEventRepository;
        private readonly IMapper _mapper;

        public CreateStudentRankingEventCommandHandler(IStudentRankingEventRepository studentRankingEventRepository, IMapper mapper)
        {
            _studentRankingEventRepository = studentRankingEventRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<StudentRankingEvent>> Handle(CreateStudentRankingEventCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentRankingEvent> methodResult = new MethodResult<StudentRankingEvent>();

            var existStudentRankingEvent = _studentRankingEventRepository.Queryable.FirstOrDefault(x => x.StudentId == request.StudentId && x.CourseResultId == request.CourseResultId);

            StudentRankingEvent studentRankingEvent = new StudentRankingEvent();

            if (existStudentRankingEvent != null)
            {
                studentRankingEvent = _mapper.Map(request, existStudentRankingEvent);
            }
            else
            {
                studentRankingEvent = _mapper.Map<StudentRankingEvent>(request);
            }

            await _studentRankingEventRepository.ExecuteTransactionAsync(async () =>
            {
                _studentRankingEventRepository.Update(studentRankingEvent);
                await _studentRankingEventRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

                methodResult.Result = studentRankingEvent;
                return methodResult;
            });

            return methodResult;
        }

    }
}
