// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.MockTestCmd.V1i1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.MockTestAnswers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateMockTestAnswerBySectionGroupCommand : CreateAnswerBySectionGroupCommandModel, IRequest<MethodResult<SectionGroupResultModel>>
    {
    }

    public class CreateMockTestAnswerBySectionGroupCommandHandler : IRequestHandler<CreateMockTestAnswerBySectionGroupCommand, MethodResult<SectionGroupResultModel>>
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly QuestionConverter _questionConverter;
        private readonly IMockTestAnswerRepository _mockTestAnswerRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly ISystemService _systemService;
        private readonly SectionGroupConverter _sectionGroupConverter;
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly ISectionTimeCodeRepository _sectionTimeCodeRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly IMapper _mapper;

        public CreateMockTestAnswerBySectionGroupCommandHandler(IQuestionRepository questionRepository
            , AuthContext authContext
            , IUserService userService
            , QuestionConverter questionConverter
            , IMockTestAnswerRepository mockTestAnswerRepository
            , IMockTestResultRepository mockTestResultRepository
            , ISectionRepository sectionRepository
            , ISystemService systemService
            , SectionGroupConverter sectionGroupConverter
            , ISectionGroupResultRepository sectionGroupResultRepository
            , ISectionTimeCodeRepository sectionTimeCodeRepository
            , ISectionGroupRepository sectionGroupRepository
            , IMapper mapper)
        {
            _questionRepository = questionRepository;
            _authContext = authContext;
            _userService = userService;
            _questionConverter = questionConverter;
            _mockTestAnswerRepository = mockTestAnswerRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _sectionRepository = sectionRepository;
            _systemService = systemService;
            _sectionGroupConverter = sectionGroupConverter;
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _sectionTimeCodeRepository = sectionTimeCodeRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<SectionGroupResultModel>> Handle(CreateMockTestAnswerBySectionGroupCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            #region Validate

            var methodResult = new MethodResult<SectionGroupResultModel>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            var studentId = student?.Id ?? request.StudentId ?? default;
            var mockTestAnswers = new List<MockTestAnswer>();
            var mockTestResult = await _mockTestResultRepository.Queryable.Include(x => x.MockTest).FirstOrDefaultAsync(x => x.Id == request.MockTestResultId, cancellationToken);
            if (mockTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTestResult));
                return methodResult;
            }
            else if (mockTestResult.Status == EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusDone), nameof(mockTestResult));
                return methodResult;
            }
            else if (mockTestResult.Status == EnumResultStatus.Unfinished)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusUnfinished));
                return methodResult;
            }
            var sectionGroup = await _sectionGroupRepository.GetByIdAsync(request.SectionGroupId);
            if (sectionGroup == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroup));
                return methodResult;
            }
            var sectionGroupResult = await _sectionGroupResultRepository.Queryable.Where(x => x.StudentId == studentId && x.SectionGroupId == request.SectionGroupId && x.MockTestResultId == mockTestResult.Id).FirstOrDefaultAsync(cancellationToken);
            if (sectionGroupResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroupResult));
                return methodResult;
            }
            else if (sectionGroupResult.Status == EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusDone), nameof(sectionGroupResult));
                return methodResult;
            }

            #endregion Validate

            await _mockTestAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                if (request.Answers != null && request.Answers.Any())
                {
                    var answerResult = await CreateAnswerAsync(request, sectionGroup, sectionGroupResult.Id);
                    if (!answerResult.IsOK)
                    {
                        methodResult.AddErrorBadRequest(answerResult.ErrorMessages);
                        return methodResult;
                    }
                }
                if (request.IsSubmit)
                {
                    await _sectionGroupConverter.UpdateMockTestAnswers(sectionGroup, sectionGroupResult);
                    sectionGroupResult = await _sectionGroupConverter.UpdateSectionGroupResultAsync(sectionGroupResult, sectionGroup, true, cancellationToken);
                }
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<SectionGroupResultModel>(sectionGroupResult);
                return methodResult;
            });

            await UpdateMockTestResultAsync(mockTestResult, cancellationToken);
            return methodResult;
        }

        private async Task UpdateMockTestResultAsync(MockTestResult mockTestResult, CancellationToken cancellationToken)
        {
            var mockTest = mockTestResult.MockTest;
            var numberOfDone = 4;
            if (mockTest == null || mockTestResult == null)
            {
                return;
            }
            var isSkillTest = mockTest.MockTestType == EnumMockTestType.SkillMockTest;
            if (isSkillTest)
            {
                var sectionGroupResult = await _sectionGroupResultRepository.Queryable.Where(s => s.MockTestResultId == mockTestResult.Id).FirstOrDefaultAsync(cancellationToken);
                if (sectionGroupResult != null && sectionGroupResult.Status == EnumResultStatus.Done)
                {
                    mockTestResult.WorkingTime = sectionGroupResult.WorkingTime;
                    mockTestResult.HighestStreak = sectionGroupResult.HighestStreak;
                    mockTestResult = await GetMockTestResult(sectionGroupResult.SkillScores, mockTestResult, isSkillTest);
                }
            }
            else
            {
                var sectionGroupResults = await _sectionGroupResultRepository.Queryable.Where(s => s.MockTestResultId == mockTestResult.Id).ToListAsync(cancellationToken);
                if (sectionGroupResults != null && sectionGroupResults.Count == numberOfDone && sectionGroupResults.All(x => x.Status == EnumResultStatus.Done))
                {
                    mockTestResult.WorkingTime = sectionGroupResults.Sum(x => x.WorkingTime);
                    mockTestResult.HighestStreak = sectionGroupResults.Max(x => x.HighestStreak);
                    mockTestResult = await GetMockTestResult(sectionGroupResults.SelectMany(x => x.SkillScores!).ToList(), mockTestResult, isSkillTest);
                }
            }

            _mockTestResultRepository.Update(mockTestResult);
            await _mockTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task<MockTestResult> GetMockTestResult(IList<SkillScores>? skillScores, MockTestResult mockTestResult, bool isSkillTest)
        {
            ArgumentNullException.ThrowIfNull(skillScores);
            mockTestResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
            mockTestResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
            mockTestResult.Status = EnumResultStatus.Done;
            mockTestResult.SkillScores = skillScores;
            mockTestResult = await GetTokenMockTestResult(mockTestResult, isSkillTest);
            return mockTestResult;
        }

        private static int GetToken(object? config)
        {
            var tokenConfig = config.Deserialize<TokenNumber>();
            return tokenConfig?.Number ?? default;
        }

        private async Task<MockTestResult> GetTokenMockTestResult(MockTestResult mockTestResult, bool isSkillTest)
        {
            var misstions = new List<string> { nameof(EnumTokenMission.HighestStreak), nameof(EnumTokenMission.TestDone), nameof(EnumTokenMission.SuperFire) };
            var tokenConfigResults = await _systemService.GetTokenConfigsAsync(new GetTokenConfigsQueryModel
            {
                Feature = isSkillTest ? EnumTokenFeature.SkillMockTest : EnumTokenFeature.FullMockTest,
                Missions = string.Join(",", misstions)
            });
            var isSuperFireModeResult = await _userService.CheckSuperFireModeAsync();
            if (!tokenConfigResults.IsSuccessStatusCode || !isSuperFireModeResult.IsSuccessStatusCode)
            {
                return mockTestResult;
            }
            var tokenConfigs = tokenConfigResults.Content?.Result;
            var isSuperFireMode = isSuperFireModeResult.Content?.Result ?? default;
            var configDone = tokenConfigs?.FirstOrDefault(x => x.Mission == EnumTokenMission.TestDone);
            var configHighestStreak = tokenConfigs?.FirstOrDefault(x => x.Mission == EnumTokenMission.HighestStreak);
            var configSuperFire = tokenConfigs?.FirstOrDefault(x => x.Mission == EnumTokenMission.SuperFire);

            mockTestResult.TokenDone = GetToken(isSuperFireMode ? configDone?.SuperConfig : configDone?.Config);
            mockTestResult.TokenHighestStreak = GetToken(isSuperFireMode ? configHighestStreak?.SuperConfig : configHighestStreak?.Config) * mockTestResult.HighestStreak ?? default;
            mockTestResult.TokenSuperFire = GetToken(isSuperFireMode ? configSuperFire?.SuperConfig : configSuperFire?.Config);
            return mockTestResult;
        }

        private async Task<MethodResult<IList<MockTestAnswer>>> CreateAnswerAsync(CreateMockTestAnswerBySectionGroupCommand request, SectionGroup sectionGroup, Guid sectionGroupResultId)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            var methodResult = new MethodResult<IList<MockTestAnswer>>();
            var anserResult = new MethodResult<IList<MockTestAnswer>>();
            if (sectionGroup.CourseSkill == EnumCourseSkill.Listening || sectionGroup.CourseSkill == EnumCourseSkill.Reading)
            {
                var questionIds = request.Answers.Where(x => x.QuestionId.HasValue).Select(x => x.QuestionId!.Value).ToList();
                var questions = await _questionRepository.GetIncludeSectionByIdAsync(questionIds);
                if (questions == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }
                anserResult = await CreateAnswer(request, questions, sectionGroupResultId);
            }
            else if (sectionGroup.CourseSkill == EnumCourseSkill.Writing)
            {
                var sectionIds = request.Answers.Where(x => x.SectionId.HasValue).Select(x => x.SectionId!.Value).ToList();
                var sections = await _sectionRepository.Queryable.Where(x => sectionIds.Contains(x.Id)).ToListAsync();
                if (sections == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }
                anserResult = await CreateAnswer(request, sections, sectionGroupResultId);
            }
            else
            {
                var sectionTimeCodeIds = request.Answers.Where(x => x.SectionTimeCodeId.HasValue).Select(x => x.SectionTimeCodeId!.Value).ToList();
                var sectionTimeCodes = await _sectionTimeCodeRepository.Queryable.Where(x => sectionTimeCodeIds.Contains(x.Id)).ToListAsync();
                if (sectionTimeCodes == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }
                anserResult = await CreateAnswer(request, sectionTimeCodes, sectionGroupResultId);
            }

            if (!anserResult.IsOK)
            {
                methodResult.AddErrorBadRequest(anserResult.ErrorMessages);
                return methodResult;
            }
            var mockTestAnswers = anserResult.Result?.ToList();
            if (mockTestAnswers != null && mockTestAnswers.Any())
            {
                await _mockTestAnswerRepository.AddList(mockTestAnswers);
                await _mockTestAnswerRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
            }
            methodResult.Result = anserResult.Result;
            return methodResult;
        }

        private async Task<MethodResult<IList<MockTestAnswer>>> CreateAnswer(CreateMockTestAnswerBySectionGroupCommand request, IList<Question>? questions, Guid sectionGroupResultId)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            ArgumentNullException.ThrowIfNull(questions);
            var methodResult = new MethodResult<IList<MockTestAnswer>>();
            var mockTestAnswers = new List<MockTestAnswer>();
            if (questions != null && questions.Any())
            {
                foreach (var item in request.Answers)
                {
                    var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                    var questionResult = _questionConverter.HandleQuestionAnswer(question, item.Answer, request.IsSubmit);
                    if (!questionResult.IsOK)
                    {
                        methodResult.AddErrorBadRequest(questionResult.ErrorMessages);
                        return methodResult;
                    }
                    var (questionItem, answerConfig, correctCount) = questionResult.Result;
                    var sectionQuestionId = questionItem.SectionQuestions.FirstOrDefault()?.Id ?? default;

                    var mockTestAnswer = await _mockTestAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.MockTestResultId == request.MockTestResultId && x.SectionQuestionId == request.SectionGroupId);
                    if (mockTestAnswer == null)
                    {
                        mockTestAnswers.Add(GetMockTestAnswer(answerConfig, correctCount, request, sectionGroupResultId, questionItem, default));
                    }
                }
            }
            methodResult.Result = mockTestAnswers;
            return methodResult;
        }

        private async Task<MethodResult<IList<MockTestAnswer>>> CreateAnswer(CreateMockTestAnswerBySectionGroupCommand request, IList<Section>? sections, Guid sectionGroupResultId)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            var methodResult = new MethodResult<IList<MockTestAnswer>>();
            var mockTestAnswers = new List<MockTestAnswer>();
            if (sections != null && sections.Any())
            {
                foreach (var item in request.Answers)
                {
                    var section = sections.FirstOrDefault(x => x.Id == item.SectionId);
                    if (section == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(section));
                        return methodResult;
                    }
                    var mockTestAnswer = await _mockTestAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.MockTestResultId == request.MockTestResultId && x.SectionQuestionId == request.SectionGroupId);
                    if (mockTestAnswer == null)
                    {
                        mockTestAnswers.Add(GetMockTestAnswer(item.Answer, default, request, sectionGroupResultId, default, section.Id));
                    }
                }
            }
            methodResult.Result = mockTestAnswers;
            return methodResult;
        }

        private async Task<MethodResult<IList<MockTestAnswer>>> CreateAnswer(CreateMockTestAnswerBySectionGroupCommand request, IList<SectionTimeCode>? sectionTimeCodes, Guid sectionGroupResultId)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            var methodResult = new MethodResult<IList<MockTestAnswer>>();
            var mockTestAnswers = new List<MockTestAnswer>();
            if (sectionTimeCodes != null && sectionTimeCodes.Any())
            {
                foreach (var item in request.Answers)
                {
                    var sectionTimeCode = sectionTimeCodes.FirstOrDefault(x => x.Id == item.SectionTimeCodeId);
                    if (sectionTimeCode == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionTimeCode));
                        return methodResult;
                    }
                    var mockTestAnswer = await _mockTestAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.MockTestResultId == request.MockTestResultId && x.SectionQuestionId == request.SectionGroupId);
                    if (mockTestAnswer == null)
                    {
                        mockTestAnswers.Add(GetMockTestAnswer(item.Answer, default, request, sectionGroupResultId, default, default, sectionTimeCode.Id));
                    }
                }
            }

            methodResult.Result = mockTestAnswers;
            return methodResult;
        }

        private static MockTestAnswer GetMockTestAnswer(object? answer, int correctCount, CreateMockTestAnswerBySectionGroupCommand request, Guid? sectionGroupResultId, Question? questionItem, Guid? sectionId, Guid? sectionTimeCodeId = default)
        {
            return new MockTestAnswer
            {
                Answer = answer,
                CorrectCount = correctCount,
                MockTestResultId = request.MockTestResultId,
                SectionTimeCodeId = sectionTimeCodeId ?? null,
                SectionId = sectionId ?? null,
                SectionQuestionId = questionItem?.SectionQuestions.FirstOrDefault()?.Id ?? default,
                SectionGroupResultId = sectionGroupResultId,
                IsCorrect = questionItem == null || questionItem.CorrectTotal == correctCount,
            };
        }
    }
}
