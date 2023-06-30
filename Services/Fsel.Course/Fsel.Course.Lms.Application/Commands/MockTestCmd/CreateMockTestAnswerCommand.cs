// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.MockTestCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
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
        private readonly ISectionQuestionRepository _sectionQuestionRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly ISectionPartRepository _sectionPartRepository;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly IMockTestSectionRepository _mockTestSectionRepository;
        private readonly IMapper _mapper;

        public CreateMockTestAnswerCommandHandler(AnswerTypeConverter answerTypeConverter
            , IQuestionRepository questionRepository
            , IMockTestAnswerRepository mockTestAnswerRepository
            , IMockTestResultRepository mockTestResultRepository
            , ISectionQuestionRepository sectionQuestionRepository
            , ISectionGroupRepository sectionGroupRepository
            , ISectionRepository sectionRepository
            , ISectionPartRepository sectionPartRepository
            , IMockTestRepository mockTestRepository
            , IMockTestSectionRepository mockTestSectionRepository
            , IMapper mapper)
        {
            _answerTypeConverter = answerTypeConverter;
            _questionRepository = questionRepository;
            _mockTestAnswerRepository = mockTestAnswerRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _sectionQuestionRepository = sectionQuestionRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _sectionRepository = sectionRepository;
            _sectionPartRepository = sectionPartRepository;
            _mockTestRepository = mockTestRepository;
            _mockTestSectionRepository = mockTestSectionRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<MockTestResultModel>> Handle(CreateMockTestAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<MockTestResultModel> methodResult = new MethodResult<MockTestResultModel>();
            if (request.Skills == null || request.Skills.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestAnswerErrorCode.SkillNull), nameof(request.Skills), request.Skills);
                return methodResult;
            }
            var mockTestResult = await _mockTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.MockTestResultId, cancellationToken);
            if (mockTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestResultErrorCode.MockTestResultNotExist), nameof(request.MockTestResultId), request.MockTestResultId);
                return methodResult;
            }

            var mockTestAnswers = new List<MockTestAnswer>();
            var skillScores = new List<SkillScores>();
            int correctCountStudent = 0;

            foreach (var item in request.Skills)
            {
                if (item.Answers != null)
                {
                    if (item.Answers == null || item.Answers.Count == 0)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumMockTestAnswerErrorCode.AnswerNull), nameof(request.Skills), request.Skills);
                        return methodResult;
                    }
                    var questionIds = request.Skills.SelectMany(x => x.Answers).Select(x => x.QuestionId).ToList();
                    var questions = await _questionRepository.GetIncludeSectionByIdAsync(questionIds);
                    if (questions == null || questions.Count == 0)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionsNotExist), nameof(questionIds), questionIds);
                        return methodResult;
                    }
                    double count = 0;
                    foreach (var answer in item.Answers)
                    {
                        var question = questions.FirstOrDefault(x => x.Id == answer.QuestionId);
                        if (question == null)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionNotExist), nameof(answer.QuestionId), answer.QuestionId);
                            return methodResult;
                        }
                        else if (question.Config == null)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionConfigNull), nameof(question), question);
                            return methodResult;
                        }
                        else if (question.SectionQuestions == null || question.SectionQuestions.Count == 0)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumSectionQuestionErrorCode.SectionQuestionsNotExist), nameof(question.SectionQuestions));
                            return methodResult;
                        }
                        var sectionQuestionId = question.SectionQuestions.FirstOrDefault()!.Id;
                        var mockTestAnswer = await _mockTestAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.MockTestResultId == mockTestResult.Id && x.SectionQuestionId == sectionQuestionId, cancellationToken);
                        if (mockTestAnswer == null)
                        {
                            var (answerConfig, correctCount) = _answerTypeConverter.GetTotalCorrectByAsnwerType(answer.Answer, question.Config, question.QuestionType);
                            if (answerConfig == null)
                            {
                                methodResult.AddErrorBadRequest(nameof(EnumMockTestAnswerErrorCode.AnswerIsInTheWrongFormat), nameof(answer.Answer), answer.Answer);
                                return methodResult;
                            }
                            correctCountStudent += correctCount;
                            mockTestAnswer = new MockTestAnswer
                            {
                                Answer = answerConfig,
                                MockTestResultId = mockTestResult.Id,
                                SectionQuestionId = sectionQuestionId
                            };
                            mockTestAnswers.Add(mockTestAnswer);
                        }
                    }
                    var sectionGroup = await _sectionGroupRepository.GetByIdAsync(item.SectionGroupId);
                    if (sectionGroup == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSectionGroupErrorCode.SectionGroupsNull), nameof(sectionGroup));
                        return methodResult;
                    }
                    var skillScore = new SkillScores
                    {
                        Skill = sectionGroup.CourseSkill,
                        TotalCount = questions.Sum(x => x.CorrectTotal),
                        CorrectCount = count,
                        Scores = count.GetIeltsScore(sectionGroup.CourseSkill)
                    };
                    skillScores.Add(skillScore);
                }
            }
            IQueryable<int>? questionQuery = from p in _mockTestRepository.Queryable
                                             join ps in _mockTestSectionRepository.Queryable on p.Id equals ps.MockTestId
                                             join sg in _sectionGroupRepository.Queryable on ps.SectionGroupId equals sg.Id
                                             join s in _sectionRepository.Queryable on sg.Id equals s.SectionGroupId
                                             join sp in _sectionPartRepository.Queryable on s.Id equals sp.SectionId
                                             join sq in _sectionQuestionRepository.Queryable on sp.Id equals sq.SectionPartId
                                             join q in _questionRepository.Queryable on sq.QuestionId equals q.Id
                                             where p.Id == mockTestResult.MockTestId
                                             select q.CorrectTotal;
            var count1 = await questionQuery.SumAsync(cancellationToken);
            mockTestResult.CorrectCount = correctCountStudent;
            mockTestResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
            mockTestResult.Percent = (double)mockTestResult.CorrectCount / mockTestResult.CorrectTotal * 100;
            mockTestResult.Status = EnumResultStatus.Done;
            mockTestResult.SkillScores = skillScores;

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
