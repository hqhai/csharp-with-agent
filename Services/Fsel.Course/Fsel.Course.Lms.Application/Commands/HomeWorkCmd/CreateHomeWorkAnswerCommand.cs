// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.HomeWorkCmd
{
    using System.Linq;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.HomeWorkAnswers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.ApplicationServices.CacheServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateHomeWorkAnswerCommand : CreateHomeWorkAnswerCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateHomeWorkAnswerCommandHandler : IRequestHandler<CreateHomeWorkAnswerCommand, MethodResult<bool>>
    {
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IHomeWorkAnswerRepository _homeWorkAnswerRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly QuestionConverter _questionConverter;
        private readonly IQuestionRepository _questionRepository;
        private readonly AuthContext _authContext;
        private const int FIFTY_PERCENT_DONE = 50;
        private readonly QuestBoardPublisher _questBoardPublisher;
        private readonly IUserService _userService;
        private readonly IRequestSafeCachingService _requestSafeCachingService;

        public CreateHomeWorkAnswerCommandHandler(IHomeWorkResultRepository homeWorkResultRepository,
            IHomeWorkAnswerRepository homeWorkAnswerRepository,
            IHomeWorkRepository homeWorkRepository,
            QuestionConverter questionConverter,
            IQuestionRepository questionRepository,
            AuthContext authContext,
            QuestBoardPublisher questBoardPublisher,
            IUserService userService,
            IRequestSafeCachingService requestSafeCachingService)
        {
            _homeWorkResultRepository = homeWorkResultRepository;
            _homeWorkAnswerRepository = homeWorkAnswerRepository;
            _homeWorkRepository = homeWorkRepository;
            _questionConverter = questionConverter;
            _questionRepository = questionRepository;
            _authContext = authContext;
            _questBoardPublisher = questBoardPublisher;
            _userService = userService;
            _requestSafeCachingService = requestSafeCachingService;
        }

        public async Task<MethodResult<bool>> Handle(CreateHomeWorkAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.Answers);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

            var homeWorkResult = await _homeWorkResultRepository.Queryable.Include(x => x.HomeWork).FirstOrDefaultAsync(x => x.Id == request.HomeWorkResultId, cancellationToken);
            if (homeWorkResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWorkResult));
                return methodResult;
            }
            else if (homeWorkResult.Status == EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusDone));
                return methodResult;
            }
            else if (homeWorkResult.Status == EnumResultStatus.Unfinished)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusUnfinished));
                return methodResult;
            }
            if (homeWorkResult.HomeWork == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWorkResult.HomeWork));
                return methodResult;
            }

            var questionIds = request.Answers.Select(x => x.QuestionId).Distinct().ToList();
            var questions = await _questionRepository.GetIncludeByHomeWorkAsync(questionIds);
            var homeWorkAnswers = new List<HomeWorkAnswer>();
            int correctTotal = default;
            foreach (var item in request.Answers)
            {
                var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                var questionResult = _questionConverter.HandleQuestionAnswer(question, item.Answer, true);
                if (!questionResult.IsOK)
                {
                    methodResult.AddErrorBadRequest(questionResult.ErrorMessages);
                    return methodResult;
                }
                var (questionItem, answerConfig, correctCount, isAnswered) = questionResult.Result;
                var homeWorkQuestion = questionItem.HomeWorkQuestions.FirstOrDefault();
                if (homeWorkQuestion == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWorkQuestion));
                    return methodResult;
                }
                correctTotal += correctCount;

                var homeWorkAnswer = await _homeWorkAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.HomeWorkQuestionId == homeWorkQuestion.Id && x.HomeWorkResultId == request.HomeWorkResultId, cancellationToken);
                if (homeWorkAnswer == null)
                {
                    homeWorkAnswers.Add(new HomeWorkAnswer
                    {
                        Answer = answerConfig,
                        CorrectCount = correctCount,
                        HomeWorkQuestionId = homeWorkQuestion.Id,
                        HomeWorkResultId = homeWorkResult.Id,
                        IsCorrect = isAnswered ? correctCount == questionItem.CorrectTotal : null
                    });
                }
                else
                {
                    homeWorkAnswer.Answer = answerConfig;
                    homeWorkAnswer.CorrectCount = correctCount;
                    homeWorkAnswer.IsCorrect = isAnswered ? correctCount == questionItem.CorrectTotal : null;
                    homeWorkAnswers.Add(homeWorkAnswer);
                }
            }

            var homeWork = await _homeWorkRepository.Queryable
                            .Include(x => x!.HomeWorkQuestions)
                            .ThenInclude(x => x.Question)
                            .Where(x => x.Id == homeWorkResult.HomeWorkId)
                            .Select(h => new LessonHomeWorkResultModel
                            {
                                Id = h.Id,
                                QuestionTotal = h.HomeWorkQuestions.Select(x => x.Question).Where(x => x!.QuestionType != EnumQuestionType.ExercisePreparation).Count(),
                                CorrectTotal = h.HomeWorkQuestions.Select(x => x.Question).Where(x => x!.QuestionType != EnumQuestionType.ExercisePreparation).Sum(x => x!.CorrectTotal),
                            }).FirstOrDefaultAsync(cancellationToken);
            if (homeWork != null && request.Answers.Count == homeWork.QuestionTotal)
            {
                homeWorkResult.CorrectCount = correctTotal;
                homeWorkResult.Status = EnumResultStatus.Done;
                homeWorkResult.Percent = NumberHelper.GetPercent(homeWorkResult.CorrectCount, homeWorkResult.CorrectTotal);
                var skillScores = new SkillScores
                {
                    Skill = homeWorkResult.HomeWork.CourseSkill,
                    CorrectCount = homeWorkResult.CorrectCount,
                    TotalCount = homeWork.CorrectTotal,
                    CountQuestion = request.Answers.Count,
                    TotalQuestion = homeWork.QuestionTotal,
                    Percent = homeWorkResult.Percent,
                    Scores = 0
                };

                // làm nhiệm vụ
                //var courseId = homeWorkResult!.LessonResult?.CourseId ?? default;
                //await DoQuestBoard(courseId, request.Answers.Count, cancellationToken);

                homeWorkResult.SkillScores = new List<SkillScores> { skillScores };
            }
            else
            {
                homeWorkResult.Status = EnumResultStatus.Process;
                homeWorkResult.CorrectCount = correctTotal;
            }

            #endregion Validation

            await _homeWorkResultRepository.BulkUpdateList(new List<HomeWorkResult> { homeWorkResult }, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = c => new { c.LessonResultId, c.StudentId, c.HomeWorkId };
            });
            await _homeWorkResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

            if (homeWorkAnswers.Any())
            {
                await _requestSafeCachingService.SafeRequest<List<HomeWorkAnswer>>(
                    key: $"Add_HomeWorkAnswers_{string.Join("_", homeWorkAnswers.Select(hwa => $"{hwa.HomeWorkQuestionId}_{hwa.HomeWorkResultId}_{hwa.IsDeleted}"))}",
                    safeFunction: async () =>
                    {
                        await _homeWorkAnswerRepository.BulkMergeAsync(homeWorkAnswers, bulk =>
                        {
                            bulk.ColumnPrimaryKeyExpression = entity => new { entity.HomeWorkQuestionId, entity.HomeWorkResultId, entity.IsDeleted };
                        });
                        return homeWorkAnswers;
                    });
            }

            methodResult.StatusCode = StatusCodes.Status201Created;
            methodResult.Result = true;
            return methodResult;
        }
    }
}
