// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.TestCmd
{
    using Common.ActionResults;
    using Common.Enums.ErrorCodes;
    using Core.Base;
    using Domain.Enums;
    using Domain.IRepositories;
    using Domain.Models.EntityModels.TestModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Services.ApplicationServices;
    using Services.ApplicationServices.Aggregates;
    using Services.UserServices;
    using Shared.Enums.ErrorCodes;

    public class ChoseTestCommand : IRequest<MethodResult<SingleTestStateModel>>
    {
        public Guid TestResultId { get; set; }
    }

    public class ChoseTestCommandHandler : IRequestHandler<ChoseTestCommand, MethodResult<SingleTestStateModel>>
    {
        private readonly IUserService _userService;
        private readonly IServiceProvider _serviceProvider;
        private readonly AuthContext _authContext;
        private readonly ITestService _testService;
        private readonly ITestGroupResultRepository _testGroupResultRepository;
        private readonly ICategoryService _categoryService;
        private readonly ITestResultRepository _testResultRepository;

        public ChoseTestCommandHandler(AuthContext authContext,
            ITestService testService,
            IUserService userService,
            ITestGroupResultRepository testGroupResultRepository,
            ITestResultRepository testResultRepository,
            ICategoryService categoryService,
            IServiceProvider serviceProvider)
        {
            _authContext = authContext;
            _testService = testService;
            _userService = userService;
            _testGroupResultRepository = testGroupResultRepository;
            _categoryService = categoryService;
            _serviceProvider = serviceProvider;
            _testResultRepository = testResultRepository;
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

            if (student.User == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student.User));
                return methodResult;
            }

            var testResult = await _testResultRepository.Queryable.Include(x => x.TestGroupResult)
                                                        .Include(x => x.SectionResults)
                                                        .FirstOrDefaultAsync(x => x.Id == request.TestResultId, cancellationToken);

            if (testResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(testResult));
                return methodResult;
            }
            var testGroupResult = await _testGroupResultRepository.Queryable.Include(x => x.TestResults)
                                                                  .FirstOrDefaultAsync(x => x.Id == testResult.TestGroupResultId, cancellationToken);
            if (testGroupResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(testGroupResult));
                return methodResult;
            }

            if (testResult.Status == EnumResultStatus.New)
            {
                var aggregate = new TestResultAggregate(testGroupResult, _serviceProvider, testResult);
                await aggregate.StartTest();
                methodResult.Result = await aggregate.ExpotStateData();

                return methodResult;
            }
            else
            {
                var aggregate = new TestResultAggregate(testGroupResult, _serviceProvider, testResult);
                await aggregate.InitAggregate();

                return new MethodResult<SingleTestStateModel> { Result = await aggregate.ExpotStateData() };
            }
        }
    }
}
