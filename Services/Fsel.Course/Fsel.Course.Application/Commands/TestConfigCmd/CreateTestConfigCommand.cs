// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.TestConfigCmd
{
    using System.Text.RegularExpressions;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.TestConfig;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.TestConfigs;
    using Fsel.Course.Domain.Models.CommandModels.TestConfigSections;
    using Fsel.Course.Domain.Models.EntityModels.TestConfig;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateTestConfigCommand : CreateTestConfigCommandModel, IRequest<MethodResult<TestConfigModel>>
    {
    }

    public class CreateTestConfigCommandHandler : IRequestHandler<CreateTestConfigCommand, MethodResult<TestConfigModel>>
    {
        private readonly IMapper _mapper;
        private readonly ITestConfigRepository _testConfigRepository;
        private readonly ITestConfigSectionRepository _testConfigSectionRepository;
        private readonly ITestConfigSectionQuestionRepository _testConfigSectionQuestionRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly QuestionConverter _questionConverter;

        public CreateTestConfigCommandHandler(IMapper mapper
            , ITestConfigRepository testConfigRepository
            , ITestConfigSectionRepository testConfigSectionRepository
            , ITestConfigSectionQuestionRepository testConfigSectionQuestionRepository
            , IQuestionRepository questionRepository
            , QuestionConverter questionConverter
            )
        {
            _mapper = mapper;
            _testConfigRepository = testConfigRepository;
            _testConfigSectionRepository = testConfigSectionRepository;
            _testConfigSectionQuestionRepository = testConfigSectionQuestionRepository;
            _questionRepository = questionRepository;
            _questionConverter = questionConverter;
        }

        public async Task<MethodResult<TestConfigModel>> Handle(CreateTestConfigCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<TestConfigModel> methodResult = new MethodResult<TestConfigModel>();
            var testNameRegex = new Regex(@"^[a-zA-Z0-9_ ]{1,150}$");
            var testCodeRegex = new Regex(@"^[a-zA-Z0-9_]{1,150}$");
            if (string.IsNullOrEmpty(request.Name) || (!string.IsNullOrEmpty(request.Name) && !testNameRegex.IsMatch(request.Name)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumCategoryErrorCode.NameNotValid), nameof(request.Name), request.Name);
                return methodResult;
            }

            if (string.IsNullOrEmpty(request.Code) || (!string.IsNullOrEmpty(request.Code) && !testCodeRegex.IsMatch(request.Code)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumCategoryErrorCode.CodeNotValid), nameof(request.Code), request.Code);
                return methodResult;
            }
            var checkCode = await _testConfigRepository.Queryable.AnyAsync(x => x.Code == request.Code.Trim(), cancellationToken);
            if (checkCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Code), request.Code);
                return methodResult;
            }
            var testConfig = _mapper.Map<TestConfig>(request);

            if (!testConfig.IsValid())
            {
                methodResult.AddErrorBadRequest(testConfig.ErrorMessages);
                return methodResult;
            }
            await _testConfigRepository.ExecuteTransactionAsync(async () =>
            {
                // Step 1: Insert TestConfig
                var createdTestConfig = _testConfigRepository.Add(testConfig);
                await _testConfigRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

                // Step 2: Insert Sections if any
                if (request.TestConfigSections?.Any() == true)
                {
                    foreach (var section in request.TestConfigSections)
                    {
                        var sectionResult = await InsertSectionRecursiveAsync(section, createdTestConfig.Id, null, cancellationToken);
                        if (!sectionResult.IsOK)
                        {
                            methodResult.AddErrorBadRequest(sectionResult.ErrorMessages);
                            return methodResult;
                        }
                    }
                }

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<TestConfigModel>(createdTestConfig);
                return methodResult;
            });
            return methodResult;
        }

        private async Task<VoidMethodResult> InsertSectionRecursiveAsync(CreateTestConfigSectionCommandModel dto, Guid testConfigId, Guid? parentId, CancellationToken cancellationToken)
        {
            VoidMethodResult methodResult = new VoidMethodResult();

            var entity = new TestConfigSection
            {
                Name = dto.Name,
                DisplayOrder = dto.DisplayOrder,
                TargetWord = dto.TargetWord,
                TestConfigId = testConfigId,
                ParentId = parentId,
                ConfigStr = dto.ConfigStr,
                LayoutType = dto.LayoutType,
                TotalScore = dto.TotalScore,
                SkillId = dto.SkillId,
            };

            var testConfigSectionResult = _testConfigSectionRepository.Add(entity);
            await _testConfigSectionRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

            if (testConfigSectionResult.Id != Guid.Empty)
            {
                // Insert questions
                if (dto.Questions?.Any() == true)
                {
                    foreach (var question in dto.Questions)
                    {
                        var newQuestion = _mapper.Map<Question>(question);
                        var method = _questionConverter.HandleQuestion(newQuestion);
                        if (!method.IsOK)
                        {
                            methodResult.AddErrorBadRequest(method.ErrorMessages);
                            return methodResult;
                        }
                        if (method.Result != null)
                        {
                            var qResult = _questionRepository.Add(method.Result);
                            _testConfigSectionQuestionRepository.Add(new TestConfigSectionQuestion
                            {
                                TestConfigSectionId = testConfigSectionResult.Id,
                                QuestionId = qResult.Id,
                            });
                        }

                    }
                }

                // Insert children with correct ParentId
                if (dto.Childrens?.Any() == true)
                {
                    foreach (var child in dto.Childrens)
                    {
                        await InsertSectionRecursiveAsync(child, testConfigId, testConfigSectionResult.Id, cancellationToken);
                    }
                }
            }
            return methodResult;
        }
    }
}
