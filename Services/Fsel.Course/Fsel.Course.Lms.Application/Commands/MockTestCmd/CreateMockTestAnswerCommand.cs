// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.MockTestCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.MockTestAnswers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Infrastructure.Repositories;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Shared.ApplicationServices.CacheServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateMockTestAnswerCommand : CreateMockTestAnswerCommandModel, IRequest<MethodResult<MockTestResultModel>>
    {
    }

    public class CreateMockTestAnswerCommandHandler : IRequestHandler<CreateMockTestAnswerCommand, MethodResult<MockTestResultModel>>
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly IMockTestAnswerRepository _mockTestAnswerRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly QuestionConverter _questionConverter;
        private readonly ISectionTimeCodeRepository _sectionTimeCodeRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly SubmitMockTestAnswerPublisher _submitMockTestAnswerPublisher;
        private readonly IMapper _mapper;
        private readonly IRequestSafeCachingService _requestSafeCachingService;

        public CreateMockTestAnswerCommandHandler(IQuestionRepository questionRepository
            , IMockTestAnswerRepository mockTestAnswerRepository
            , IMockTestResultRepository mockTestResultRepository
            , ISectionRepository sectionRepository
            , QuestionConverter questionConverter
            , ISectionTimeCodeRepository sectionTimeCodeRepository
            , ISectionGroupRepository sectionGroupRepository
            , IMapper mapper
            , SubmitMockTestAnswerPublisher submitMockTestAnswerPublisher
            , IRequestSafeCachingService requestSafeCachingService)
        {
            _questionRepository = questionRepository;
            _mockTestAnswerRepository = mockTestAnswerRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _sectionRepository = sectionRepository;
            _questionConverter = questionConverter;
            _sectionTimeCodeRepository = sectionTimeCodeRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _mapper = mapper;
            _submitMockTestAnswerPublisher = submitMockTestAnswerPublisher;
            _requestSafeCachingService = requestSafeCachingService;
        }

        public async Task<MethodResult<MockTestResultModel>> Handle(CreateMockTestAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<MockTestResultModel> methodResult = new MethodResult<MockTestResultModel>();
            if (request.SectionGroups == null || request.SectionGroups.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.SectionGroups));
                return methodResult;
            }
            var mockTestResult = await _mockTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.MockTestResultId, cancellationToken);
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
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusUnfinished), nameof(mockTestResult));
                return methodResult;
            }
            var mockTestAnswers = new List<MockTestAnswer>();
            var skillScores = new List<SkillScores>();
            var sectionGroups = await _sectionGroupRepository.Queryable.Where(x => request.SectionGroups.Select(x => x.SectionGroupId).Contains(x.Id)).ToListAsync(cancellationToken);

            if (sectionGroups == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroups));
                return methodResult;
            }

            var questionIds = request.SectionGroups.Where(x => x.Answers != null).SelectMany(x => x.Answers!).Select(x => x.QuestionId ?? default).ToList();
            var questions = await _questionRepository.GetIncludeSectionByIdAsync(questionIds);
            foreach (var item in request.SectionGroups)
            {
                if (item.Answers != null)
                {
                    if (item.Answers == null || item.Answers.Count == 0)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.SectionGroups));
                        return methodResult;
                    }

                    var count = 0;
                    var questionCount = 0;
                    foreach (var answer in item.Answers)
                    {
                        if (answer.QuestionId != null && questions != null)
                        {
                            var question = questions.FirstOrDefault(x => x.Id == answer.QuestionId);
                            var questionResult = _questionConverter.HandleQuestionAnswer(question, answer.Answer, true);
                            if (!questionResult.IsOK)
                            {
                                methodResult.AddErrorBadRequest(questionResult.ErrorMessages);
                                return methodResult;
                            }
                            var (questionItem, answerConfig, correctCount, isAnswered) = questionResult.Result;
                            var sectionQuestionId = questionItem.SectionQuestions.FirstOrDefault()!.Id;
                            var mockTestAnswer = await _mockTestAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.MockTestResultId == mockTestResult.Id && x.SectionQuestionId == sectionQuestionId, cancellationToken);
                            if (mockTestAnswer == null)
                            {
                                count += correctCount;
                                questionCount += questionItem.CorrectTotal;
                                mockTestAnswer = new MockTestAnswer
                                {
                                    Answer = answerConfig,
                                    CorrectCount = correctCount,
                                    MockTestResultId = mockTestResult.Id,
                                    SectionQuestionId = sectionQuestionId,
                                    IsCorrect = isAnswered ? correctCount == questionItem.CorrectTotal : null,
                                };
                                mockTestAnswers.Add(mockTestAnswer);
                            }
                        }
                        else if (answer.SectionId != null)
                        {
                            var sectionId = answer.SectionId;
                            var section = await _sectionRepository.Queryable.FirstOrDefaultAsync(x => x.Id == sectionId, cancellationToken);
                            if (section == null)
                            {
                                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(section));
                                return methodResult;
                            }
                            var mockTestAnswer = await _mockTestAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.MockTestResultId == mockTestResult.Id && x.SectionId == sectionId, cancellationToken);
                            if (mockTestAnswer == null)
                            {
                                mockTestAnswer = new MockTestAnswer
                                {
                                    Answer = answer.Answer,
                                    MockTestResultId = mockTestResult.Id,
                                    SectionId = sectionId,
                                    IsCorrect = true
                                };
                                mockTestAnswers.Add(mockTestAnswer);
                            }
                        }
                        else
                        {
                            var sectionTimeCodeId = answer.SectionTimeCodeId;
                            var sectionTimeCode = await _sectionTimeCodeRepository.Queryable.FirstOrDefaultAsync(x => x.Id == sectionTimeCodeId, cancellationToken);
                            if (sectionTimeCode == null)
                            {
                                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionTimeCode));
                                return methodResult;
                            }
                            var mockTestAnswer = await _mockTestAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.MockTestResultId == mockTestResult.Id && x.SectionTimeCodeId == sectionTimeCodeId, cancellationToken);
                            if (mockTestAnswer == null)
                            {
                                mockTestAnswer = new MockTestAnswer
                                {
                                    Answer = answer.Answer,
                                    MockTestResultId = mockTestResult.Id,
                                    SectionTimeCodeId = sectionTimeCodeId,
                                    IsCorrect = true
                                };
                                mockTestAnswers.Add(mockTestAnswer);
                            }
                        }
                    }
                    var sectionGroup = sectionGroups.FirstOrDefault(x => x.Id == item.SectionGroupId);
                    if (sectionGroup == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(item.SectionGroupId));
                        return methodResult;
                    }
                    var skillScore = new SkillScores
                    {
                        Skill = sectionGroup.CourseSkill,
                        TotalCount = questionCount,
                        CorrectCount = count,
                        Scores = count.GetIeltsScore(sectionGroup.CourseSkill),
                        CountQuestion = item.Answers.Count,
                        TotalQuestion = item.Answers.Count,
                    };
                    skillScores.Add(skillScore);
                }
            }

            if (mockTestAnswers.Any())
            {
                mockTestResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
                mockTestResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
                mockTestResult.Status = EnumResultStatus.Done;
                mockTestResult.SkillScores = skillScores;
            }

            await _mockTestAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                if (mockTestAnswers.Count > 0)
                {
                    await _requestSafeCachingService.SafeRequest<List<MockTestAnswer>>(
                        key: $"Add_MockTestAnswers_{string.Join("_", mockTestAnswers.Select(ma => $"{ma.SectionGroupResultId}_{ma.MockTestResultId}_{ma.IsDeleted}"))}",
                        safeFunction: async () =>
                        {
                            await _mockTestAnswerRepository.BulkMergeAsync(mockTestAnswers, bulk =>
                            {
                                bulk.ColumnPrimaryKeyExpression = entity => new { entity.SectionGroupResultId, entity.SectionId, entity.SectionTimeCodeId, entity.SectionQuestionId, entity.MockTestResultId, entity.IsDeleted };
                            });
                            return mockTestAnswers;
                        });
                }

                await _mockTestResultRepository.BulkUpdateList(new List<MockTestResult> { mockTestResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId, c.MockTestId, c.UnitId };
                });

                await _mockTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<MockTestResultModel>(mockTestResult);
                return methodResult;
            });

            return methodResult;
        }
    }
}
