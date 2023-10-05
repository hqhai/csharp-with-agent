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
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.MockTestAnswers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateMockTestAnswerCommand : CreateMockTestAnswerCommandModel, IRequest<MethodResult<MockTestResultModel>>
    {
    }

    public class CreateMockTestAnswerCommandHandler : IRequestHandler<CreateMockTestAnswerCommand, MethodResult<MockTestResultModel>>
    {
        private readonly AnswerTypeConverter _answerTypeConverter;
        private readonly IQuestionRepository _questionRepository;
        private readonly IMockTestAnswerRepository _mockTestAnswerRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly ISectionTimeCodeRepository _sectionTimeCodeRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly IMapper _mapper;

        public CreateMockTestAnswerCommandHandler(AnswerTypeConverter answerTypeConverter
            , IQuestionRepository questionRepository
            , IMockTestAnswerRepository mockTestAnswerRepository
            , IMockTestResultRepository mockTestResultRepository
            , ISectionRepository sectionRepository
            , ISectionTimeCodeRepository sectionTimeCodeRepository
            , ISectionGroupRepository sectionGroupRepository
            , IMapper mapper)
        {
            _answerTypeConverter = answerTypeConverter;
            _questionRepository = questionRepository;
            _mockTestAnswerRepository = mockTestAnswerRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _sectionRepository = sectionRepository;
            _sectionTimeCodeRepository = sectionTimeCodeRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _mapper = mapper;
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
                methodResult.AddErrorBadRequest(nameof(EnumMockTestResultErrorCode.MockTestResultDone), nameof(mockTestResult.Status));
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
            if (questions == null || questions.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questions));
                return methodResult;
            }
            foreach (var item in request.SectionGroups)
            {
                if (item.Answers != null)
                {
                    if (item.Answers == null || item.Answers.Count == 0)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.SectionGroups));
                        return methodResult;
                    }

                    double count = 0;
                    double questionCount = 0;
                    foreach (var answer in item.Answers)
                    {
                        if (answer.QuestionId != null)
                        {
                            var question = questions.FirstOrDefault(x => x.Id == answer.QuestionId);
                            if (question == null)
                            {
                                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(question));
                                return methodResult;
                            }
                            else if (question.Config == null)
                            {
                                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(question));
                                return methodResult;
                            }
                            else if (question.SectionQuestions == null || question.SectionQuestions.Count == 0)
                            {
                                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(question.SectionQuestions));
                                return methodResult;
                            }
                            var sectionQuestionId = question.SectionQuestions.FirstOrDefault()!.Id;
                            var mockTestAnswer = await _mockTestAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.MockTestResultId == mockTestResult.Id && x.SectionQuestionId == sectionQuestionId, cancellationToken);
                            if (mockTestAnswer == null)
                            {
                                var (answerConfig, correctCount) = _answerTypeConverter.GetTotalCorrectByAsnwerType(answer.Answer, question.Config, question.QuestionType);
                                if (!string.IsNullOrEmpty(answer.Answer?.ToString()) && answerConfig == null)
                                {
                                    methodResult.AddErrorBadRequest(nameof(EnumMockTestAnswerErrorCode.AnswerIsInTheWrongFormat), nameof(answer.Answer), answer.Answer);
                                    return methodResult;
                                }
                                count += correctCount;
                                questionCount += question.CorrectTotal;
                                mockTestAnswer = new MockTestAnswer
                                {
                                    Answer = answerConfig,
                                    MockTestResultId = mockTestResult.Id,
                                    SectionQuestionId = sectionQuestionId
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
                                    SectionId = sectionId
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
                                    SectionTimeCodeId = sectionTimeCodeId
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
                        Percent = questionCount > 0 ? NumberHelper.ConvertPercentDouble((double)count / questionCount) : default
                    };
                    skillScores.Add(skillScore);
                }
            }
            if (mockTestAnswers.Count > 0)
            {
                mockTestResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
                mockTestResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
                mockTestResult.Percent = (int)skillScores.Sum(x => x.TotalCount) > 0 ? NumberHelper.ConvertPercentDouble(skillScores.Sum(x => x.CorrectCount) / skillScores.Sum(x => x.TotalCount)) : default;
                mockTestResult.Status = EnumResultStatus.Done;
                mockTestResult.SkillScores = skillScores;
            }

            await _mockTestAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                if (mockTestAnswers.Count > 0)
                {
                    await _mockTestAnswerRepository.AddList(mockTestAnswers);
                    await _mockTestAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }

                _mockTestResultRepository.Update(mockTestResult);
                await _mockTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<MockTestResultModel>(mockTestResult);
                return methodResult;
            });

            return methodResult;
        }
    }
}
