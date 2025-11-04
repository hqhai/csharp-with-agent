// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.StudentGoalAggregateCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class DeleteStudentGoalAggregateCommand : IRequest<MethodResult<bool>>
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
    }

    public class DeleteStudentGoalAggregateCommandHandler : IRequestHandler<DeleteStudentGoalAggregateCommand, MethodResult<bool>>
    {
        private readonly IStudentGoalAggregateRepository _studentGoalAggregateRepository;

        public DeleteStudentGoalAggregateCommandHandler(IStudentGoalAggregateRepository studentGoalAggregateRepository)
        {
            _studentGoalAggregateRepository = studentGoalAggregateRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteStudentGoalAggregateCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            var studentGoalAggregate = await _studentGoalAggregateRepository.Queryable.Include(x => x.StudentGoalSummaries)
                                                          .Where(x => x.CourseId == request.CourseId && x.StudentId == request.StudentId)
                                                          .FirstOrDefaultAsync(cancellationToken);
            if (studentGoalAggregate == null)
            {
                return methodResult;
            }

            await _studentGoalAggregateRepository.DeleteAsync(studentGoalAggregate);
            await _studentGoalAggregateRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            methodResult.Result = true;
            return methodResult;
        }
    }
}
