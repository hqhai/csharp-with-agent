// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.CourseGoalCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories.CourseGoals;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteCourseGoalsCommand : IRequest<MethodResult<bool>>
    {
        public IList<Guid>? Ids { get; set; }
    }

    public class DeleteCourseGoalsCommandHandler : IRequestHandler<DeleteCourseGoalsCommand, MethodResult<bool>>
    {
        private readonly ICourseGoalRepository _courseGoalRepository;

        public DeleteCourseGoalsCommandHandler(ICourseGoalRepository courseGoalRepository)
        {
            _courseGoalRepository = courseGoalRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteCourseGoalsCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            if (request.Ids == null || !request.Ids.Any())
            {
                return methodResult;
            }

            var courseGoals = await _courseGoalRepository.Queryable.Include(x => x.CourseGoalConfigs).WhereBulkContains(request.Ids, x => x.Id).ToListAsync(cancellationToken);
            if (courseGoals == null || !courseGoals.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Ids), request.Ids);
                return methodResult;
            }
            await _courseGoalRepository.ExecuteTransactionAsync(async () =>
            {
                await _courseGoalRepository.DeleteListAsync(courseGoals);
                await _courseGoalRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
