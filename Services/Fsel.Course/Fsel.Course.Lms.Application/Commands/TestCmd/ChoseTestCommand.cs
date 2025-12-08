// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.TestCmd
{
    using Common.ActionResults;
    using Common.Enums.ErrorCodes;
    using Core.Base;
    using Domain.Entities.TestConfigs;
    using Domain.Enums;
    using Domain.Models.EntityModels.PlacementTestModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Services.ApplicationServices;
    using Services.ApplicationServices.Aggregates;
    using Services.UserServices;
    using Shared.Enums.ErrorCodes;


    public class ChoseTestCommand : IRequest<MethodResult<TestStateModel>>
    {
        public ChoseTestCommand(Guid testResultId)
        {
            TestResultId = testResultId;
        }

        public Guid TestResultId { get; set; }
    }

    public class ChoseTestCommandHandler : IRequestHandler<ChoseTestCommand, MethodResult<TestStateModel>>
    {
        private readonly IUserService _userService;
        private readonly IServiceProvider _serviceProvider;
        private readonly AuthContext _authContext;
        private readonly ITestService _testService;
        private readonly Core.Base.Interfaces.IRepository<TestGroupResult> _testGroupResult;
        private readonly ICategoryService _categoryService;

        public ChoseTestCommandHandler(AuthContext authContext,
            ITestService testService,
            IUserService userService,
            Core.Base.Interfaces.IRepository<TestGroupResult> testGroupResult,
            ICategoryService categoryService,
            IServiceProvider serviceProvider)
        {
            _authContext = authContext;
            _testService = testService;
            _userService = userService;
            _categoryService = categoryService;
            _serviceProvider = serviceProvider;
            _testGroupResult = testGroupResult;
        }

        public async Task<MethodResult<TestStateModel>> Handle(ChoseTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<TestStateModel>();
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

            var testResult = await _testService.LoadHierachicalTestResult(x => x.Id == request.TestResultId);
            var testId = testResult.TestId;
            var test = await _testService.GetHierachicalTestById((Guid)testId!);
            // var programContainPtFound = await _categoryService.GetProgramContainPtBySelectedProject(test.ProgramId, cancellationToken);
            // if (programContainPtFound == null)
            // {
            //     return methodResult;
            // }

            var aggregate = new TestResultAggregate(testResult, _serviceProvider);
            await aggregate.Start();
            methodResult.Result = await aggregate.ExpotStateData();
            return methodResult;
        }
    }
}
