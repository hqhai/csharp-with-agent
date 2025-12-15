// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.TestCmd
{
    using Common.ActionResults;
    using Common.Enums.ErrorCodes;
    using Core.Base;
    using Core.Base.Interfaces;
    using Domain.Entities.TestConfigs;
    using Domain.Enums;
    using Domain.Models.EntityModels.TestModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Services.ApplicationServices;
    using Services.ApplicationServices.Aggregates;
    using Services.UserServices;
    using Shared.Enums.ErrorCodes;
    using TestStateModel = Domain.Models.EntityModels.PlacementTestModels.TestStateModel;


    public class ChoseTestCommand : IRequest<MethodResult<SingleTestStateModel>>
    {
        public Guid TestResultId { get; set; }
        public Guid ProjectId { get; set; }
    }

    public class ChoseTestCommandHandler : IRequestHandler<ChoseTestCommand, MethodResult<SingleTestStateModel>>
    {
        private readonly IUserService _userService;
        private readonly IServiceProvider _serviceProvider;
        private readonly AuthContext _authContext;
        private readonly ITestService _testService;
        private readonly IRepository<TestGroupResult> _testGroupResult;
        private readonly ICategoryService _categoryService;
        private IRepository<TestResult> _testResult;

        public ChoseTestCommandHandler(AuthContext authContext,
            ITestService testService,
            IUserService userService,
            IRepository<TestGroupResult> testGroupResult,
            IRepository<TestResult> testResult,
            ICategoryService categoryService,
            IServiceProvider serviceProvider)
        {
            _authContext = authContext;
            _testService = testService;
            _userService = userService;
            _testResult = testResult;
            _categoryService = categoryService;
            _serviceProvider = serviceProvider;
            _testGroupResult = testGroupResult;
        }

        public async Task<MethodResult<SingleTestStateModel>> Handle(ChoseTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<SingleTestStateModel>();
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

            var testResult = await _testResult.ReadQueryable.Include(x => x.TestGroupResult)
                .FirstOrDefaultAsync(x => x.Id == request.TestResultId, cancellationToken);

            if (testResult.Status == EnumResultStatus.New)
            {
                var programContainPtFound = await _categoryService.GetProgramContainPtBySelectedProject(request.ProjectId, cancellationToken);
                if (programContainPtFound != null)
                {
                    var testGroupResult = await _testService.InitTestGroupResult(programContainPtFound.Id, student.Id, EnumTestType.Test);

                    var aggregate = new TestResultAggregate(testGroupResult, _serviceProvider, testResult);
                    await aggregate.Start();
                    methodResult.Result = await aggregate.ExpotStateData();

                    return methodResult;
                }
                else
                {
                    var testGroupResult = await _testService.InitTestGroupResult(request.ProjectId, student.Id, EnumTestType.Test);
                    methodResult.Result = new SingleTestStateModel{ StudentId = student.Id, Status = EnumResultStatus.ByPass,  TestGroupResultId = testGroupResult.Id };
                    return methodResult;
                }
            }
            else
            {
                var testGroupResult = await _testGroupResult.ReadQueryable.FirstOrDefaultAsync(x => x.Id == testResult.TestGroupResultId, cancellationToken);

                var aggregate = new TestResultAggregate(testGroupResult, _serviceProvider, testResult);
                await aggregate.InitAggregate();

                if (testResult.Status != EnumResultStatus.Done && testResult.Status != EnumResultStatus.ByPass)
                {
                    await aggregate.Start();
                }
                return new MethodResult<SingleTestStateModel> { Result = await aggregate.ExpotStateData() };
            }
        }
    }
}
