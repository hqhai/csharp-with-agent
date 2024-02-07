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
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.MockTestAnswers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumAutoDot;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Queues.Publishers;
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
        private readonly SubmitMockTestAnswerPublisher _submitMockTestAnswerPublisher;
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
            , IMapper mapper
            , SubmitMockTestAnswerPublisher submitMockTestAnswerPublisher)
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
            _submitMockTestAnswerPublisher = submitMockTestAnswerPublisher;
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
            if (mockTestResult == null || mockTestResult.MockTest == null)
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

            var isSkillTest = mockTestResult.MockTest.MockTestType == EnumMockTestType.SkillMockTest;
            await _mockTestAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                if (request.Answers != null && request.Answers.Any())
                {
                    var answerResult = await SaveAnswerAsync(request, sectionGroup, sectionGroupResult);
                    if (!answerResult.IsOK)
                    {
                        methodResult.AddErrorBadRequest(answerResult.ErrorMessages);
                        return methodResult;
                    }
                    sectionGroupResult = answerResult.Result;

                    if (sectionGroup.CourseSkill == EnumCourseSkill.Writing)
                    {
                        foreach (var item in request.Answers)
                        {
                            if (item.SectionId == null || sectionGroupResult == null)
                            {
                                continue;
                            }
                            await _submitMockTestAnswerPublisher.Publish(new MockTestAnswerResponseModel()
                            {
                                SectionId = (Guid)item.SectionId,
                                MockTestResultId = mockTestResult.Id,
                                WordContent = item.Answer?.ToString() ?? string.Empty,
                            }, cancellationToken);
                        }
                    }
                }
                sectionGroupResult = await _sectionGroupConverter.UpdateSectionGroupToIsSubmit(sectionGroup, sectionGroupResult, request.IsSubmit);

                if (sectionGroup.CourseSkill == EnumCourseSkill.Reading || sectionGroup.CourseSkill == EnumCourseSkill.Listening)
                {
                    sectionGroupResult = await UpdateSectionGroupResultAsync(sectionGroup, sectionGroupResult, isSkillTest);
                }
                return methodResult;
            });

            await UpdateMockTestResultAsync(mockTestResult, isSkillTest, cancellationToken);
            var sectionGroupResultDto = _mapper.Map<SectionGroupResultModel>(sectionGroupResult);
            sectionGroupResultDto.IsTestDone = mockTestResult.Status == EnumResultStatus.Done;
            methodResult.Result = sectionGroupResultDto;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<SectionGroupResult?> UpdateSectionGroupResultAsync(SectionGroup sectionGroup, SectionGroupResult? sectionGroupResult, bool isSkillTest)
        {
            if (sectionGroupResult != null)
            {
                if (sectionGroup.CourseSkill == EnumCourseSkill.Reading)
                {
                    sectionGroupResult.TokenFirstTime = (int?)await GetTokenAsync(isSkillTest ? EnumTokenMission.SkillMockTestReading : EnumTokenMission.FullMockTestReading, isSkillTest);
                }
                else if (sectionGroup.CourseSkill == EnumCourseSkill.Listening)
                {
                    sectionGroupResult.TokenFirstTime = (int?)await GetTokenAsync(isSkillTest ? EnumTokenMission.SkillMockTestListening : EnumTokenMission.FullMockTestListening, isSkillTest);
                }
                _sectionGroupResultRepository.Update(sectionGroupResult);
                await _sectionGroupResultRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
            }

            return sectionGroupResult;
        }

        private async Task<long> GetTokenAsync(EnumTokenMission mission, bool isSkillTest)
        {
            var tokenConfigs = await _systemService.GetTokenConfigAsync(new GetTokenQueryModel
            {
                Feature = isSkillTest ? EnumTokenFeature.SkillMockTest : EnumTokenFeature.FullMockTest,
                Mission = mission,
                CourseType = EnumCourseType.Ielts
            });
            if (!tokenConfigs.IsSuccessStatusCode)
            {
                return default;
            }
            var tokenConfig = tokenConfigs.Content?.Result;
            return tokenConfig.GetTokenConfig<TokenCoinConfigs>()?.BaseValue ?? default;
        }

        private async Task UpdateMockTestResultAsync(MockTestResult mockTestResult, bool isSkillTest, CancellationToken cancellationToken)
        {
            var numberOfDone = 4;
            var sectionGroupResults = await _sectionGroupResultRepository.Queryable.Where(s => s.MockTestResultId == mockTestResult.Id).ToListAsync(cancellationToken);

            if (sectionGroupResults != null && (isSkillTest || sectionGroupResults.Count == numberOfDone) && sectionGroupResults.All(x => x.Status == EnumResultStatus.Done))
            {
                mockTestResult.WorkingTime = sectionGroupResults.Sum(x => x.WorkingTime);
                mockTestResult.HighestStreak = sectionGroupResults.Max(x => x.HighestStreak);
                mockTestResult = GetMockTestResult(sectionGroupResults, mockTestResult);
                _mockTestResultRepository.Update(mockTestResult);
                await _mockTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private static MockTestResult GetMockTestResult(IList<SectionGroupResult>? sectionGroupResults, MockTestResult mockTestResult)
        {
            var skillScores = sectionGroupResults?.SelectMany(x => x.SkillScores!).OrderBy(x => x.Skill).ToList();
            if (skillScores != null)
            {
                mockTestResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
                mockTestResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
                mockTestResult.TokenFirstTime = sectionGroupResults?.Sum(x => x.TokenFirstTime);
            }
            mockTestResult.Status = EnumResultStatus.Done;
            mockTestResult.SkillScores = skillScores;
            return mockTestResult;
        }

        private async Task<MethodResult<SectionGroupResult>> SaveAnswerAsync(CreateMockTestAnswerBySectionGroupCommand request, SectionGroup sectionGroup, SectionGroupResult sectionGroupResult)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            var methodResult = new MethodResult<SectionGroupResult>();
            var anserResult = new MethodResult<(IList<MockTestAnswer>, IList<MockTestAnswer>)>();
            if (sectionGroup.CourseSkill == EnumCourseSkill.Listening || sectionGroup.CourseSkill == EnumCourseSkill.Reading)
            {
                var questionIds = request.Answers.Where(x => x.QuestionId.HasValue).Select(x => x.QuestionId!.Value).ToList();
                var questions = await _questionRepository.GetIncludeSectionByIdAsync(questionIds);
                if (questions == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }
                anserResult = await SaveAnswer(request, questions, sectionGroupResult);
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
                anserResult = await SaveAnswer(request, sections, sectionGroupResult);
            }
            else
            {
                var sectionTimeCodeId = request.Answers.Where(x => x.SectionTimeCodeId.HasValue).Select(x => x.SectionTimeCodeId!.Value).FirstOrDefault();
                var sectionTimeCode = await _sectionTimeCodeRepository.GetByIdAsync(sectionTimeCodeId);
                if (sectionTimeCode == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }
                sectionGroupResult.CurrentSectionTimeCodeId = sectionTimeCodeId;
                anserResult = await SaveAnswer(request, sectionTimeCode, sectionGroupResult);
            }

            if (!anserResult.IsOK)
            {
                methodResult.AddErrorBadRequest(anserResult.ErrorMessages);
                return methodResult;
            }
            var (createMockTestAnswers, updateMockTestAnswers) = anserResult.Result;
            if (createMockTestAnswers != null && createMockTestAnswers.Any())
            {
                await _mockTestAnswerRepository.AddList(createMockTestAnswers);
                await _mockTestAnswerRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
            }
            if (updateMockTestAnswers != null && updateMockTestAnswers.Any())
            {
                _mockTestAnswerRepository.UpdateList(updateMockTestAnswers);
                await _mockTestAnswerRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
            }
            methodResult.Result = sectionGroupResult;
            return methodResult;
        }

        private async Task<MethodResult<(IList<MockTestAnswer>, IList<MockTestAnswer>)>> SaveAnswer(CreateMockTestAnswerBySectionGroupCommand request, IList<Question>? questions, SectionGroupResult sectionGroupResult)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            ArgumentNullException.ThrowIfNull(questions);
            var methodResult = new MethodResult<(IList<MockTestAnswer>, IList<MockTestAnswer>)>();
            var createMockTestAnswers = new List<MockTestAnswer>();
            var updateMockTestAnswers = new List<MockTestAnswer>();
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
                    var (questionItem, answerConfig, correctCount, isAnswered) = questionResult.Result;
                    var sectionQuestionId = questionItem.SectionQuestions.FirstOrDefault()?.Id ?? default;

                    var mockTestAnswer = await _mockTestAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.SectionGroupResultId == sectionGroupResult.Id && x.SectionQuestionId == sectionQuestionId);
                    if (mockTestAnswer == null)
                    {
                        mockTestAnswer = GetMockTestAnswer(sectionGroupResult, correctCount, isAnswered, questionItem);
                        createMockTestAnswers.Add(GetMockTestAnswer(mockTestAnswer, answerConfig, correctCount));
                    }
                    else
                    {
                        updateMockTestAnswers.Add(GetMockTestAnswer(mockTestAnswer, answerConfig, correctCount));
                    }
                }
            }
            methodResult.Result = (createMockTestAnswers, updateMockTestAnswers);
            return methodResult;
        }

        private async Task<MethodResult<(IList<MockTestAnswer>, IList<MockTestAnswer>)>> SaveAnswer(CreateMockTestAnswerBySectionGroupCommand request, IList<Section>? sections, SectionGroupResult sectionGroupResult)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            var methodResult = new MethodResult<(IList<MockTestAnswer>, IList<MockTestAnswer>)>();
            var createMockTestAnswers = new List<MockTestAnswer>();
            var updateMockTestAnswers = new List<MockTestAnswer>();
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
                    var mockTestAnswer = await _mockTestAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.MockTestResultId == request.MockTestResultId && x.SectionId == section.Id);
                    if (mockTestAnswer == null)
                    {
                        mockTestAnswer = GetMockTestAnswerWriting(sectionGroupResult, section.Id);
                        createMockTestAnswers.Add(GetMockTestAnswer(mockTestAnswer, item.Answer));
                    }
                    else
                    {
                        updateMockTestAnswers.Add(GetMockTestAnswer(mockTestAnswer, item.Answer));
                    }
                }
            }
            methodResult.Result = (createMockTestAnswers, updateMockTestAnswers);
            return methodResult;
        }

        private async Task<MethodResult<(IList<MockTestAnswer>, IList<MockTestAnswer>)>> SaveAnswer(CreateMockTestAnswerBySectionGroupCommand request, SectionTimeCode sectionTimeCode, SectionGroupResult sectionGroupResult)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            var methodResult = new MethodResult<(IList<MockTestAnswer>, IList<MockTestAnswer>)>();
            var createMockTestAnswers = new List<MockTestAnswer>();
            var updateMockTestAnswers = new List<MockTestAnswer>();
            var mockTestAnswer = await _mockTestAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.MockTestResultId == request.MockTestResultId && x.SectionTimeCodeId == sectionTimeCode.Id);
            if (mockTestAnswer == null)
            {
                mockTestAnswer = GetMockTestAnswerSpeaking(sectionGroupResult, sectionTimeCode.Id);
                createMockTestAnswers.Add(GetMockTestAnswer(mockTestAnswer, request.Answers.Select(x => x.Answer).FirstOrDefault()));
            }
            else
            {
                updateMockTestAnswers.Add(GetMockTestAnswer(mockTestAnswer, request.Answers.Select(x => x.Answer).FirstOrDefault()));
            }

            methodResult.Result = (createMockTestAnswers, updateMockTestAnswers);
            return methodResult;
        }

        private static MockTestAnswer GetMockTestAnswerWriting(SectionGroupResult sectionGroupResult, Guid? sectionId = null)
        {
            return new MockTestAnswer
            {
                MockTestResultId = sectionGroupResult.MockTestResultId ?? default,
                SectionId = sectionId ?? null,
                SectionGroupResultId = sectionGroupResult.Id,
                IsCorrect = null,
                Status = EnumAnswerStatus.Done
            };
        }

        private static MockTestAnswer GetMockTestAnswerSpeaking(SectionGroupResult sectionGroupResult, Guid? sectionTimeCodeId = null)
        {
            return new MockTestAnswer
            {
                MockTestResultId = sectionGroupResult.MockTestResultId ?? default,
                SectionTimeCodeId = sectionTimeCodeId ?? null,
                SectionGroupResultId = sectionGroupResult.Id,
                IsCorrect = null,
                Status = EnumAnswerStatus.Done
            };
        }

        private static MockTestAnswer GetMockTestAnswer(SectionGroupResult sectionGroupResult, int correctCount, bool isAnswered, Question questionItem)
        {
            return new MockTestAnswer
            {
                MockTestResultId = sectionGroupResult.MockTestResultId ?? default,
                SectionQuestionId = questionItem?.SectionQuestions.FirstOrDefault()?.Id,
                SectionGroupResultId = sectionGroupResult.Id,
                Status = questionItem?.CorrectTotal == correctCount ? EnumAnswerStatus.Done : EnumAnswerStatus.Process,
                IsCorrect = isAnswered ? (questionItem == null || questionItem.CorrectTotal == correctCount) : null,
            };
        }

        private static MockTestAnswer GetMockTestAnswer(MockTestAnswer mockTestAnswer, object? answer, int correctCount = default)
        {
            mockTestAnswer.Answer = answer;
            mockTestAnswer.CorrectCount = correctCount;
            return mockTestAnswer;
        }
    }
}
