// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.TestCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Tests;
    using Fsel.Course.Domain.Models.EntityModels.TestModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Infrastructure.Common.TestHelper;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateTestCommand : UpdateTestCommandModel, IRequest<MethodResult<TestModel>>
    {
    }

    public class CreateTestConfigCommandHandler : IRequestHandler<CreateTestCommand, MethodResult<TestModel>>
    {
        private readonly IMapper _mapper;
        private readonly ITestRepository _testRepository;
        private readonly QuestionConverter _questionConverter;
        private readonly IServiceProvider _serviceProvider;
        private readonly TestConverter _testConverter;

        public CreateTestConfigCommandHandler(IMapper mapper,
            ITestRepository testRepository,
            QuestionConverter questionConverter,
            IServiceProvider serviceProvider,
            TestConverter testConverter)
        {
            _mapper = mapper;
            _testRepository = testRepository;
            _questionConverter = questionConverter;
            _serviceProvider = serviceProvider;
            _testConverter = testConverter;
        }

        public async Task<MethodResult<TestModel>> Handle(CreateTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<TestModel> methodResult = new MethodResult<TestModel>();

            var test = TestFactory.Create(request, _mapper, _questionConverter).Build();
            if (await test.ValidateDuplicateTest(_testRepository).ConfigureAwait(false))
            {
                methodResult.AddErrorBadRequest(test.ErrorMessages);
                return methodResult;
            }
            var method = _testConverter.IsValidateQuestion(request.TestSections);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }
            if (!await test.IsValid(_serviceProvider))
            {
                methodResult.AddErrorBadRequest(test.ErrorMessages);
                return methodResult;
            }

            await _testRepository.ExecuteTransactionAsync(async () =>
            {
                test = _testRepository.Add(test);
                await _testRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<TestModel>(test);
                return methodResult;
            });
            return methodResult;
        }
    }
}
