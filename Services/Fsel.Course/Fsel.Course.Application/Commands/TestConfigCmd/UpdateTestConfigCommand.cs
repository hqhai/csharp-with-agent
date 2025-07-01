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
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.TestConfig;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateTestConfigCommand : UpdateTestConfigCommandModel, IRequest<MethodResult<TestConfigModel>>
    {
    }

    public class UpdateTestConfigCommandHandler : IRequestHandler<UpdateTestConfigCommand, MethodResult<TestConfigModel>>
    {
        private readonly IMapper _mapper;
        private readonly ITestConfigRepository _testConfigRepository;
        private readonly ITestConfigSectionRepository _testConfigSectionRepository;
        private readonly ITestConfigSectionQuestionRepository _testConfigSectionQuestionRepository;
        private readonly IQuestionRepository _questionRepository;

        public UpdateTestConfigCommandHandler(IMapper mapper
            , ITestConfigRepository testConfigRepository
            , ITestConfigSectionRepository testConfigSectionRepository
            , ITestConfigSectionQuestionRepository testConfigSectionQuestionRepository
            , IQuestionRepository questionRepository)

        {
            _mapper = mapper;
            _testConfigRepository = testConfigRepository;
            _testConfigSectionRepository = testConfigSectionRepository;
            _testConfigSectionQuestionRepository = testConfigSectionQuestionRepository;
            _questionRepository = questionRepository;
        }

        public async Task<MethodResult<TestConfigModel>> Handle(UpdateTestConfigCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<TestConfigModel>();

            var testConfig = await _testConfigRepository.GetIncludeByIdAsync(request.Id);
            if (testConfig == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }

            // Validate
            var testNameRegex = new Regex(@"^[a-zA-Z0-9_ ]{1,150}$");
            var testCodeRegex = new Regex(@"^[a-zA-Z0-9_]{1,150}$");

            if (string.IsNullOrEmpty(request.Name) || !testNameRegex.IsMatch(request.Name))
            {
                methodResult.AddErrorBadRequest(nameof(EnumCategoryErrorCode.NameNotValid), nameof(request.Name), request.Name);
                return methodResult;
            }

            if (string.IsNullOrEmpty(request.Code) || !testCodeRegex.IsMatch(request.Code))
            {
                methodResult.AddErrorBadRequest(nameof(EnumCategoryErrorCode.CodeNotValid), nameof(request.Code), request.Code);
                return methodResult;
            }

            var codeExists = await _testConfigRepository.Queryable
                .AnyAsync(x => x.Code == request.Code.Trim() && x.Id != request.Id, cancellationToken);

            if (codeExists)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Code), request.Code);
                return methodResult;
            }

            _mapper.Map(request, testConfig);
            if (!testConfig.IsValid())
            {
                methodResult.AddErrorBadRequest(testConfig.ErrorMessages);
                return methodResult;
            }

            // Bắt đầu transaction
            await _testConfigRepository.ExecuteTransactionAsync(async () =>
            {
                // Cập nhật TestConfig
                _testConfigRepository.Update(testConfig);

                // Lấy tất cả sections hiện tại
                var existingSections = await _testConfigSectionRepository
                    .Queryable
                    .Where(x => x.TestConfigId == testConfig.Id)
                    .ToListAsync(cancellationToken);

                // Xử lý sync section
                var incomingSections = request.TestConfigSections ?? new List<TestConfigSectionModel>();
                await SyncSections(incomingSections, existingSections, testConfig.Id, null, cancellationToken);

                await _testConfigRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<TestConfigModel>(testConfig);
                return methodResult;
            });
            return methodResult;
        }

        private async Task SyncSections(
            IList<TestConfigSectionModel> incoming,
            List<TestConfigSection> existing,
            Guid testConfigId,
            Guid? parentId,
            CancellationToken cancellationToken)
        {
            foreach (var sectionModel in incoming)
            {
                var existingSection = existing.FirstOrDefault(x => x.Id == sectionModel.Id);

                if (existingSection == null)
                {
                    // Thêm mới section
                    var newSection = new TestConfigSection
                    {
                        Id = Guid.NewGuid(),
                        Name = sectionModel.Name,
                        TestConfigId = testConfigId,
                        ParentId = parentId,
                        DisplayOrder = sectionModel.DisplayOrder,
                        LayoutType = sectionModel.LayoutType,
                        TargetWord = sectionModel.TargetWord,
                        ExecutionTime = sectionModel.ExecutionTime,
                        TotalScore = sectionModel.TotalScore,
                        SkillId = sectionModel.SkillId,
                        ConfigStr = sectionModel.ConfigStr
                    };

                    _testConfigSectionRepository.Add(newSection);

                    // Thêm mới: chỉ cần truyền newSection.Id là đủ
                    await SyncQuestions(sectionModel.Questions, newSection.Id, cancellationToken);

                    // Đệ quy cho các section con
                    await SyncSections(
                        sectionModel.Children ?? new List<TestConfigSectionModel>(),
                        existing,
                        testConfigId,
                        newSection.Id,
                        cancellationToken
                    );
                }
                else
                {
                    // Cập nhật section
                    existingSection.Name = sectionModel.Name;
                    existingSection.DisplayOrder = sectionModel.DisplayOrder;
                    existingSection.LayoutType = sectionModel.LayoutType;
                    existingSection.TargetWord = sectionModel.TargetWord;
                    existingSection.ExecutionTime = sectionModel.ExecutionTime;
                    existingSection.TotalScore = sectionModel.TotalScore;
                    existingSection.SkillId = sectionModel.SkillId;
                    existingSection.ConfigStr = sectionModel.ConfigStr;
                    existingSection.ParentId = parentId;

                    _testConfigSectionRepository.Update(existingSection);

                    // Cập nhật câu hỏi trong bảng trung gian
                    await SyncQuestions(sectionModel.Questions, existingSection.Id, cancellationToken);

                    // Đệ quy cho các section con
                    await SyncSections(
                        sectionModel.Children ?? new List<TestConfigSectionModel>(),
                        existing,
                        testConfigId,
                        existingSection.Id,
                        cancellationToken
                    );
                }
            }

            // Tìm section bị xóa (so với cha hiện tại)
            var incomingIds = incoming.Select(x => x.Id).Where(id => id != Guid.Empty).ToHashSet();
            var toDelete = existing
                .Where(x => x.ParentId == parentId && !incomingIds.Contains(x.Id))
                .ToList();

            if (toDelete.Any())
            {
                await _testConfigSectionRepository.DeleteListAsync(toDelete);
            }
        }

        private async Task SyncQuestions(
            IList<QuestionModel>? incomingQuestions,
            Guid sectionId,
            CancellationToken cancellationToken)
        {
            if (incomingQuestions == null)
            {
                return;
            }

            // 1. Lấy các SectionQuestion
            var existingLinks = await _testConfigSectionQuestionRepository
                .Queryable
                .Where(x => x.TestConfigSectionId == sectionId)
                .ToListAsync(cancellationToken);

            var incomingQuestionIds = incomingQuestions
                .Where(x => x.Id != Guid.Empty)
                .Select(x => x.Id)
                .ToHashSet();

            foreach (var qModel in incomingQuestions)
            {
                Question? question;
                if (qModel.Id == Guid.Empty)
                {
                    // 2.1 Thêm mới SectionQuestion
                    var newQuestion = _mapper.Map<Question>(qModel);
                    var questionResult = _questionRepository.Add(newQuestion);
                    var newSectionQuestion = new TestConfigSectionQuestion
                    {
                        Id = Guid.NewGuid(),
                        TestConfigSectionId = sectionId,
                        QuestionId = questionResult.Id,
                    };
                    _testConfigSectionQuestionRepository.Add(newSectionQuestion);
                }
                else
                {
                    question = await _questionRepository.GetByIdAsync(qModel.Id);
                    if (question != null)
                    {
                        _mapper.Map(qModel, question);
                        _questionRepository.Update(question);
                    }
                }
            }

            // 3. Xóa các liên kết không còn trong danh sách mới
            var toRemove = existingLinks
                .Where(link => !incomingQuestionIds.Contains(link.QuestionId))
                .ToList();

            if (toRemove.Any())
            {
                await _testConfigSectionQuestionRepository.DeleteListAsync(toRemove);
            }
        }
    }
}
