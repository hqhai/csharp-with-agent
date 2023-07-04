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
            if (request.Answers == null || request.Answers.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestAnswerErrorCode.AnswerNull), nameof(request.Answers), request.Answers);
                return methodResult;
            }
            var mockTestResult = await _mockTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.MockTestResultId, cancellationToken);
            if (mockTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestResultErrorCode.MockTestResultNotExist), nameof(request.MockTestResultId), request.MockTestResultId);
                return methodResult;
            }

            var questionIds = request.Answers.Select(x => x.QuestionId).ToList();
            var questions = await _questionRepository.GetIncludeSectionByIdAsync(questionIds);
            if (questions == null || questions.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionsNotExist), nameof(questionIds), questionIds);
                return methodResult;
            }
            var mockTestAnswers = new List<MockTestAnswer>();
            int correctCountStudent = 0;

            foreach (var item in request.Answers)
            {
                if (item.Answer != null)
                {
                    var question = await _questionRepository.GetByIdAsync(item.QuestionId);
                    if (question == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionNotExist), nameof(item.QuestionId), item.QuestionId);
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
                        var (answerConfig, correctCount) = _answerTypeConverter.GetTotalCorrectByAsnwerType(item.Answer, question.Config, question.QuestionType);
                        if (answerConfig == null)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumMockTestAnswerErrorCode.AnswerIsInTheWrongFormat), nameof(item.Answer), item.Answer);
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
            }
                    var mockTestAnswer = await _mockTestAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.MockTestResultId == mockTestResult.Id && x.SectionQuestionId == sectionQuestionId, cancellationToken);
                    if (mockTestAnswer == null)
                    {
                        var (answerConfig, correctCount) = _answerTypeConverter.GetTotalCorrectByAsnwerType(item.Answer, question.Config, question.QuestionType);
                        if (answerConfig == null)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumMockTestAnswerErrorCode.AnswerIsInTheWrongFormat), nameof(item.Answer), item.Answer);
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
                else if (item.SectionTimeCodeId != null)
                {
                    var sectionTimeCode = await _sectionTimeCodeRepository.Queryable.FirstOrDefaultAsync(x => x.Id == item.SectionTimeCodeId!, cancellationToken);
                    if (sectionTimeCode == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSectionTimeCodeErrorCode.SectionTimeCodeNotExist), nameof(item.SectionTimeCodeId), item.SectionTimeCodeId);
                        return methodResult;
                    }
                    var mockTestAnswer = await _mockTestAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.MockTestResultId == mockTestResult.Id && x.SectionTimeCodeId == sectionTimeCode.Id, cancellationToken);
                    if (mockTestAnswer == null)
                    {
                        var (answerConfig, correctCount) = _answerTypeConverter.GetTotalCorrectByAsnwerType(item.Answer, null, EnumQuestionType.BaseContent);
                        if (answerConfig == null)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumMockTestAnswerErrorCode.AnswerIsInTheWrongFormat), nameof(item.Answer), item.Answer);
                            return methodResult;
                        }
                        mockTestAnswer = new MockTestAnswer
                        {
                            Answer = answerConfig,
                            MockTestResultId = mockTestResult.Id,
                            SectionTimeCodeId = sectionTimeCode.Id
                        };
                        mockTestAnswers.Add(mockTestAnswer);
                    }
                }
                else if (item.SectionId != null)
                {
                    var section = await _sectionRepository.Queryable.FirstOrDefaultAsync(x => x.Id == item.SectionId!, cancellationToken);
                    if (section == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSectionErrorCode.SectionNotExist), nameof(item.SectionId), item.SectionId);
                        return methodResult;
                    }
                    var mockTestAnswer = await _mockTestAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.MockTestResultId == mockTestResult.Id && x.SectionId == section.Id, cancellationToken);
                    if (mockTestAnswer == null)
                    {
                        var (answerConfig, correctCount) = _answerTypeConverter.GetTotalCorrectByAsnwerType(item.Answer, null, EnumQuestionType.BaseContent);
                        if (answerConfig == null)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumMockTestAnswerErrorCode.AnswerIsInTheWrongFormat), nameof(item.Answer), item.Answer);
                            return methodResult;
                        }
                        mockTestAnswer = new MockTestAnswer
                        {
                            Answer = answerConfig,
                            MockTestResultId = mockTestResult.Id,
                            SectionId = section.Id
                        };
                        mockTestAnswers.Add(mockTestAnswer);
                    }
                }
            }
            IQueryable<int>? questionQuery = null;

            questionQuery = from p in _mockTestRepository.Queryable
                            join ps in _mockTestSectionRepository.Queryable on p.Id equals ps.MockTestId
                            join sg in _sectionGroupRepository.Queryable on ps.SectionGroupId equals sg.Id
                            join s in _sectionRepository.Queryable on sg.Id equals s.SectionGroupId
                            join sp in _sectionPartRepository.Queryable on s.Id equals sp.SectionId
                            join sq in _sectionQuestionRepository.Queryable on sp.Id equals sq.SectionPartId
                            join q in _questionRepository.Queryable on sq.QuestionId equals q.Id
                            where p.Id == mockTestResult.MockTestId
                            select q.CorrectTotal;

            mockTestResult.CorrectCount = correctCountStudent;
            mockTestResult.CorrectTotal = await questionQuery.SumAsync(cancellationToken);
            mockTestResult.Percent = mockTestResult.CorrectTotal != 0 ? (double)mockTestResult.CorrectCount / mockTestResult.CorrectTotal * 100 : 0;
            mockTestResult.SkillScores = new List<SkillScores>();
            mockTestResult.Status = EnumResultStatus.Done;
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
