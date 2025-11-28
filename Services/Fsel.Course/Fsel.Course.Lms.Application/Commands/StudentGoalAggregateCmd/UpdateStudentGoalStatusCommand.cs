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
    using Services.UserServices;
    using Services.UserServices.Models;
    using Shared.Enums;

    public class UpdateStudentGoalStatusCommand : IRequest<MethodResult<StausStudentGoalHistoryModel>>
    {
        public Guid StudentId { get; set; }
        public EnumStatusStudentCampus StatusStudentGoal { get; set; }
    }

    public class UpdateStudentGoalStatusCommandHandler : IRequestHandler<UpdateStudentGoalStatusCommand, MethodResult<StausStudentGoalHistoryModel>>
    {
        private readonly IStudentGoalAggregateRepository  _studentGoalAggregateRepository;
        private readonly IStatusStudentGoalRepository _statusStudentGoalRepository;
        private readonly IUserService  _userService;
        private readonly IMapper _mapper;

        public UpdateStudentGoalStatusCommandHandler(IStudentGoalAggregateRepository studentGoalAggregateRepository,
            IStatusStudentGoalRepository statusStudentGoalRepository,
            IMapper mapper,
            IUserService userService)
        {
            _studentGoalAggregateRepository = studentGoalAggregateRepository;
            _statusStudentGoalRepository = statusStudentGoalRepository;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<MethodResult<StausStudentGoalHistoryModel>> Handle(UpdateStudentGoalStatusCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<StausStudentGoalHistoryModel>();

            // vết hàm update status cho student
            var student = await _userService.UpdateStatusStudentCampus(request.StudentId, new UpdateStatusStudentMode{ StatusStudentGoal =  request.StatusStudentGoal});

            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vnNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);

            var statusHistoty = new StatusStudentGoalHistory();
            statusHistoty.StatusStudentGoal = request.StatusStudentGoal;
            statusHistoty.StudentId = student.Content!.Result!.Id;
            statusHistoty.CreatedDate = vnNow;

            await _statusStudentGoalRepository.ExecuteTransactionAsync(async () =>
            {
                statusHistoty = _statusStudentGoalRepository.Add(statusHistoty);

                await _studentGoalAggregateRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                await _statusStudentGoalRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                var result = new StausStudentGoalHistoryModel();
                result.StatusStudentGoal = statusHistoty.StatusStudentGoal;
                result.StudentId = statusHistoty.StudentId;

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = result;

                return methodResult;
            });

            return methodResult;
        }
    }
}
