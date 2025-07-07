// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.TestCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Questions;
    using Fsel.Course.Domain.Models.CommandModels.Tests;
    using Fsel.Course.Domain.Models.CommandModels.TestSections;
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
        private readonly QuestionConverter _questionConverter;

        public CreateTestConfigCommandHandler(IMapper mapper
            , ITestRepository testRepository
            , QuestionConverter questionConverter
            )
        {
            _mapper = mapper;
            _testRepository = testRepository;
            _questionConverter = questionConverter;
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
            var test = _mapper.Map<Test>(request);
            if (!test.IsValid())
            {
                methodResult.AddErrorBadRequest(test.ErrorMessages);
                return methodResult;
            }

            if (request.TestSections.Any())
            {
                var methodResultCreated = InsertSectionRecursive(request.TestSections, test: test);
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

        private VoidMethodResult InsertSectionRecursive(IList<CreateTestSectionCommandModel>? testSectionRequests, TestSection? testSection = null, Test? test = null)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            if (testSectionRequests == null || !testSectionRequests.Any())
            {
                return methodResult;
            }

            foreach (var testSectionRequest in testSectionRequests)
            {
                var testSectionCreated = _mapper.Map<TestSection>(testSectionRequest);
                if (!testSectionCreated.IsValid())
                {
                    methodResult.AddErrorBadRequest(testSectionCreated.ErrorMessages);
                    return methodResult;
                }
                testSectionCreated.DisplayOrder = testSectionRequests.IndexOf(testSectionRequest);

                if (testSectionRequest.Childrens.Any())
                {
                    var methodResultCreated = InsertSectionRecursive(testSectionRequest.Childrens, testSectionCreated);
                    if (!methodResultCreated.IsOK)
                    {
                        methodResult.AddErrorBadRequest(methodResultCreated.ErrorMessages);
                        return methodResult;
                    }
                }
                if (testSectionRequest.Questions.Any())
                {
                    var methodResultCreated = InsertQuestions(testSectionRequest.Questions, testSectionCreated);
                    if (!methodResultCreated.IsOK)
                    {
                        methodResult.AddErrorBadRequest(methodResultCreated.ErrorMessages);
                        return methodResult;
                    }
                }
                if (testSection != null)
                {
                    testSection.TestSections.Add(testSectionCreated);
                }
                if (test != null)
                {
                    test.TestSections.Add(testSectionCreated);
                }
            }
            return methodResult;
        }

        private VoidMethodResult InsertQuestions(IList<CreateQuestionCommandModel>? questionRequests, TestSection testSection)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            if (questionRequests == null || !questionRequests.Any())
            {
                return methodResult;
            }

            foreach (var questionRequest in questionRequests)
            {
                if (questionRequest == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questionRequest));
                    return methodResult;
                }
                var question = _mapper.Map<Question>(questionRequest);
                var method = _questionConverter.HandleQuestion(question);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
                testSection.TestSectionQuestions.Add(new TestSectionQuestion
                {
                    Question = method.Result ?? question,
                });
            }
            return methodResult;
        }
    }
}
