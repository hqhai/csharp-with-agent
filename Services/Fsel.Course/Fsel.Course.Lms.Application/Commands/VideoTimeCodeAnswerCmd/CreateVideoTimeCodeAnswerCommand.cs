// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd
{
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.VideoTimeCodeAnswers;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.ApplicationServices.CacheServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateVideoTimeCodeAnswerCommand : CreateVideoTimeCodeAnswerCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateVideoTimeCodeAnswerCommandHandler : IRequestHandler<CreateVideoTimeCodeAnswerCommand, MethodResult<bool>>
    {
        private readonly IVideoTimeCodeAnswerRepository _videoTimeCodeAnswerRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly QuestionConverter _questionConverter;
        private readonly VideoConverter _videoConverter;
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly QuestBoardPublisher _questBoardPublisher;
        private readonly IQuestionRepository _questionRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly IRequestSafeCachingService _requestSafeCachingService;

        public CreateVideoTimeCodeAnswerCommandHandler(
             IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository
            , IVideoResultRepository videoResultRepository
            , QuestionConverter questionConverter
            , VideoConverter videoConverter
            , IVideoTimeCodeResultRepository videoTimeCodeResultRepository
            , IQuestionRepository questionRepository,
              QuestBoardPublisher questBoardPublisher,
              AuthContext authContext,
              IUserService userService,
              IRequestSafeCachingService requestSafeCachingService)
        {
            _videoTimeCodeAnswerRepository = videoTimeCodeAnswerRepository;
            _videoResultRepository = videoResultRepository;
            _questionConverter = questionConverter;
            _videoConverter = videoConverter;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _questionRepository = questionRepository;
            _questBoardPublisher = questBoardPublisher;
            _authContext = authContext;
            _userService = userService;
            _requestSafeCachingService = requestSafeCachingService;
        }

        public async Task<MethodResult<bool>> Handle(CreateVideoTimeCodeAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

            if (request.Answers == null || request.Answers.Any(x => x.Answer == null) || request.Answers.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Answers));
                return methodResult;
            }

            var videoResult = await _videoResultRepository.Queryable.Include(x => x.VideoTimeCodeResults).FirstOrDefaultAsync(x => x.LessonResultId == request.LessonResultId, cancellationToken);
            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult));
                return methodResult;
            }
            var (questions, videoTimeCode) = await GetQuestionsAndVideoTimeCodeAsyns(request.Answers.Select(x => x.QuestionId).ToList());
            if (questions == null || !questions.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questions));
                return methodResult;
            }
            if (videoTimeCode == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoTimeCode));
                return methodResult;
            }
            var videoTimeCodeId = videoTimeCode.Id;
            var videoTimeCodeResult = await GetVideoTimeCodeResultAsync(videoResult, videoTimeCodeId);
            if (videoTimeCodeResult.Status == EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusDone), nameof(videoTimeCodeResult));
                return methodResult;
            }
            videoResult.CurrentVideoTimeCodeId = videoTimeCodeId;

            #region Chặn Time Code Chưa Done

            //var videoTimeCode = await _videoTimeCodeRepository.Queryable.Include(x => x.VideoTimeCodeAnswers.Where(x => x.VideoResultId == videoResult.Id)).Where(x => x.Id == videoResult.CurrentVideoTimeCodeId).FirstOrDefaultAsync(cancellationToken);
            //if (videoTimeCode != null && videoTimeCode.Id != videoTimeCodeQuestion?.Id && videoTimeCode.VideoTimeCodeAnswers.Any() && videoTimeCode.VideoTimeCodeAnswers.All(x => x.Status == EnumAnswerStatus.Process))
            //{
            //    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(EnumVideoTimeCodeErrorCode.VideoTimeCodePreviousNotDone));
            //    return methodResult;
            //}

            #endregion Chặn Time Code Chưa Done

            #endregion Validation

            var videoTimeCodeAnswers = new List<VideoTimeCodeAnswer>();
            var updateVideoTimeCodeAnswers = new List<VideoTimeCodeAnswer>();
            var skillScores = new List<SkillScores>();

            foreach (var item in request.Answers)
            {
                var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                var exercise = question?.ExerciseQuestions.Select(x => x.Exercise).FirstOrDefault();
                var exerciseId = exercise?.Id ?? default;
                var answer = await _videoTimeCodeAnswerRepository.GetAsync(videoTimeCodeId, videoResult.Id, question?.Id, exerciseId);
                var questionResult = _questionConverter.HandleQuestionAnswer(question, item.Answer, true, answer?.Answer, videoTimeCodeResult.Status == EnumResultStatus.Process, false);
                if (!questionResult.IsOK)
                {
                    methodResult.AddErrorBadRequest(questionResult.ErrorMessages);
                    return methodResult;
                }
                var (questionItem, answerConfig, correctCount, isAnswered) = questionResult.Result;
                if (answer == null)
                {
                    answer = new VideoTimeCodeAnswer
                    {
                        Answer = answerConfig ?? item.Answer,
                        VideoTimeCodeId = videoTimeCodeId,
                        ExerciseId = exerciseId,
                        QuestionId = questionItem.Id,
                        VideoTimeCodeResultId = videoTimeCodeResult.Id,
                        VideoResultId = videoResult.Id,
                        CorrectCount = questionItem.Ungraded ? default : correctCount,
                        Status = GetAnswerStatus(videoTimeCode.TimeCodeType, correctCount, questionItem.CorrectTotal),
                        IsCorrect = isAnswered ? correctCount == questionItem.CorrectTotal : null,
                        IsFirstSubmit = true,
                    };

                    videoTimeCodeAnswers.Add(answer);
                }
                else if (answer.Status != EnumAnswerStatus.Done)
                {
                    answer.Answer = answerConfig ?? item.Answer;
                    answer.Status = EnumAnswerStatus.Done;
                    answer.IsCorrect = isAnswered ? correctCount == questionItem.CorrectTotal : null;
                    answer.CorrectCount = questionItem.Ungraded ? default : correctCount;
                    answer.IsFirstSubmit = false;
                    updateVideoTimeCodeAnswers.Add(answer);
                }
                skillScores.Add(new SkillScores
                {
                    Skill = exercise?.CourseSkill ?? default,
                    CorrectCount = correctCount,
                    TotalCount = questionItem.CorrectTotal,
                    CountQuestion = 1,
                    TotalQuestion = 1,
                });
            }
            skillScores = GetSkillScores(skillScores);
            videoTimeCodeResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
            videoTimeCodeResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
            videoTimeCodeResult.Percent = skillScores.Sum(x => x.CorrectCount).GetPercent(skillScores.Sum(x => x.TotalCount));
            videoTimeCodeResult.SkillScores = skillScores;
            await _videoTimeCodeAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                if (videoTimeCode.TimeCodeType == EnumTimeCodeType.UnitTest)
                {
                    var courseId = videoResult.LessonResult?.CourseId;
                }
                if (videoTimeCodeAnswers.Any())
                {
                    videoTimeCodeResult.Status = EnumResultStatus.Process;
                    if (videoTimeCode?.TimeCodeType != EnumTimeCodeType.Standalone || skillScores.Sum(x => x.TotalCount) == videoTimeCodeAnswers.Sum(x => x.CorrectCount))
                    {
                        videoTimeCodeResult.Status = EnumResultStatus.Done;
                    }

                    await _requestSafeCachingService.SafeRequest<List<VideoTimeCodeAnswer>>(
                    key: $"Add_VideoTimeCodeAnswers_{string.Join("_", videoTimeCodeAnswers.Select(vtca => $"{vtca.VideoTimeCodeResultId}_{vtca.QuestionId}_{vtca.IsDeleted}"))}",
                    safeFunction: async () =>
                    {
                        await _videoTimeCodeAnswerRepository.BulkMergeAsync(videoTimeCodeAnswers, bulk =>
                        {
                            bulk.ColumnPrimaryKeyExpression = entity => new { entity.VideoTimeCodeResultId, entity.QuestionId, entity.IsDeleted };
                        });
                        return videoTimeCodeAnswers;
                    });
                }
                else if (updateVideoTimeCodeAnswers.Any())
                {
                    videoTimeCodeResult.Status = EnumResultStatus.Done;
                    await _videoTimeCodeAnswerRepository.BulkUpdateList(updateVideoTimeCodeAnswers, bulk =>
                    {
                        bulk.IgnoreOnUpdateExpression = entity => new { entity.VideoResultId, entity.VideoTimeCodeResultId, entity.QuestionId };
                    });
                }
                if (videoTimeCode != null && videoTimeCode.TimeCodeType == EnumTimeCodeType.Standalone)
                {
                    var highestStreak = await _videoConverter.GetHighestStreak(videoResult);

                    videoResult.HighestStreak = highestStreak.HighestStreakQuestion;
                    videoResult.HighestStreakSubQuestion = highestStreak.HighestStreakSubQuestion;
                }
                else if (videoTimeCode != null && videoTimeCode.TimeCodeType != EnumTimeCodeType.Standalone)
                {
                    var highestStreak = await _videoConverter.GetHighestStreak(videoTimeCodeResult);

                    videoTimeCodeResult.HighestStreak = highestStreak.HighestStreakSubQuestion;
                    videoTimeCodeResult.HighestStreakSubQuestion = highestStreak.HighestStreakSubQuestion;
                }
                await _videoResultRepository.BulkUpdateList(new List<VideoResult> { videoResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.VideoId, c.StudentId, c.LessonResultId };
                });
                await _videoResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                await _videoTimeCodeResultRepository.BulkUpdateList(new List<VideoTimeCodeResult> { videoTimeCodeResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.VideoTimeCodeId, c.StudentId, c.VideoResultId };
                });

                return methodResult;
            });

            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static List<SkillScores> GetSkillScores(IList<SkillScores> skillScores)
        {
            ArgumentNullException.ThrowIfNull(skillScores);
            return skillScores.GroupBy(x => x.Skill).Select(x => new SkillScores
            {
                Skill = x.Key,
                CorrectCount = x.Sum(x => x.CorrectCount),
                TotalCount = x.Sum(x => x.TotalCount),
                CountQuestion = x.Sum(x => x.CountQuestion),
                TotalQuestion = x.Sum(x => x.TotalQuestion),
                Percent = x.Sum(x => x.CorrectCount).GetPercent(x.Sum(x => x.TotalCount))
            }).ToList();
        }

        private async Task<VideoTimeCodeResult> GetVideoTimeCodeResultAsync(VideoResult videoResult, Guid videoTimeCodeId)
        {
            var videoTimeCodeResult = videoResult.VideoTimeCodeResults.Where(x => x.VideoTimeCodeId == videoTimeCodeId && x.VideoResultId == videoResult.Id).FirstOrDefault();
            if (videoTimeCodeResult == null)
            {
                videoTimeCodeResult = new VideoTimeCodeResult
                {
                    VideoResultId = videoResult.Id,
                    VideoTimeCodeId = videoTimeCodeId,
                    StudentId = videoResult.StudentId,
                    Status = EnumResultStatus.New
                };

                await _requestSafeCachingService.SafeRequest<VideoTimeCodeResult>(
                key: $"Add_VideoTimeCodeResult_{videoTimeCodeResult.VideoTimeCodeId}_{videoTimeCodeResult.VideoResultId}_{videoTimeCodeResult.IsDeleted}",
                safeFunction: async () =>
                {
                    await _videoTimeCodeResultRepository.BulkMergeAsync(new List<VideoTimeCodeResult> { videoTimeCodeResult }, bulk =>
                    {
                        bulk.ColumnPrimaryKeyExpression = c => new { c.VideoTimeCodeId, c.StudentId, c.VideoResultId, c.IsDeleted };
                    });
                    return videoTimeCodeResult;
                });
            }
            return videoTimeCodeResult;
        }

        private static EnumAnswerStatus GetAnswerStatus(EnumTimeCodeType? timeCodeType, int correctCount, int correctTotal)
        {
            if (timeCodeType == EnumTimeCodeType.Standalone)
            {
                if (correctCount == correctTotal)
                {
                    return EnumAnswerStatus.Done;
                }
                return EnumAnswerStatus.Process;
            }
            return EnumAnswerStatus.Done;
        }

        private async Task<(IList<Question>?, VideoTimeCode?)> GetQuestionsAndVideoTimeCodeAsyns(IList<Guid> questionIds)
        {
            var questions = await _questionRepository.GetIncludeTimeCodeByIdAsync(questionIds);
            var exercise = questions?.SelectMany(x => x.ExerciseQuestions).Select(x => x.Exercise).FirstOrDefault();
            var videoTimeCode = exercise?.TimeCodeExercises.Select(x => x.VideoTimeCode).FirstOrDefault();
            return (questions, videoTimeCode);
        }
    }
}
