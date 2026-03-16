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
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetQuestionByIdsQuery : IRequest<MethodResult<IList<QuestionModel>>>
    {
        public Guid ObjectResultId { get; set; }

        public IList<string> QuestionIds { get; set; } = new List<string>();

        [JsonIgnore]
        public IList<Guid> ListQuestionIds
        { get { return QuestionIds.ToList<Guid>(); } }
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
        private readonly IQuestionShuffleRepository _questionShuffleRepository;

        public GetQuestionByIdQueryHandler(IQuestionRepository questionRepository,
            IPlacementTestResultRepository placementTestResultRepository,
            AnswerTypeConverter answerTypeConverter,
            IFinalTestResultRepository finalTestResultRepository,
            IExtraPracticeResultRepository extraPracticeResultRepository,
            IMockTestResultRepository mockTestResultRepository,
            QuestionTypeConverter questionTypeConverter,
            IMapper mapper,
            IQuestionShuffleRepository questionShuffleRepository)
        {
            _questionRepository = questionRepository;
            _placementTestResultRepository = placementTestResultRepository;
            _answerTypeConverter = answerTypeConverter;
            _finalTestResultRepository = finalTestResultRepository;
            _extraPracticeResultRepository = extraPracticeResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _questionTypeConverter = questionTypeConverter;
            _mapper = mapper;
            _questionShuffleRepository = questionShuffleRepository;
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
                                        .Include(x => x.SectionQuestions)
                                            .ThenInclude(x => x.SectionPart)
                                        .Where(x => request.ListQuestionIds.Contains(x.Id))
                                        .OrderBy(x => x.CreatedDate)
                                        .ToListAsync();
            if (questions == null || !questions.Any())
            {
                return default;
            }
            return await GetQuestionsAsync(questions, mockTestResult.StudentId, mockTestResult.Status);
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
                                                              .Where(x => request.ListQuestionIds.Contains(x.Id)).OrderBy(x => x.CreatedDate).ToListAsync();
            if (questions == null || !questions.Any())
            {
                return default;
            }
            return await GetQuestionsAsync(questions, extraPracticeResult.StudentId, extraPracticeResult.Status);
        }

        private async Task<IList<QuestionModel>?> GetQuestionByFinalTest(GetQuestionByIdsQuery request)
        {
            ArgumentNullException.ThrowIfNull(request.ListQuestionIds);
            var finalTestResult = await _finalTestResultRepository.GetByIdAsync(request.ObjectResultId);
            if (finalTestResult == null)
            {
                return default;
            }
            var questions = await _questionRepository.Queryable
                                .Include(x => x.SectionQuestions)
                                .ThenInclude(x => x.FinalTestAnswers.Where(x => finalTestResult != null && x.FinalTestResultId == finalTestResult.Id))
                                .Where(x => request.ListQuestionIds.Contains(x.Id)).OrderBy(x => x.CreatedDate).ToListAsync();
            if (questions == null || !questions.Any())
            {
                return default;
            }
            return await GetQuestionsAsync(questions, finalTestResult.StudentId, finalTestResult.Status);
        }

        private async Task<IList<QuestionModel>?> GetQuestionByPlacementTest(GetQuestionByIdsQuery request)
        {
            ArgumentNullException.ThrowIfNull(request.ListQuestionIds);
            var placementTestResult = await _placementTestResultRepository.GetByIdAsync(request.ObjectResultId);
            if (placementTestResult == null)
            {
                return default;
            }
            var questions = await _questionRepository.Queryable
                                .Include(x => x.SectionQuestions)
                                .ThenInclude(x => x.PlacementTestAnswers.Where(x => placementTestResult != null && x.PlacementTestResultId == placementTestResult.Id))
                                .Where(x => request.ListQuestionIds.Contains(x.Id)).OrderBy(x => x.CreatedDate).ToListAsync();
            if (questions == null || !questions.Any())
            {
                return default;
            }
            return await GetQuestionsAsync(questions, placementTestResult.StudentId, placementTestResult.Status);
        }

        private async Task<IList<QuestionModel>> GetQuestionsAsync(IList<Question>? questions, Guid studentId, EnumResultStatus status)
        {
            var listQuestion = new List<QuestionModel>();
            if (questions == null || !questions.Any())
            {
                return new List<QuestionModel>();
            }
            var listQuestionShuffle = new List<QuestionShuffle>();
            var questionShuffles = await _questionShuffleRepository.Queryable.Where(x => questions.Select(x => x!.Id).Contains(x.QuestionId) && x.StudentId == studentId).ToListAsync();
            foreach (var question in questions)
            {
                bool isShowAnswer = status == EnumResultStatus.Done;
                var answer = GetAnswer(question);
                var questionModel = _mapper.Map<QuestionModel>(question);

                questionModel.Config = _questionTypeConverter.QuestionTypeConverterObject(question.Config, question.QuestionType, isDisableAnswers: !isShowAnswer).Item1;

                var questionShuffle = questionShuffles.FirstOrDefault(x => x.QuestionId == question.Id);
                (questionModel.Config, string? questionShuffleStr) = _questionTypeConverter.QuestionShuffleConverterObject(questionModel.Config, question.QuestionType, isShowAnswer, questionShuffle?.ShuffleConfigs);

                if (!string.IsNullOrEmpty(questionShuffleStr) && (questionShuffle == null || questionShuffle.ShuffleConfigStr != questionShuffleStr))
                {
                    if (questionShuffle == null)
                    {
                        questionShuffle = new QuestionShuffle
                        {
                            QuestionId = question.Id,
                            StudentId = studentId,
                            ShuffleConfigStr = questionShuffleStr
                        };
                    }
                    else
                    {
                        questionShuffle.ShuffleConfigStr = questionShuffleStr;
                    }
                    listQuestionShuffle.Add(questionShuffle);
                }

                questionModel.CorrectStatus = GetCorrectStatus(_mapper.Map<BaseAnswer>(answer));
                questionModel.SectionId = question.SectionQuestions.Any() ? question.SectionQuestions.Select(x => x.SectionId ?? x.SectionPart?.SectionId).FirstOrDefault() : default;
                if (answer != null)
                {
                    var answerDto = _mapper.Map<AnswerModel>(answer);
                    answerDto.Answer = _answerTypeConverter.AnswerTypeConverterObject(answerDto.Answer, question.QuestionType, false, status, isShowAnswer);
                    if (!isShowAnswer)
                    {
                        answerDto.Status = EnumAnswerStatus.Process;
                        answerDto.CorrectCount = default;
                        answerDto.IsCorrect = null;
                    }
                    questionModel.ResultAnswer = answerDto;
                }
                listQuestion.Add(questionModel);
            }

            await _questionShuffleRepository.SaveQuestionShufflesAsync(listQuestionShuffle);
            return listQuestion;
        }

        private static object? GetAnswer(Question question)
        {
            var finalAnswer = question.SectionQuestions.SelectMany(x => x.FinalTestAnswers).FirstOrDefault();
            if (finalAnswer != null)
            {
                return finalAnswer;
            }
            var mockAnswer = question.SectionQuestions.SelectMany(x => x.MockTestAnswers).FirstOrDefault();
            if (mockAnswer != null)
            {
                return mockAnswer;
            }
            var placementAnswer = question.SectionQuestions.SelectMany(x => x.PlacementTestAnswers).FirstOrDefault();
            if (placementAnswer != null)
            {
                return placementAnswer;
            }
            return null;
        }

        private static EnumCorrectStatus? GetCorrectStatus(BaseAnswer? answer)
        {
            EnumCorrectStatus? status = null;
            if (answer != null)
            {
                status = EnumCorrectStatus.Process;
                if (answer.Status == EnumAnswerStatus.Done)
                {
                    status = answer.IsCorrect.HasValue && answer.IsCorrect.Value ? EnumCorrectStatus.Correct : EnumCorrectStatus.Fail;
                }
            }
            return status;
        }
    }
}
