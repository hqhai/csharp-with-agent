// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.PlacementTestCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Domain.Entities.TestConfigs;
    using Domain.Enums;
    using Common.ActionResults;
    using Common.Enums.ErrorCodes;
    using Core.Base;
    using Domain.Models.EntityModels.PlacementTestModels;
    using Services.ApplicationServices;
    using Services.ApplicationServices.Aggregates;
    using Services.UserServices;
    using Shared.Enums;
    using Shared.Enums.ErrorCodes;
    using Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Nest;

    public class ChosePtFlowCommand : MediatR.IRequest<MethodResult<PtStateModel>>
    {
        public ChosePtFlowCommand(Guid projectId)
        {
            ProjectId = projectId;
        }

        public Guid ProjectId { get; set; }
    }

    public class ChosePtFlowCommandHandler : IRequestHandler<ChosePtFlowCommand, MethodResult<PtStateModel>>
    {
        private readonly IUserService _userService;
        private readonly IServiceProvider _serviceProvider;
        private readonly AuthContext _authContext;
        private readonly IFlowService _flowService;
        private readonly ITestService _testService;
        private readonly Core.Base.Interfaces.IRepository<TestGroupResult> _testGroupResult;
        private readonly ICategoryService _categoryService;

        public ChosePtFlowCommandHandler(AuthContext authContext,
            IFlowService flowService,
            ITestService testService,
            IUserService userService, Core.Base.Interfaces.IRepository<TestGroupResult> testGroupResult,
            ICategoryService categoryService,
            IServiceProvider serviceProvider)
        {
            _authContext = authContext;
            _flowService = flowService;
            _testService = testService;
            _userService = userService;
            _categoryService = categoryService;
            _serviceProvider = serviceProvider;
            _testGroupResult = testGroupResult;
        }

        public async Task<MethodResult<PtStateModel>> Handle(ChosePtFlowCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PtStateModel>();
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

            if (student.User == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student.User));
                return methodResult;
            }

            var isExistPt = await _testGroupResult.Queryable.AnyAsync(x => x.StudentId == student.Id && x.TestType == EnumTestType.PlacementTest, cancellationToken);
            if (isExistPt)
            {
                methodResult.AddErrorBadRequest("PT is started or completed");
                return methodResult;
            }

            var programContainPtFound = await _categoryService.GetProgramContainPtBySelectedProject(request.ProjectId, cancellationToken);
            if (programContainPtFound != null)
            {
                if (programContainPtFound.TestMode is EnumTestMode.Custom)
                {
                    var age = DateTimeHelper.GetYearOld(student.User.Birthday);
                    var flowMatch = await _flowService.GetHierarchicalFlowByCondition(x => x.ProgramId == programContainPtFound.Id
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

                    var testGroupResult = await _testService.InitTestGroupResultForFlow(flowMatch.Id, request.ProjectId, student.Id, EnumTestType.PlacementTest);

                    var aggregate = new FlowTestResultAggregate(testGroupResult, _serviceProvider);
                    await aggregate.Start();
                    methodResult.Result = await aggregate.ExpotStateData();
                }
                else if (programContainPtFound.TestMode == EnumTestMode.Not)
                {
                    var testGroupResult = await _testService.InitTestGroupResultForFlow(null, request.ProjectId, student.Id, EnumTestType.PlacementTest, isByPass: true);
                    methodResult.Result = new PtStateModel { Status = EnumResultStatus.ByPass, TestGroupResultId = testGroupResult.Id, StudentId = student.Id };
                }
            }
            else
            {
                methodResult.AddErrorBadRequest("Not found program contain PT");
            }

            return methodResult;
        }
    }
}
