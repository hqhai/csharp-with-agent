// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.StudentGoalAggregateCmd
{
    using AutoMapper;
    using Common.ActionResults;
    using Common.Enums.ErrorCodes;
    using Domain.Entities;
    using Domain.IRepositories;
    using Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Shared.Enums;

    public class UpdateStudentGoalStatusCommand : IRequest<MethodResult<StausStudentGoalHistoryModel>>
    {
        public Guid StudentGoalId { get; set; }
        public EnumStatusStudentGoal StatusStudentGoal { get; set; }
    }

    public class UpdateStudentGoalStatusCommandHandler : IRequestHandler<UpdateStudentGoalStatusCommand, MethodResult<StausStudentGoalHistoryModel>>
    {
        private readonly IStudentGoalAggregateRepository  _studentGoalAggregateRepository;
        private readonly IStatusStudentGoalRepository _statusStudentGoalRepository;
        private readonly IMapper _mapper;

        public UpdateStudentGoalStatusCommandHandler(IStudentGoalAggregateRepository studentGoalAggregateRepository,
            IStatusStudentGoalRepository statusStudentGoalRepository,
            IMapper mapper)
        {
            _studentGoalAggregateRepository = studentGoalAggregateRepository;
            _statusStudentGoalRepository = statusStudentGoalRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<StausStudentGoalHistoryModel>> Handle(UpdateStudentGoalStatusCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<StausStudentGoalHistoryModel>();

            var exits = await _studentGoalAggregateRepository.ReadQueryable
                .FirstOrDefaultAsync(x => x.Id == request.StudentGoalId, cancellationToken);

            if (exits == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            exits.StatusStudentGoal = request.StatusStudentGoal;

            var statusHistoty = new StatusStudentGoalHistory();
            statusHistoty.StatusStudentGoal = request.StatusStudentGoal;
            statusHistoty.StudentId = exits.StudentId;

            var result = new StausStudentGoalHistoryModel();
            result.StatusStudentGoal = statusHistoty.StatusStudentGoal;
            result.StudentId = statusHistoty.StudentId;

            await _statusStudentGoalRepository.ExecuteTransactionAsync(async () =>
            {
                exits = _studentGoalAggregateRepository.Update(exits);
                statusHistoty = _statusStudentGoalRepository.Add(statusHistoty);

                await _studentGoalAggregateRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                await _statusStudentGoalRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = result;

                return methodResult;
            });

            return methodResult;
        }
    }
}
