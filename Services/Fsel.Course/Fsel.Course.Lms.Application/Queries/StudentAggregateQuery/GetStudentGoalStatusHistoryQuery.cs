// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentAggregateQuery
{
    using AutoMapper;
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
        private readonly IMapper  _mapper;

        public GetStudentGoalStatusHistoryQueryHandler(IStatusStudentGoalRepository statusStudentGoalRepository, IMapper mapper)
        {
            _statusStudentGoalRepository = statusStudentGoalRepository;
            _mapper = mapper;
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

            methodResult.Result = _mapper.Map<StausStudentGoalHistoryModel>(studentGoalHistory);
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }
    }
}
