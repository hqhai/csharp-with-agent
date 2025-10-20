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
    using Fsel.Course.Lms.Application.Services.AiService.SpeakingAIService;
    using Fsel.Course.Lms.Application.Services.AIService.SpeakingAIService.Interface;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using static Fsel.Shared.Constants.ValueSettings;

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
        private readonly SectionGroupConverter _sectionGroupConverter;
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly ISectionTimeCodeRepository _sectionTimeCodeRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly SubmitMockTestAnswerPublisher _submitMockTestAnswerPublisher;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ISpeakingAIService _speakingAIService;
        private readonly ISpeakingEvaluationAIService _evaluationAIService;
        private readonly SubmitSpeakingAIPublisher _submitSpeakingAIPublisher;
        private readonly ILogger<CreateMockTestAnswerBySectionGroupCommand> _logger;
        private readonly DisconnectSocketCalculateTimePublisher _disconnectSocketCalculateTimePublisher;
        private readonly SaveUserSurveyAssignmentPublisher _saveUserSurveyAssignmentPublisher;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;

        public CreateMockTestAnswerBySectionGroupCommandHandler(IQuestionRepository questionRepository,
            AuthContext authContext,
            IUserService userService,
            QuestionConverter questionConverter,
            IMockTestAnswerRepository mockTestAnswerRepository,
            IMockTestResultRepository mockTestResultRepository,
            ISectionRepository sectionRepository,
            SectionGroupConverter sectionGroupConverter,
            ISectionGroupResultRepository sectionGroupResultRepository,
            ISectionTimeCodeRepository sectionTimeCodeRepository,
            ISectionGroupRepository sectionGroupRepository,
            SubmitMockTestAnswerPublisher submitMockTestAnswerPublisher,
            IMediator mediator,
            IMapper mapper,
            ILogger<CreateMockTestAnswerBySectionGroupCommand> logger,
            ISpeakingAIService speakingAIService,
            ISpeakingEvaluationAIService evaluationAIService,
            SubmitSpeakingAIPublisher submitSpeakingAIPublisher,
            DisconnectSocketCalculateTimePublisher disconnectSocketCalculateTimePublisher,
            SaveUserSurveyAssignmentPublisher saveUserSurveyAssignmentPublisher,
            ICourseUnitMockTestRepository courseUnitMockTestRepository)
        {
            _questionRepository = questionRepository;
            _authContext = authContext;
            _userService = userService;
            _questionConverter = questionConverter;
            _mockTestAnswerRepository = mockTestAnswerRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _sectionRepository = sectionRepository;
            _sectionGroupConverter = sectionGroupConverter;
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _sectionTimeCodeRepository = sectionTimeCodeRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _submitMockTestAnswerPublisher = submitMockTestAnswerPublisher;
            _mediator = mediator;
            _mapper = mapper;
            _logger = logger;
            _speakingAIService = speakingAIService;
            _evaluationAIService = evaluationAIService;
            _submitSpeakingAIPublisher = submitSpeakingAIPublisher;
            _disconnectSocketCalculateTimePublisher = disconnectSocketCalculateTimePublisher;
            _saveUserSurveyAssignmentPublisher = saveUserSurveyAssignmentPublisher;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
        }

        public async Task<MethodResult<SectionGroupResultModel>> Handle(CreateMockTestAnswerBySectionGroupCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            _logger.LoggerRequest(request);
            var methodResult = new MethodResult<SectionGroupResultModel>();

            #region Validate

            StudentModel? student;
            if (request.StudentId.HasValue)
            {
                var studentResult = await _userService.GetUserByStudentId(request.StudentId.Value);
                if (!studentResult.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                    return methodResult;
                }
                student = studentResult.Content?.Result;
            }
            else
            {
                var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
                if (!studentResult.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                    return methodResult;
                }
                student = studentResult.Content?.Result;
            }
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

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
            var sectionGroupResult = await _sectionGroupResultRepository.Queryable.Include(x => x.SectionGroup).Where(x => x.StudentId == student.Id && x.SectionGroupId == request.SectionGroupId && x.MockTestResultId == mockTestResult.Id && x.CreatedDate >= mockTestResult.CreatedDate).FirstOrDefaultAsync(cancellationToken);
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

            if (request.IsSubmit)
            {
                await _disconnectSocketCalculateTimePublisher.Publish(new SetTimeModuleModel
                {
                    Type = nameof(MockTest),
                    ObjectId = sectionGroupResult.Id
                }, cancellationToken);
            }

            var isSkillTest = mockTestResult.MockTest.MockTestType == EnumMockTestType.SkillMockTest;
            await _mockTestAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                if (request.Answers != null && request.Answers.Any())
                {
                    var answerResult = await SaveAnswerAsync(request, sectionGroup, sectionGroupResult, mockTestResult);
                    if (!answerResult.IsOK)
                    {
                        methodResult.AddErrorBadRequest(answerResult.ErrorMessages);
                        return methodResult;
                    }
                    sectionGroupResult = answerResult.Result;
                }
                if (sectionGroup.CourseSkill == EnumCourseSkill.Speaking && sectionGroupResult != null)
                {
                    await _sectionGroupResultRepository.BulkUpdateList(new List<SectionGroupResult> { sectionGroupResult }, bulk =>
                    {
                        bulk.ColumnInputExpression = entity => new { entity.CurrentSectionTimeCodeId };
                    });
                }
                sectionGroupResult = await _sectionGroupConverter.UpdateSectionGroupToIsSubmit(sectionGroup, sectionGroupResult, request.IsSubmit, mockTestResult.MockTest?.Version ?? (int)EnumVersion.V1);
                return methodResult;
            });

            if (!methodResult.IsOK)
            {
                return methodResult;
            }

            try
            {
                if (sectionGroup.CourseSkill == EnumCourseSkill.Writing && sectionGroup.Sections.Any() && request.IsSubmit)
                {
                    var sectionGroupId = sectionGroup.Id;
                    if (request.Answers != null && request.Answers.Count > 0)
                    {
                        foreach (var item in request.Answers)
                        {
                            if (item.SectionId == null)
                            {
                                continue;
                            }
                            await SendToChatGpt((Guid)item.SectionId, sectionGroupId, mockTestResult.Id, item.Answer?.ToString(), cancellationToken);
                        }
                    }
                    else if (request.Answers == null || request.Answers.Count == 0)
                    {
                        var mockTestAnswers = await _mockTestAnswerRepository.Queryable.Where(x => x.MockTestResultId == mockTestResult.Id && x.SectionGroupResultId == sectionGroupResult.Id)
                                                                       .Where(x => x.CreatedDate >= sectionGroupResult.CreatedDate)
                                                                       .ToListAsync(cancellationToken);
                        foreach (var item in mockTestAnswers)
                        {
                            await SendToChatGpt(item.SectionId ?? default, sectionGroupId, mockTestResult.Id, item.AnswerStr, cancellationToken);
                        }
                    }
                }
            }
            catch
            {
            }

            await UpdateMockTestResultAsync(mockTestResult, isSkillTest, cancellationToken);
            if (isSkillTest && (sectionGroup.CourseSkill == EnumCourseSkill.Reading || sectionGroup.CourseSkill == EnumCourseSkill.Listening))
            {
                await _mediator.Send(new SendTokenHistoryCommand { MockTestResultId = mockTestResult.Id }, cancellationToken);
            }

            var sectionGroupResultDto = _mapper.Map<SectionGroupResultModel>(sectionGroupResult);
            sectionGroupResultDto.IsTestDone = mockTestResult.Status == EnumResultStatus.Done;

            try
            {
                if (sectionGroup.CourseSkill == EnumCourseSkill.Speaking && sectionGroup.Sections.Any() && request.IsSubmit)
                {
                    // await _speakingAIService.EvaluationSpeakingAI(request.MockTestResultId, request.SectionGroupId, cancellationToken);

                    SpeakingAIEvaluationModel speakingEvaluationModel = new SpeakingAIEvaluationModel()
                    {
                        MockTestResultId = request.MockTestResultId,
                        SectionGroupId = request.SectionGroupId,
                    };
                    await _submitSpeakingAIPublisher.Publish(speakingEvaluationModel, cancellationToken);
                }
            }
            catch
            {
            }
            methodResult.Result = sectionGroupResultDto;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task SendToChatGpt(Guid sectionId, Guid sectionGroupId, Guid mockTestResultId, string? answer, CancellationToken cancellationToken)
        {
            await _submitMockTestAnswerPublisher.Publish(new MockTestAnswerResponseModel()
            {
                SectionId = sectionId,
                SectionGroupId = sectionGroupId,
                MockTestResultId = mockTestResultId,
                WordContent = (answer == "null" || string.IsNullOrEmpty(answer)) ? string.Empty : answer,
            }, cancellationToken);
        }

        private async Task UpdateMockTestResultAsync(MockTestResult mockTestResult, bool isSkillTest, CancellationToken cancellationToken)
        {
            var numberOfDone = 4;
            var sectionGroupResults = await _sectionGroupResultRepository.Queryable.Where(s => s.MockTestResultId == mockTestResult.Id && s.CreatedDate >= mockTestResult.CreatedDate).OrderBy(x => x.CreatedDate).ToListAsync(cancellationToken);
            if (sectionGroupResults != null && (isSkillTest || sectionGroupResults.Count == numberOfDone) && sectionGroupResults.All(x => x.Status == EnumResultStatus.Done))
            {
                mockTestResult.WorkingTime = sectionGroupResults.Sum(x => x.WorkingTime);
                mockTestResult.HighestStreak = sectionGroupResults.Max(x => x.HighestStreak);
                mockTestResult = GetMockTestResult(sectionGroupResults, mockTestResult);
                await _mockTestResultRepository.BulkUpdateList(new List<MockTestResult> { mockTestResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId, c.MockTestId, c.UnitId };
                });
                if (mockTestResult.Status == EnumResultStatus.Done)
                {
                    var courseUnitMockTest = await _courseUnitMockTestRepository.Queryable.Include(p => p.Course).FirstOrDefaultAsync(p => p.CourseId == mockTestResult.CourseId && p.MockTestId == mockTestResult.MockTestId, cancellationToken);
                    if (courseUnitMockTest != null)
                    {
                        await SaveSurvey(courseUnitMockTest, cancellationToken);
                    }

                    await _mockTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private async Task SaveSurvey(CourseUnitMockTest courseUnitMockTest, CancellationToken cancellationToken)
        {
            await _saveUserSurveyAssignmentPublisher.Publish(new SaveUserSurveyAssignmentCommandModel()
            {
                CourseLevel = courseUnitMockTest.Course!.CourseLevel,
                CourseType = courseUnitMockTest.Course!.CourseType,
                ProgressRequirement = courseUnitMockTest.Number == 1 ? EnumProgressRequirement.DoneFullMockTestOne : EnumProgressRequirement.DoneFullMockTestTwo,
                IsSurveyQuestBoard = false
            }, cancellationToken);
        }

        private static MockTestResult GetMockTestResult(IList<SectionGroupResult>? sectionGroupResults, MockTestResult mockTestResult)
        {
            var skillScores = sectionGroupResults?.Where(x => x.SkillScores != null && x.SkillScores.Any()).SelectMany(x => x.SkillScores!).OrderBy(x => x.Skill).ToList();
            if (skillScores != null)
            {
                mockTestResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
                mockTestResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
            }
            mockTestResult.Status = EnumResultStatus.Done;
            mockTestResult.SkillScores = skillScores;
            return mockTestResult;
        }

        private async Task<MethodResult<SectionGroupResult>> SaveAnswerAsync(CreateMockTestAnswerBySectionGroupCommand request, SectionGroup sectionGroup, SectionGroupResult sectionGroupResult, MockTestResult mockTestResult)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            var methodResult = new MethodResult<SectionGroupResult>();
            var anserResult = new MethodResult<(IList<MockTestAnswer>, IList<MockTestAnswer>)>();
            if (sectionGroup.CourseSkill == EnumCourseSkill.Listening || sectionGroup.CourseSkill == EnumCourseSkill.Reading)
            {
                var questionIds = request.Answers.Where(x => x.QuestionId.HasValue).Select(x => x.QuestionId!.Value).ToList();
                var questions = await _questionRepository.GetIncludeSectionByIdAsync(questionIds, mockTestResult.MockTest?.Version);
                if (questions == null || questions.Count == 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questions));
                    return methodResult;
                }
                var sectionQuestions = questions.SelectMany(x => x.SectionQuestions);
                var sectionGroupIds = new List<Guid?>();
                if (sectionQuestions.Select(x => x.SectionPart).Any())
                {
                    sectionGroupIds = sectionQuestions.Select(x => x.SectionPart?.Section?.SectionGroupId).Distinct().ToList();
                }
                else
                {
                    sectionGroupIds = sectionQuestions.Select(x => x.Section?.SectionGroupId).Distinct().ToList();
                }
                if (!sectionGroupIds.Any(x => x == sectionGroup.Id))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(questions), nameof(request.SectionGroupId));
                    return methodResult;
                }

                anserResult = await SaveAnswer(request, questions, sectionGroupResult, mockTestResult);
            }
            else if (sectionGroup.CourseSkill == EnumCourseSkill.Writing)
            {
                var sectionIds = request.Answers.Where(x => x.SectionId.HasValue).Select(x => x.SectionId!.Value).ToList();
                var sections = await _sectionRepository.Queryable.Where(x => sectionIds.Contains(x.Id)).ToListAsync();
                if (sections == null || !sections.Any())
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sections));
                    return methodResult;
                }

                if (!sections.Any(x => x.SectionGroupId == sectionGroup.Id))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(sections), nameof(request.SectionGroupId));
                    return methodResult;
                }
                anserResult = await SaveAnswer(request, sections, sectionGroupResult);
            }
            else
            {
                var sectionTimeCodeIds = request.Answers.Where(x => x.SectionTimeCodeId.HasValue).Select(x => x.SectionTimeCodeId!.Value).ToList();
                var sectionTimeCodes = await _sectionTimeCodeRepository.Queryable.Include(x => x.Section).WhereBulkContains(sectionTimeCodeIds, x => x.Id).ToListAsync();
                if (sectionTimeCodes == null || !sectionTimeCodes.Any())
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionTimeCodes));
                    return methodResult;
                }

                if (!sectionTimeCodes.Select(x => x.Section).Any(x => x != null && x.SectionGroupId == sectionGroup.Id))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(sectionTimeCodes), nameof(request.SectionGroupId));
                    return methodResult;
                }
                anserResult = await SaveAnswer(request, sectionTimeCodes.ToList(), sectionGroupResult);
            }
            if (!anserResult.IsOK)
            {
                methodResult.AddErrorBadRequest(anserResult.ErrorMessages);
                return methodResult;
            }
            var (createMockTestAnswers, updateMockTestAnswers) = anserResult.Result;

            try
            {
                if (createMockTestAnswers != null && createMockTestAnswers.Any())
                {
                    await _mockTestAnswerRepository.BulkMergeAsync(createMockTestAnswers, bulk =>
                    {
                        bulk.ColumnPrimaryKeyExpression = entity => new { entity.SectionGroupResultId, entity.SectionId, entity.SectionTimeCodeId, entity.SectionQuestionId, entity.MockTestResultId, entity.IsDeleted };
                    });
                }
                if (updateMockTestAnswers != null && updateMockTestAnswers.Any())
                {
                    await _mockTestAnswerRepository.BulkUpdateList(updateMockTestAnswers, bulk =>
                    {
                        bulk.IgnoreOnUpdateExpression = entity => new { entity.MockTestResultId, entity.SectionGroupResultId, entity.SectionQuestionId, entity.SectionId, entity.SectionTimeCodeId };
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Log Duplicate MockTestAnswer : {ex.Message}");
            }
            methodResult.Result = sectionGroupResult;
            return methodResult;
        }

        private async Task<MethodResult<(IList<MockTestAnswer>, IList<MockTestAnswer>)>> SaveAnswer(CreateMockTestAnswerBySectionGroupCommand request, IList<Question>? questions, SectionGroupResult sectionGroupResult, MockTestResult mockTestResult)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            ArgumentNullException.ThrowIfNull(questions);

            var methodResult = new MethodResult<(IList<MockTestAnswer>, IList<MockTestAnswer>)>();
            var createMockTestAnswers = new List<MockTestAnswer>();
            var updateMockTestAnswers = new List<MockTestAnswer>();
            if (questions != null && questions.Any())
            {
                var mockTestAnswers = await _mockTestAnswerRepository.Queryable.Where(x => x.CreatedDate >= sectionGroupResult.CreatedDate)
                                                                     .Where(x => x.SectionGroupResultId == sectionGroupResult.Id)
                                                                     .ToListAsync();

                foreach (var item in request.Answers)
                {
                    var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                    var questionResult = _questionConverter.HandleAnswerTest(question, item.Answer, request.IsSubmit, mockTestResult.MockTest?.Version == (int)EnumVersion.V2);
                    if (!questionResult.IsOK)
                    {
                        methodResult.AddErrorBadRequest(questionResult.ErrorMessages);
                        return methodResult;
                    }
                    var (questionItem, answerConfig, correctCount, isAnswered) = questionResult.Result;
                    var sectionQuestionId = questionItem.SectionQuestions.FirstOrDefault()?.Id ?? default;
                    var mockTestAnswer = mockTestAnswers.FirstOrDefault(x => x.SectionGroupResultId == sectionGroupResult.Id && x.SectionQuestionId == sectionQuestionId);
                    if (mockTestAnswer == null)
                    {
                        mockTestAnswer = GetMockTestAnswer(sectionGroupResult, questionItem);
                        mockTestAnswer = GetMockTestAnswer(mockTestAnswer, answerConfig, questionItem, isAnswered, item.SpeechTextAnswer, correctCount);
                        if (!mockTestAnswer.IsValid())
                        {
                            methodResult.AddErrorBadRequest(mockTestAnswer.ErrorMessages);
                            return methodResult;
                        }
                        createMockTestAnswers.Add(mockTestAnswer);
                    }
                    else
                    {
                        mockTestAnswer = GetMockTestAnswer(mockTestAnswer, answerConfig, questionItem, isAnswered, item.SpeechTextAnswer, correctCount);
                        if (!mockTestAnswer.IsValid())
                        {
                            methodResult.AddErrorBadRequest(mockTestAnswer.ErrorMessages);
                            return methodResult;
                        }
                        updateMockTestAnswers.Add(mockTestAnswer);
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
                var mockTestAnswers = await _mockTestAnswerRepository.Queryable.Where(x => x.CreatedDate >= sectionGroupResult.CreatedDate)
                                                                   .Where(x => x.SectionGroupResultId == sectionGroupResult.Id)
                                                                   .ToListAsync();

                foreach (var item in request.Answers)
                {
                    var section = sections.FirstOrDefault(x => x.Id == item.SectionId);
                    if (section == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(section));
                        return methodResult;
                    }
                    int answerLength = item.Answer?.ToString()?.Length ?? default;
                    if (section.DisplayOrder == AnswerLength.Section0 && answerLength > AnswerLength.MaxLengthDisplayOrder0)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(item.Answer), item.Answer ?? new(), answerLength);
                        return methodResult;
                    }
                    if (section.DisplayOrder == AnswerLength.Section1 && answerLength > AnswerLength.MaxLengthDisplayOrder1)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(item.Answer), item.Answer ?? new(), answerLength);
                        return methodResult;
                    }
                    var mockTestAnswer = mockTestAnswers.FirstOrDefault(x => x.SectionGroupResultId == sectionGroupResult.Id && x.SectionId == section.Id);
                    if (mockTestAnswer == null)
                    {
                        mockTestAnswer = GetMockTestAnswer(sectionGroupResult, section.Id);
                        mockTestAnswer = GetMockTestAnswer(mockTestAnswer, item.Answer, item.SpeechTextAnswer, 0);

                        if (!mockTestAnswer.IsValid())
                        {
                            methodResult.AddErrorBadRequest(mockTestAnswer.ErrorMessages);
                            return methodResult;
                        }
                        createMockTestAnswers.Add(mockTestAnswer);
                    }
                    else
                    {
                        mockTestAnswer = GetMockTestAnswer(mockTestAnswer, item.Answer, item.SpeechTextAnswer, 0);
                        if (!mockTestAnswer.IsValid())
                        {
                            methodResult.AddErrorBadRequest(mockTestAnswer.ErrorMessages);
                            return methodResult;
                        }
                        updateMockTestAnswers.Add(mockTestAnswer);
                    }
                }
            }
            methodResult.Result = (createMockTestAnswers, updateMockTestAnswers);
            return methodResult;
        }

        private async Task<MethodResult<(IList<MockTestAnswer>, IList<MockTestAnswer>)>> SaveAnswer(CreateMockTestAnswerBySectionGroupCommand request, IList<SectionTimeCode> sectionTimeCodes, SectionGroupResult sectionGroupResult)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            var methodResult = new MethodResult<(IList<MockTestAnswer>, IList<MockTestAnswer>)>();

            if (sectionGroupResult.CurrentSectionTimeCodeId.HasValue)
            {
                var currentSectionTimeCode = await _sectionTimeCodeRepository.GetByIdAsync(sectionGroupResult.CurrentSectionTimeCodeId.Value);
                if (currentSectionTimeCode == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(currentSectionTimeCode));
                    return methodResult;
                }
                if (sectionTimeCodes.Any(x => x.DisplayTime > currentSectionTimeCode.DisplayTime))
                {
                    sectionGroupResult.CurrentSectionTimeCodeId = sectionTimeCodes.OrderByDescending(x => x.DisplayTime).FirstOrDefault()?.Id;
                }
            }
            else
            {
                sectionGroupResult.CurrentSectionTimeCodeId = sectionTimeCodes.OrderByDescending(x => x.DisplayTime).FirstOrDefault()?.Id;
            }

            var createMockTestAnswers = new List<MockTestAnswer>();
            var updateMockTestAnswers = new List<MockTestAnswer>();

            var mockTestAnswers = await _mockTestAnswerRepository.Queryable.Where(x => x.CreatedDate >= sectionGroupResult.CreatedDate)
                                                                .Where(x => x.SectionGroupResultId == sectionGroupResult.Id)
                                                                .ToListAsync();

            foreach (var item in request.Answers)
            {
                var sectionTimeCode = sectionTimeCodes.FirstOrDefault(x => x.Id == item.SectionTimeCodeId);
                if (sectionTimeCode != null)
                {
                    var mockTestAnswer = mockTestAnswers.FirstOrDefault(x => x.SectionGroupResultId == sectionGroupResult.Id && x.SectionTimeCodeId == sectionTimeCode.Id);
                    if (mockTestAnswer == null)
                    {
                        mockTestAnswer = GetMockTestAnswer(sectionGroupResult, null, sectionTimeCode.Id);
                        double pronScore = await _evaluationAIService.EvaluationSpeakingV1(sectionTimeCode.Name, item?.Answer?.ToString() ?? default);

                        mockTestAnswer = GetMockTestAnswer(mockTestAnswer, item?.Answer, item?.SpeechTextAnswer, pronScore);
                        if (!mockTestAnswer.IsValid())
                        {
                            methodResult.AddErrorBadRequest(mockTestAnswer.ErrorMessages);
                            return methodResult;
                        }
                        createMockTestAnswers.Add(mockTestAnswer);
                    }
                    else
                    {
                        double pronScore = await _evaluationAIService.EvaluationSpeakingV1(sectionTimeCode.Name, item?.Answer?.ToString() ?? default);

                        mockTestAnswer = GetMockTestAnswer(mockTestAnswer, item?.Answer, item?.SpeechTextAnswer, pronScore);
                        if (!mockTestAnswer.IsValid())
                        {
                            methodResult.AddErrorBadRequest(mockTestAnswer.ErrorMessages);
                            return methodResult;
                        }
                        updateMockTestAnswers.Add(mockTestAnswer);
                    }
                }
            }

            methodResult.Result = (createMockTestAnswers, updateMockTestAnswers);
            return methodResult;
        }

        private static MockTestAnswer GetMockTestAnswer(SectionGroupResult sectionGroupResult, Guid? sectionId = null, Guid? sectionTimeCodeId = null)
        {
            return new MockTestAnswer
            {
                MockTestResultId = sectionGroupResult.MockTestResultId ?? default,
                SectionTimeCodeId = sectionTimeCodeId ?? null,
                SectionId = sectionId ?? null,
                SectionGroupResultId = sectionGroupResult.Id,
                IsCorrect = null,
                Status = EnumAnswerStatus.Process,
            };
        }

        private static MockTestAnswer GetMockTestAnswer(SectionGroupResult sectionGroupResult, Question questionItem)
        {
            return new MockTestAnswer
            {
                MockTestResultId = sectionGroupResult.MockTestResultId ?? default,
                SectionQuestionId = questionItem?.SectionQuestions.FirstOrDefault()?.Id,
                SectionGroupResultId = sectionGroupResult.Id,
                Status = EnumAnswerStatus.Process,
            };
        }

        private static MockTestAnswer GetMockTestAnswer(MockTestAnswer mockTestAnswer, object? answer, Question questionItem, bool isAnswered, string? speechText, short correctCount = default)
        {
            mockTestAnswer = GetMockTestAnswer(mockTestAnswer, answer, speechText, 0, correctCount);
            mockTestAnswer.IsCorrect = isAnswered ? (questionItem == null || questionItem.CorrectTotal == correctCount) : null;
            return mockTestAnswer;
        }

        private static MockTestAnswer GetMockTestAnswer(MockTestAnswer mockTestAnswer, object? answer, string? speechText, double pronsScore, short correctCount = default)
        {
            mockTestAnswer.Answer = answer;
            mockTestAnswer.SpeechTextAnswer = speechText;
            mockTestAnswer.PronunciationScore = pronsScore;
            mockTestAnswer.CorrectCount = correctCount;
            return mockTestAnswer;
        }
    }
}