// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.PlacementTestCmd.V1i1
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.PlacementTestAnswers;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Shared.ApplicationServices.CacheServices;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class SavePlacementTestAnswersCommand : CreatePlacementTestAnswerBySectionGroupCommandModel, IRequest<bool>
    {
    }

    public class SavePlacementTestAnswersCommandHandler : IRequestHandler<SavePlacementTestAnswersCommand, bool>
    {
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IPlacementTestAnswerRepository _placementTestAnswerRepository;
        private readonly QuestionConverter _questionConverter;
        private readonly ILogger<SavePlacementTestAnswersCommandHandler> _logger;
        private readonly AnswerTypeConverter _answerTypeConverter;
        private readonly SectionGroupConverter _sectionGroupConverter;
        private readonly ISectionQuestionRepository _sectionQuestionRepository;
        private readonly IRequestSafeCachingService _requestSafeCachingService;

        public SavePlacementTestAnswersCommandHandler(ISectionGroupResultRepository sectionGroupResultRepository,
            IQuestionRepository questionRepository,
            IPlacementTestAnswerRepository placementTestAnswerRepository,
            QuestionConverter questionConverter,
            ILogger<SavePlacementTestAnswersCommandHandler> logger,
            AnswerTypeConverter answerTypeConverter,
            SectionGroupConverter sectionGroupConverter,
            ISectionQuestionRepository sectionQuestionRepository,
            IRequestSafeCachingService requestSafeCachingService)
        {
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _questionRepository = questionRepository;
            _placementTestAnswerRepository = placementTestAnswerRepository;
            _questionConverter = questionConverter;
            _logger = logger;
            _answerTypeConverter = answerTypeConverter;
            _sectionGroupConverter = sectionGroupConverter;
            _sectionQuestionRepository = sectionQuestionRepository;
            _requestSafeCachingService = requestSafeCachingService;
        }

        public async Task<bool> Handle(SavePlacementTestAnswersCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var sectionGroupResult = await _sectionGroupResultRepository.Queryable.Include(x => x.SectionGroup).Where(x => x.SectionGroupId == request.SectionGroupId && x.PlacementTestResultId == request.PlacementTestResultId).FirstOrDefaultAsync(cancellationToken);
            if (sectionGroupResult == null || sectionGroupResult.SectionGroup == null)
            {
                return false;
            }
            if (request.Answers != null && request.Answers.Any())
            {
                var answerResult = await CreateAnswerAsync(request, sectionGroupResult);
                if (!answerResult.IsOK)
                {
                    return false;
                }
            }
            if (request.IsSubmit)
            {
                await UpdateUnansweredQuestions(sectionGroupResult.SectionGroup, sectionGroupResult);
            }
            return true;
        }

        public async Task UpdateUnansweredQuestions(SectionGroup sectionGroup, SectionGroupResult sectionGroupResult, double version = (int)EnumVersion.V1)
        {
            var questionIds = await _sectionGroupConverter.GetUnansweredQuestionIds(sectionGroup, sectionGroupResult, version);
            if (questionIds == null || !questionIds.Any())
            {
                return;
            }
            try
            {
                var questions = await _sectionQuestionRepository.Queryable.WhereBulkContains(questionIds, x => x.QuestionId).Select(x => new
                {
                    SectionQuestionId = x.Id,
                    QuestionType = x.Question!.QuestionType
                }).ToListAsync();

                var placementTestAnswers = questions.Select(x => new PlacementTestAnswer
                {
                    Answer = _answerTypeConverter.GetConfigEmpty(x.QuestionType),
                    SectionQuestionId = x.SectionQuestionId,
                    SectionGroupResultId = sectionGroupResult.Id,
                    PlacementTestResultId = sectionGroupResult.PlacementTestResultId ?? default,
                    IsCorrect = null,
                    Status = EnumAnswerStatus.Done
                }).ToList();
                if (placementTestAnswers.Any())
                {
                    var firstAnswer = placementTestAnswers.First();
                    await _requestSafeCachingService.SafeRequest<List<PlacementTestAnswer>>(
                        key: $"Add_PlacementTestAnswers_{firstAnswer.SectionGroupResultId}_{firstAnswer.PlacementTestResultId}_{firstAnswer.IsDeleted}",
                        safeFunction: async () =>
                        {
                            await _placementTestAnswerRepository.BulkMergeAsync(placementTestAnswers, bulk =>
                            {
                                bulk.ColumnPrimaryKeyExpression = entity => new { entity.SectionGroupResultId, entity.SectionQuestionId, entity.PlacementTestResultId, entity.IsDeleted };
                            });
                            return placementTestAnswers;
                        });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Log Duplicate PlacementTestAnswer : {ex.Message}");
            }
        }

        private async Task<MethodResult<IList<PlacementTestAnswer>>> CreateAnswerAsync(SavePlacementTestAnswersCommand request, SectionGroupResult sectionGroupResult)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            var methodResult = new MethodResult<IList<PlacementTestAnswer>>();
            var questionIds = request.Answers.Select(x => x.QuestionId).ToList();
            var questions = await _questionRepository.GetIncludeSectionByIdAsync(questionIds);
            if (questions == null || !questions.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questions));
                return methodResult;
            }
            var anwserResult = await CreateAnswer(request, questions, sectionGroupResult);
            if (!anwserResult.IsOK)
            {
                methodResult.AddErrorBadRequest(anwserResult.ErrorMessages);
                return methodResult;
            }
            var (createPlacementTestAnswers, updatePlacementTestAnswers) = anwserResult.Result;

            try
            {
                if (createPlacementTestAnswers != null && createPlacementTestAnswers.Any())
                {
                    var firstAnswer = createPlacementTestAnswers.First();
                    await _requestSafeCachingService.SafeRequest<List<PlacementTestAnswer>>(
                        key: $"Add_PlacementTestAnswers_{firstAnswer.SectionGroupResultId}_{firstAnswer.PlacementTestResultId}_{firstAnswer.IsDeleted}",
                        safeFunction: async () =>
                        {
                            await _placementTestAnswerRepository.BulkMergeAsync(createPlacementTestAnswers, bulk =>
                            {
                                bulk.ColumnPrimaryKeyExpression = entity => new { entity.SectionGroupResultId, entity.SectionQuestionId, entity.PlacementTestResultId, entity.IsDeleted };
                            });
                            return createPlacementTestAnswers.ToList();
                        });
                }
                if (updatePlacementTestAnswers != null && updatePlacementTestAnswers.Any())
                {
                    await _placementTestAnswerRepository.BulkUpdateList(updatePlacementTestAnswers, bulk =>
                    {
                        bulk.IgnoreOnUpdateExpression = entity => new { entity.PlacementTestResultId, entity.SectionQuestionId, entity.SectionGroupResultId };
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Log Duplicate PlacementTestAnswer : {ex.Message}");
            }
            return methodResult;
        }

        private async Task<MethodResult<(IList<PlacementTestAnswer>, IList<PlacementTestAnswer>)>> CreateAnswer(SavePlacementTestAnswersCommand request, IList<Question>? questions, SectionGroupResult sectionGroupResult)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            ArgumentNullException.ThrowIfNull(questions);
            var methodResult = new MethodResult<(IList<PlacementTestAnswer>, IList<PlacementTestAnswer>)>();
            var createPlacementTestAnswers = new List<PlacementTestAnswer>();
            var updatePlacementTestAnswers = new List<PlacementTestAnswer>();
            if (questions != null && questions.Any())
            {
                var placementTestAnswers = await _placementTestAnswerRepository.Queryable.Where(x => x.SectionGroupResultId == sectionGroupResult.Id)
                                                                               .Where(x => x.CreatedDate >= sectionGroupResult.CreatedDate)
                                                                               .ToListAsync();
                foreach (var item in request.Answers)
                {
                    var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                    var questionResult = _questionConverter.HandleAnswerTest(question, item.Answer, request.IsSubmit);
                    if (!questionResult.IsOK)
                    {
                        methodResult.AddErrorBadRequest(questionResult.ErrorMessages);
                        return methodResult;
                    }
                    var (questionItem, answerConfig, correctCount, isAnswered) = questionResult.Result;

                    var sectionQuestionId = questionItem.SectionQuestions.FirstOrDefault()?.Id ?? default;
                    var placementTestAnswer = placementTestAnswers.FirstOrDefault(x => x.SectionQuestionId == sectionQuestionId);
                    if (placementTestAnswer == null)
                    {
                        placementTestAnswer = new PlacementTestAnswer
                        {
                            PlacementTestResultId = request.PlacementTestResultId,
                            SectionGroupResultId = sectionGroupResult.Id,
                            SectionQuestionId = questionItem.SectionQuestions.FirstOrDefault()?.Id ?? default,
                        };
                        createPlacementTestAnswers.Add(GetPlacementTestAnswer(placementTestAnswer, answerConfig, correctCount, isAnswered, questionItem));
                    }
                    else
                    {
                        updatePlacementTestAnswers.Add(GetPlacementTestAnswer(placementTestAnswer, answerConfig, correctCount, isAnswered, questionItem));
                    }
                }
            }

            methodResult.Result = (createPlacementTestAnswers, updatePlacementTestAnswers);
            return methodResult;
        }

        private static PlacementTestAnswer GetPlacementTestAnswer(PlacementTestAnswer placementTestAnswer, object? answer, short correctCount, bool isAnswered, Question questionItem)
        {
            placementTestAnswer.Answer = answer;
            placementTestAnswer.CorrectCount = correctCount;
            placementTestAnswer.IsCorrect = isAnswered ? correctCount == questionItem.CorrectTotal : null;
            placementTestAnswer.Status = questionItem.CorrectTotal == correctCount ? EnumAnswerStatus.Done : EnumAnswerStatus.Process;
            return placementTestAnswer;
        }
    }
}
