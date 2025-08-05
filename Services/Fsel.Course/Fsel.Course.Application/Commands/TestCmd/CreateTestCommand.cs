// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.TestCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Tests;
    using Fsel.Course.Domain.Models.EntityModels.TestModels;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateTestCommand : CreateTestCommandModel, IRequest<MethodResult<TestModel>>
    {
    }

    public class CreateTestConfigCommandHandler : IRequestHandler<CreateTestCommand, MethodResult<TestModel>>
    {
        private readonly IMapper _mapper;
        private readonly ITestRepository _testRepository;
        private readonly TestHelper _testHelper;

        public CreateTestConfigCommandHandler(IMapper mapper
            , ITestRepository testRepository
            , TestHelper testHelper)
        {
            _mapper = mapper;
            _testRepository = testRepository;
            _testHelper = testHelper;
        }

        public async Task<MethodResult<TestModel>> Handle(CreateTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<TestModel> methodResult = new MethodResult<TestModel>();

            var checkCode = await _testRepository.Queryable.AnyAsync(x => x.Code == request.Code, cancellationToken);
            if (checkCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Code), request.Code);
                return methodResult;
            }
<<<<<<< Updated upstream
            var test = _mapper.Map<Test>(request);
            if (!test.IsValid())
=======
            var method = _testConverter.IsValidateQuestion(request.TestSections);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }
            if (!await test.IsValid(_serviceProvider))
>>>>>>> Stashed changes
            {
                methodResult.AddErrorBadRequest(test.ErrorMessages);
                return methodResult;
            }

            if (request.TestSections.Any())
            {
                var methodResultCreated = _testHelper.InsertSectionRecursive(request.TestSections, test: test);
                if (!methodResultCreated.IsOK)
                {
                    methodResult.AddErrorBadRequest(methodResultCreated.ErrorMessages);
                    return methodResult;
                }
            }

            await _testRepository.ExecuteTransactionAsync(async () =>
            {
                test = _testRepository.Add(test);
                await _testRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<TestModel>(test);
                return methodResult;
            });
            return methodResult;
        }
    }
}
