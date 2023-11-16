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
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.HomeWorkAnswers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateHomeWorkAnswerCommand : CreateHomeWorkAnswerCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateHomeWorkAnswerCommandHandler : IRequestHandler<CreateHomeWorkAnswerCommand, MethodResult<bool>>
    {
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IHomeWorkQuestionRepository _homeWorkQuestionRepository;
        private readonly IHomeWorkAnswerRepository _homeWorkAnswerRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly FinishOneHomeWorkPublisher _finishOneHomeWorkPublisher;
        private readonly AnswerTypeConverter _answerTypeConverter;
        private readonly IQuestionRepository _questionRepository;
        private readonly AuthContext _authContext;
        private readonly QuestBoardPublisher _questBoardPublisher;
        private const int FIFTY_PERCENT_DONE = 50;
        private const float ACHIEVED_POINT = 1;

        public CreateHomeWorkAnswerCommandHandler(IHomeWorkResultRepository homeWorkResultRepository,
            IHomeWorkQuestionRepository homeWorkQuestionRepository,
            IHomeWorkAnswerRepository homeWorkAnswerRepository,
            IHomeWorkRepository homeWorkRepository,
            FinishOneHomeWorkPublisher finishOneHomeWorkPublisher,
            AnswerTypeConverter answerTypeConverter,
            IQuestionRepository questionRepository
,
            AuthContext authContext,
            QuestBoardPublisher questBoardPublisher)
        {
            _homeWorkResultRepository = homeWorkResultRepository;
            _homeWorkQuestionRepository = homeWorkQuestionRepository;
            _homeWorkAnswerRepository = homeWorkAnswerRepository;
            _homeWorkRepository = homeWorkRepository;
            _finishOneHomeWorkPublisher = finishOneHomeWorkPublisher;
            _answerTypeConverter = answerTypeConverter;
            _questionRepository = questionRepository;
            _authContext = authContext;
            _questBoardPublisher = questBoardPublisher;
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
                methodResult.AddErrorBadRequest(nameof(EnumHomeWorkResultErrorCode.HomeWorkResultDone));
                return methodResult;
            }
            if (homeWorkResult.HomeWork == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWorkResult.HomeWork));
                return methodResult;
            }

            var questionIds = request.Answers.Select(x => x.QuestionId).Distinct().ToList();
            var questions = await _questionRepository.GetByIdsAsync(questionIds);
            var homeWorkAnswers = new List<HomeWorkAnswer>();
            int correctTotal = default;
            foreach (var item in request.Answers)
            {
                var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
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

                var homeWorkQuestion = await _homeWorkQuestionRepository.Queryable.Where(x => x.HomeWorkId == homeWorkResult.HomeWorkId && x.QuestionId == item.QuestionId)
                                                                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);
                if (homeWorkQuestion == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWorkQuestion));
                    return methodResult;
                }
                var homeWorkAnswer = await _homeWorkAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.HomeWorkQuestionId == homeWorkQuestion.Id && x.HomeWorkResultId == request.HomeWorkResultId, cancellationToken);
                var (answerConfig, correctCount) = _answerTypeConverter.GetTotalCorrectByAsnwerType(item.Answer, question.Config, question.QuestionType);
                if (answerConfig == null && !string.IsNullOrEmpty(item.Answer?.ToString()))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumHomeWorkAnswerErrorCode.AnswerIsInTheWrongFormat), nameof(answerConfig), answerConfig);
                    return methodResult;
                }
                if (homeWorkAnswer == null)
                {
                    correctTotal += correctCount;
                    homeWorkAnswers.Add(new HomeWorkAnswer
                    {
                        Answer = answerConfig,
                        CorrectCount = correctCount,
                        HomeWorkQuestionId = homeWorkQuestion.Id,
                        HomeWorkResultId = homeWorkResult.Id
                    });
                }
                else
                {
                    correctTotal += correctCount;
                    homeWorkAnswer.Answer = answerConfig;
                    homeWorkAnswer.CorrectCount = correctCount;
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
                await _finishOneHomeWorkPublisher.Publish(homeWorkResult, cancellationToken);
                homeWorkResult.SkillScores = new List<SkillScores> { skillScores };
            }
            else
            {
                homeWorkResult.Status = EnumResultStatus.Process;
                homeWorkResult.CorrectCount = correctTotal;
            }

            #endregion Validation

            _homeWorkResultRepository.Update(homeWorkResult);
            await _homeWorkResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

            await _homeWorkAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                var homeworkresulttoday = _homeWorkResultRepository.Queryable.Where(x => x.CreatedUserId == _authContext.CurrentUserId);
                var homeworkTest = homeworkresulttoday.ToList();

                if (homeWorkAnswers.Any())
                {
                    await DoDailyQuest(cancellationToken);
                    _homeWorkAnswerRepository.UpdateList(homeWorkAnswers);
                    await _homeWorkAnswerRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
        public async Task DoDailyQuest(CancellationToken cancellationToken)
        {
            IList<EnumQuestBoardCategory> categories = new List<EnumQuestBoardCategory>() { EnumQuestBoardCategory.CompleteHomeWorkAtLeastFiftyPercent };

            var checkHomeWorkDoneAtLeastFiftyPercent = _homeWorkResultRepository.Queryable.Any(x => x.CreatedUserId == _authContext.CurrentUserId && x.CreatedDate.Date == DateTime.UtcNow.Date && x.CreatedDate.Month == DateTime.UtcNow.Month && x.CreatedDate.Year == DateTime.UtcNow.Year && x.Percent >= FIFTY_PERCENT_DONE);

            var homeWorkResultDaily = _homeWorkResultRepository.Queryable.FirstOrDefault(x => x.CreatedUserId == _authContext.CurrentUserId);

            var courseId = homeWorkResultDaily?.LessonResult?.CourseId;
            var studentId = homeWorkResultDaily?.StudentId;

            if (checkHomeWorkDoneAtLeastFiftyPercent && homeWorkResultDaily != null)
            {
                QuestBoardQueueModel questBoardModel = new QuestBoardQueueModel()
                {
                    StudentId = homeWorkResultDaily.StudentId!,
                    Categories = categories,
                    AchievedPoint = ACHIEVED_POINT,
                    ObjectId = homeWorkResultDaily.Id,
                    CourseId = homeWorkResultDaily.LessonResult!.CourseId,
                };

                await _questBoardPublisher.Publish(questBoardModel, cancellationToken);
            }
        }

    }
}
