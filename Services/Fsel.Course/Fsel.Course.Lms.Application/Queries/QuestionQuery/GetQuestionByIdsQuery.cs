// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.QuestionQuery
{
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetQuestionByIdsQuery : IRequest<MethodResult<IList<QuestionModel>>>
    {
        public Guid ObjectResultId { get; set; }
        public IList<Guid>? QuestionIds { get; set; }
    }

    public class GetQuestionByIdQueryHandler : IRequestHandler<GetQuestionByIdsQuery, MethodResult<IList<QuestionModel>>>
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IExtraPracticeResultRepository _extraPracticeResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly IMapper _mapper;

        public GetQuestionByIdQueryHandler(IQuestionRepository questionRepository, IFinalTestResultRepository finalTestResultRepository, IExtraPracticeResultRepository extraPracticeResultRepository, IMockTestResultRepository mockTestResultRepository, QuestionTypeConverter questionTypeConverter, IMapper mapper)
        {
            _questionRepository = questionRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _extraPracticeResultRepository = extraPracticeResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _questionTypeConverter = questionTypeConverter;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<QuestionModel>>> Handle(GetQuestionByIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<QuestionModel>>();
            if (request.QuestionIds == null || !request.QuestionIds.Any())
            {
                return methodResult;
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = await GetQuestionAsync(request);
            return methodResult;
        }

        private async Task<IList<QuestionModel>?> GetQuestionAsync(GetQuestionByIdsQuery request)
        {
            return await GetQuestionByMockTest(request) ?? await GetQuestionByFinalTest(request) ?? await GetQuestionByExtraPratice(request);
        }

        private async Task<IList<QuestionModel>?> GetQuestionByMockTest(GetQuestionByIdsQuery request)
        {
            ArgumentNullException.ThrowIfNull(request.QuestionIds);
            var mockTestResult = await _mockTestResultRepository.Queryable.Where(x => x.Id == request.ObjectResultId).FirstOrDefaultAsync();
            if (mockTestResult == null)
            {
                return default;
            }
            var questions = await _questionRepository.Queryable.Include(x => x.SectionQuestions)
                                        .ThenInclude(x => x.MockTestAnswers.Where(x => mockTestResult != null && x.MockTestResultId == mockTestResult.Id))
                                        .Where(x => request.QuestionIds.Contains(x.Id)).ToListAsync();
            if (questions == null || !questions.Any())
            {
                return default;
            }
            return questions.Select(x => GetQuestionByMockTest(x, mockTestResult?.Status == EnumResultStatus.Done)).ToList();
        }

        private async Task<IList<QuestionModel>?> GetQuestionByExtraPratice(GetQuestionByIdsQuery request)
        {
            ArgumentNullException.ThrowIfNull(request.QuestionIds);
            var extraPracticeResult = await _extraPracticeResultRepository.Queryable.Where(x => x.Id == request.ObjectResultId).FirstOrDefaultAsync();
            if (extraPracticeResult == null)
            {
                return default;
            }
            var questions = await _questionRepository.Queryable.Include(x => x.ExtraPracticeAnswers.Where(x => extraPracticeResult != null && x.ExtraPracticeResultId == extraPracticeResult.Id))
                                                              .Where(x => request.QuestionIds.Contains(x.Id)).ToListAsync();
            if (questions == null || !questions.Any())
            {
                return default;
            }
            return questions.Select(x => GetQuestionByExtraPractice(x, extraPracticeResult?.Status == EnumResultStatus.Done)).ToList();
        }

        private async Task<IList<QuestionModel>?> GetQuestionByFinalTest(GetQuestionByIdsQuery request)
        {
            ArgumentNullException.ThrowIfNull(request.QuestionIds);
            var finalTestResult = await _finalTestResultRepository.Queryable.Where(x => x.Id == request.ObjectResultId).FirstOrDefaultAsync();
            if (finalTestResult == null)
            {
                return default;
            }
            var questions = await _questionRepository.Queryable.Include(x => x.SectionQuestions)
                                .ThenInclude(x => x.FinalTestAnswers.Where(x => finalTestResult != null && x.FinalTestResultId == finalTestResult.Id))
                                .Where(x => request.QuestionIds.Contains(x.Id)).ToListAsync();
            if (questions == null || !questions.Any())
            {
                return default;
            }
            return questions.Select(x => GetQuestionByFinalTest(x, finalTestResult?.Status == EnumResultStatus.Done)).ToList();
        }

        private QuestionModel GetQuestion(Question question, bool isShowAnswer = false)
        {
            var questionModel = _mapper.Map<QuestionModel>(question);
            questionModel.Config = _questionTypeConverter.QuestionTypeConverterObject(question.Config, question.QuestionType, isDisableAnswers: !isShowAnswer).Item1;
            return questionModel;
        }

        private QuestionModel GetQuestionByMockTest(Question question, bool isShowAnswer = false)
        {
            var questionModel = GetQuestion(question, isShowAnswer);
            questionModel.ResultAnswer = _mapper.Map<AnswerModel>(question.SectionQuestions.SelectMany(x => x.MockTestAnswers).FirstOrDefault());
            return questionModel;
        }

        private QuestionModel GetQuestionByFinalTest(Question question, bool isShowAnswer = false)
        {
            var questionModel = GetQuestion(question, isShowAnswer);
            questionModel.ResultAnswer = _mapper.Map<AnswerModel>(question.SectionQuestions.SelectMany(x => x.FinalTestAnswers).FirstOrDefault());
            return questionModel;
        }

        private QuestionModel GetQuestionByExtraPractice(Question question, bool isShowAnswer = false)
        {
            var questionModel = GetQuestion(question, isShowAnswer);
            questionModel.ResultAnswer = _mapper.Map<AnswerModel>(question.ExtraPracticeAnswers.FirstOrDefault());
            return questionModel;
        }
    }
}
