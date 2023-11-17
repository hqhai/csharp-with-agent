// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.QuestionQuery
{
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Text.Json.Serialization;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetQuestionByIdsQuery : IRequest<MethodResult<IList<QuestionModel>>>
    {
        public Guid ObjectResultId { get; set; }

        public IList<string>? QuestionIds { get; set; }

        [JsonIgnore]
        public IList<Guid> ListQuestionIds { get { return QuestionIds.ToList<Guid>(); } }
    }

    public class GetQuestionByIdQueryHandler : IRequestHandler<GetQuestionByIdsQuery, MethodResult<IList<QuestionModel>>>
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly AnswerTypeConverter _answerTypeConverter;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IExtraPracticeResultRepository _extraPracticeResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly IMapper _mapper;

        public GetQuestionByIdQueryHandler(IQuestionRepository questionRepository, IPlacementTestResultRepository placementTestResultRepository, AnswerTypeConverter answerTypeConverter, IFinalTestResultRepository finalTestResultRepository, IExtraPracticeResultRepository extraPracticeResultRepository, IMockTestResultRepository mockTestResultRepository, QuestionTypeConverter questionTypeConverter, IMapper mapper)
        {
            _questionRepository = questionRepository;
            _placementTestResultRepository = placementTestResultRepository;
            _answerTypeConverter = answerTypeConverter;
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
            if (request.ListQuestionIds == null || !request.ListQuestionIds.Any())
            {
                return methodResult;
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = await GetQuestionAsync(request);
            return methodResult;
        }

        private async Task<IList<QuestionModel>?> GetQuestionAsync(GetQuestionByIdsQuery request)
        {
            return await GetQuestionByMockTest(request) ?? await GetQuestionByFinalTest(request) ?? await GetQuestionByPlacementTest(request) ?? await GetQuestionByExtraPratice(request);
        }

        private async Task<IList<QuestionModel>?> GetQuestionByMockTest(GetQuestionByIdsQuery request)
        {
            ArgumentNullException.ThrowIfNull(request.ListQuestionIds);
            var mockTestResult = await _mockTestResultRepository.GetByIdAsync(request.ObjectResultId);
            if (mockTestResult == null)
            {
                return default;
            }
            var questions = await _questionRepository.Queryable.Include(x => x.SectionQuestions)
                                        .ThenInclude(x => x.MockTestAnswers.Where(x => mockTestResult != null && x.MockTestResultId == mockTestResult.Id))
                                        .Where(x => request.ListQuestionIds.Contains(x.Id)).ToListAsync();
            if (questions == null || !questions.Any())
            {
                return default;
            }
            return questions.Select(x => GetQuestion(x, mockTestResult?.Status == EnumResultStatus.Done, x.SectionQuestions.SelectMany(n => n.MockTestAnswers).FirstOrDefault())).ToList();
        }

        private async Task<IList<QuestionModel>?> GetQuestionByExtraPratice(GetQuestionByIdsQuery request)
        {
            ArgumentNullException.ThrowIfNull(request.ListQuestionIds);
            var extraPracticeResult = await _extraPracticeResultRepository.GetByIdAsync(request.ObjectResultId);
            if (extraPracticeResult == null)
            {
                return default;
            }
            var questions = await _questionRepository.Queryable.Include(x => x.ExtraPracticeAnswers.Where(x => extraPracticeResult != null && x.ExtraPracticeResultId == extraPracticeResult.Id))
                                                              .Where(x => request.ListQuestionIds.Contains(x.Id)).ToListAsync();
            if (questions == null || !questions.Any())
            {
                return default;
            }
            return questions.Select(x => GetQuestion(x, extraPracticeResult?.Status == EnumResultStatus.Done, x.ExtraPracticeAnswers.FirstOrDefault())).ToList();
        }

        private async Task<IList<QuestionModel>?> GetQuestionByFinalTest(GetQuestionByIdsQuery request)
        {
            ArgumentNullException.ThrowIfNull(request.ListQuestionIds);
            var finalTestResult = await _finalTestResultRepository.GetByIdAsync(request.ObjectResultId);
            if (finalTestResult == null)
            {
                return default;
            }
            var questions = await _questionRepository.Queryable.Include(x => x.SectionQuestions).ThenInclude(x => x.Section)
                                .Include(x => x.SectionQuestions)
                                .ThenInclude(x => x.FinalTestAnswers.Where(x => finalTestResult != null && x.FinalTestResultId == finalTestResult.Id))
                                .Where(x => request.ListQuestionIds.Contains(x.Id)).ToListAsync();
            if (questions == null || !questions.Any())
            {
                return default;
            }
            return questions.Select(x => GetQuestion(x, finalTestResult?.Status == EnumResultStatus.Done, x.SectionQuestions.SelectMany(n => n.FinalTestAnswers).FirstOrDefault())).ToList();
        }

        private async Task<IList<QuestionModel>?> GetQuestionByPlacementTest(GetQuestionByIdsQuery request)
        {
            ArgumentNullException.ThrowIfNull(request.ListQuestionIds);
            var placementTestResult = await _placementTestResultRepository.GetByIdAsync(request.ObjectResultId);
            if (placementTestResult == null)
            {
                return default;
            }
            var questions = await _questionRepository.Queryable.Include(x => x.SectionQuestions).ThenInclude(x => x.Section)
                                .Include(x => x.SectionQuestions)
                                .ThenInclude(x => x.PlacementTestAnswers.Where(x => placementTestResult != null && x.PlacementTestResultId == placementTestResult.Id))
                                .Where(x => request.ListQuestionIds.Contains(x.Id)).ToListAsync();
            if (questions == null || !questions.Any())
            {
                return default;
            }
            return questions.Select(x => GetQuestion(x, placementTestResult?.Status == EnumResultStatus.Done, x.SectionQuestions.SelectMany(n => n.PlacementTestAnswers).FirstOrDefault())).ToList();
        }

        private QuestionModel GetQuestion(Question question, bool isShowAnswer = false, object? answer = null)
        {
            var questionModel = _mapper.Map<QuestionModel>(question);
            questionModel.Config = _questionTypeConverter.QuestionTypeConverterObject(question.Config, question.QuestionType, isDisableAnswers: !isShowAnswer).Item1;
            questionModel.SectionId = question.SectionQuestions.Any() ? question.SectionQuestions.Select(x => x.SectionId ?? x.SectionPart?.SectionId).FirstOrDefault() : default;
            if (answer != null)
            {
                var answerDto = _mapper.Map<AnswerModel>(answer);
                answerDto.Answer = _answerTypeConverter.AnswerTypeConverterObject(answerDto.Answer, question.QuestionType, !isShowAnswer);
                questionModel.ResultAnswer = answerDto;
            }
            return questionModel;
        }
    }
}