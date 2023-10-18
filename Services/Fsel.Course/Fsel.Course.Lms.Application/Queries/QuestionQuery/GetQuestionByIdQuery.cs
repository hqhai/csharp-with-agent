// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.QuestionQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetQuestionByIdQuery : IRequest<MethodResult<QuestionModel>>
    {
        public Guid ObjectResultId { get; set; }
        public Guid QuestionId { get; set; }
        public EnumTestResult Result { get; set; }
    }

    public class GetQuestionByIdQueryHandler : IRequestHandler<GetQuestionByIdQuery, MethodResult<QuestionModel>>
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

        public async Task<MethodResult<QuestionModel>> Handle(GetQuestionByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<QuestionModel>();
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = await GetQuestionAsync(request);
            return methodResult;
        }

        private async Task<QuestionModel?> GetQuestionAsync(GetQuestionByIdQuery request)
        {
            switch (request.Result)
            {
                case EnumTestResult.MockTest:
                    return await GetQuestionByMockTest(request);

                case EnumTestResult.FinalTest:
                    return await GetQuestionByFinalTest(request);

                case EnumTestResult.ExtraPratice:
                    return await GetQuestionByExtraPratice(request);

                default:
                    throw new InvalidOperationException();
            }
        }

        private async Task<QuestionModel?> GetQuestionByMockTest(GetQuestionByIdQuery request)
        {
            var mockTestResult = await _mockTestResultRepository.Queryable.Where(x => x.MockTestId == request.ObjectResultId).FirstOrDefaultAsync();

            var question = await _questionRepository.Queryable.Include(x => x.SectionQuestions)
                                        .ThenInclude(x => x.MockTestAnswers.Where(x => mockTestResult != null && x.MockTestResultId == mockTestResult.Id))
                                        .FirstOrDefaultAsync(x => x.Id == request.QuestionId);
            if (question == null)
            {
                return default;
            }
            var mockTestAnswer = question.SectionQuestions.SelectMany(x => x.MockTestAnswers).FirstOrDefault();
            var questionModel = GetQuestion(question, mockTestAnswer != null);
            questionModel.ResultAnswer = _mapper.Map<AnswerModel>(mockTestAnswer);
            return questionModel;
        }

        private async Task<QuestionModel?> GetQuestionByExtraPratice(GetQuestionByIdQuery request)
        {
            var extraPracticeResult = await _extraPracticeResultRepository.Queryable.Where(x => x.ExtraPracticeId == request.ObjectResultId).FirstOrDefaultAsync();

            var question = await _questionRepository.Queryable.Include(x => x.ExtraPracticeAnswers.Where(x => extraPracticeResult != null && x.ExtraPracticeResultId == extraPracticeResult.Id))
                                                              .FirstOrDefaultAsync(x => x.Id == request.QuestionId);
            if (question == null)
            {
                return default;
            }
            var extraPracticeAnswer = question.ExtraPracticeAnswers.FirstOrDefault();
            var questionModel = GetQuestion(question, extraPracticeAnswer != null);
            questionModel.ResultAnswer = _mapper.Map<AnswerModel>(extraPracticeAnswer);
            return questionModel;
        }

        private async Task<QuestionModel?> GetQuestionByFinalTest(GetQuestionByIdQuery request)
        {
            var finalTestResult = await _finalTestResultRepository.Queryable.Where(x => x.FinalTestId == request.ObjectResultId).FirstOrDefaultAsync();

            var question = await _questionRepository.Queryable.Include(x => x.SectionQuestions)
                                .ThenInclude(x => x.FinalTestAnswers.Where(x => finalTestResult != null && x.FinalTestResultId == finalTestResult.Id))
                                .FirstOrDefaultAsync(x => x.Id == request.QuestionId);
            if (question == null)
            {
                return default;
            }
            var finalTestAnswer = question.SectionQuestions.SelectMany(x => x.FinalTestAnswers).FirstOrDefault();
            var questionModel = GetQuestion(question, finalTestAnswer != null);
            questionModel.ResultAnswer = _mapper.Map<AnswerModel>(finalTestAnswer);
            return questionModel;
        }

        private QuestionModel GetQuestion(Question question, bool isShowAnswer = false)
        {
            return new QuestionModel()
            {
                Id = question.Id,
                QuestionType = question.QuestionType,
                CorrectTotal = question.CorrectTotal,
                Explanation = question.Explanation,
                Ungraded = question.Ungraded,
                Config = _questionTypeConverter.QuestionTypeConverterObject(question.Config, question.QuestionType, isDisableAnswers: !isShowAnswer).Item1,
            };
        }
    }
}
