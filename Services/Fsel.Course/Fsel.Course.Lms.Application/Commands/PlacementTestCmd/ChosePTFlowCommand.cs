// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.PlacementTestCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Models.EntityModels.PlacementTestModels;
    using Fsel.Course.Lms.Application.Services.ApplicationServices;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.Aggregates;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;

    public class ChosePTFlowCommand : IRequest<MethodResult<PTStateModel>>
    {
        public ChosePTFlowCommand(Guid programId)
        {
            ProgramId = programId;
        }

        public Guid ProgramId { get; set; }
    }

    public class ChosePTFlowCommandHandler : IRequestHandler<ChosePTFlowCommand, MethodResult<PTStateModel>>
    {
        private readonly IUserService _userService;
        private readonly IServiceProvider _serviceProvider;
        private readonly AuthContext _authContext;
        private readonly IFlowService _flowService;
        private readonly ITestService _testService;

        public ChosePTFlowCommandHandler(AuthContext authContext, IFlowService flowService, ITestService testService, IUserService userService, IServiceProvider serviceProvider)
        {
            _authContext = authContext;
            _flowService = flowService;
            _testService = testService;
            _userService = userService;
            _serviceProvider = serviceProvider;
        }

        public async Task<MethodResult<PTStateModel>> Handle(ChosePTFlowCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PTStateModel>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            if (student.Human == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student.Human));
                return methodResult;
            }

            var age = DateTimeHelper.GetYearOld(student.Human.Birthday);

            var flowMatch = await _flowService.GetHierarchicalFlowByCondition(x => x.ProgramId == request.ProgramId
            && x.Status == EnumStatus.Active
            && x.FromAge <= age && x.ToAge >= age);

            if (flowMatch?.StepFlows.FirstOrDefault() == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(flowMatch));
                return methodResult;
            }

            var firstStepFlow = flowMatch.StepFlows?.FirstOrDefault();
            if (firstStepFlow == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(firstStepFlow));
                return methodResult;
            }

            var testGroupResult = await _testService.InitTestGroupResultForFlow(flowMatch.Id, request.ProgramId, student.Id, Domain.Enums.EnumTestType.PlacementTest);

            var aggregate = new FlowTestResultAggregate(testGroupResult, _serviceProvider);
            await aggregate.Start();
            methodResult.Result = await aggregate.ExpotStateData();

            return methodResult;
        }
    }
}
