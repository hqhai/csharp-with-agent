// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.PlacementTestCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Models.EntityModels.FlowModels;
    using Fsel.Course.Lms.Application.Services.ApplicationServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;

    public class ChosePTFlowCommand : IRequest<MethodResult<PTFlowModel>>
    {
        public ChosePTFlowCommand(Guid programId)
        {
            ProgramId = programId;
        }

        public Guid ProgramId { get; set; }
    }

    public class ChosePTFlowCommandHandler : IRequestHandler<ChosePTFlowCommand, MethodResult<PTFlowModel>>
    {
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly IFlowService _flowService;
        private readonly ITestService _testService;

        public ChosePTFlowCommandHandler(AuthContext authContext, IFlowService flowService, ITestService testService, IUserService userService)
        {
            _authContext = authContext;
            _flowService = flowService;
            _testService = testService;
            _userService = userService;
        }

        public async Task<MethodResult<PTFlowModel>> Handle(ChosePTFlowCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PTFlowModel>();
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

            var testGroupResult = await _testService.InitTestGroupResultForFlow(flowMatch.Id, student.Id, Domain.Enums.EnumTestType.PlacementTest);

            await _testService.InitTestForStepFlow(student.Id, firstStepFlow.Id, testGroupResult.Id, request.ProgramId);

            var flowBranches = FlowService.GetAllFlowBranchesByFlow(flowMatch.StepFlows.First());

            var numberOfModules = flowBranches.FirstOrDefault().Count();

            methodResult.Result = new PTFlowModel
            {
                Name = flowMatch.Name,
                FlowId = flowMatch.Id,
                TestGroupResultId = flowMatch.Id,
                NumberOfModules = numberOfModules,
            };
            return methodResult;
        }
    }
}
