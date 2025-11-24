// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentAggregateQuery
{
    using Common.ActionResults;
    using Common.Enums.ErrorCodes;
    using Domain.IRepositories;
    using Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentGoalStatusHistoryQuery : IRequest<MethodResult<StausStudentGoalHistoryModel>>
    {
        public Guid StudentId { get; set; }
    }

    public class GetStudentGoalStatusHistoryQueryHandler : IRequestHandler<GetStudentGoalStatusHistoryQuery, MethodResult<StausStudentGoalHistoryModel>>
    {
        private readonly IStatusStudentGoalRepository _statusStudentGoalRepository;

        public GetStudentGoalStatusHistoryQueryHandler(IStatusStudentGoalRepository statusStudentGoalRepository)
        {
            _statusStudentGoalRepository = statusStudentGoalRepository;
        }

        public async Task<MethodResult<StausStudentGoalHistoryModel>> Handle(GetStudentGoalStatusHistoryQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<StausStudentGoalHistoryModel>();

            var studentGoalHistory = await _statusStudentGoalRepository.ReadQueryable
                .FirstOrDefaultAsync(x => x.StudentId == request.StudentId, cancellationToken);

            if (studentGoalHistory == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var result = new StausStudentGoalHistoryModel();

            result.StudentId = studentGoalHistory.StudentId;
            result.StatusStudentGoal =  studentGoalHistory.StatusStudentGoal;
            result.CreatedDate = studentGoalHistory.CreatedDate;
            result.CreatedFullName = studentGoalHistory.CreatedFullName;

            methodResult.Result = result;
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }
    }
}
