// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices
{
    using AutoMapper;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;

    public interface IVideoTimeCodeService
    {
        Task<VideoTimeCodeModel> GetVideoTimeCodeDetailAsync(VideoTimeCode videoTimeCode,
        VideoTimeCodeResultModel videoTimeCodeResult,
        bool isShowSubStatus = false);
    }

    public class VideoTimeCodeService : IVideoTimeCodeService
    {
        private readonly ITimeCodeExerciseRepository _timeCodeExerciseRepository;
        private readonly IExerciseQuestionRepository _exerciseQuestionRepository;
        private readonly IQuestionExplanationErrorRepository _questionExplanationErrorRepository;
        private readonly IQuestionShuffleRepository _questionShuffleRepository;
        private readonly IVideoTimeCodeAnswerRepository _videoTimeCodeAnswerRepository;
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly IMapper _mapper;
        private readonly IVideoTimeCodeModelCachingService _videoTimeCodeModelCachingService;
        private readonly DateTimeConverter _dateTimeConverter;
        private readonly AnswerTypeConverter _answerTypeConverter;
        private readonly IExerciseRepository _exerciseRepository;

        public VideoTimeCodeService(ITimeCodeExerciseRepository timeCodeExerciseRepository,
            IExerciseQuestionRepository exerciseQuestionRepository,
            IQuestionExplanationErrorRepository questionExplanationErrorRepository,
            IQuestionShuffleRepository questionShuffleRepository,
            IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository,
            QuestionTypeConverter questionTypeConverter,
            IMapper mapper,
            IVideoTimeCodeModelCachingService videoTimeCodeModelCachingService,
            DateTimeConverter dateTimeConverter,
            AnswerTypeConverter answerTypeConverter,
            IExerciseRepository exerciseRepository)
        {
            _timeCodeExerciseRepository = timeCodeExerciseRepository;
            _exerciseQuestionRepository = exerciseQuestionRepository;
            _questionExplanationErrorRepository = questionExplanationErrorRepository;
            _questionShuffleRepository = questionShuffleRepository;
            _videoTimeCodeAnswerRepository = videoTimeCodeAnswerRepository;
            _questionTypeConverter = questionTypeConverter;
            _mapper = mapper;
            _videoTimeCodeModelCachingService = videoTimeCodeModelCachingService;
            _dateTimeConverter = dateTimeConverter;
            _answerTypeConverter = answerTypeConverter;
            _exerciseRepository = exerciseRepository;
        }

        public async Task<VideoTimeCodeModel> GetVideoTimeCodeDetailAsync(VideoTimeCode videoTimeCode, VideoTimeCodeResultModel videoTimeCodeResult, bool isShowSubStatus = false)
        {
            ArgumentNullException.ThrowIfNull(videoTimeCode);
            ArgumentNullException.ThrowIfNull(videoTimeCodeResult);
            var timeCodeModel = await _videoTimeCodeModelCachingService.GetOrSetAsync(videoTimeCode.Id.ToString(), async (ctx, _) =>
            {
                return await BuildVideoTimeCodeStaticModelInternalAsync(videoTimeCode, _);
            });

            await MapVideoTimeCodeResultAsync(
                timeCodeModel,
                videoTimeCode,
                videoTimeCodeResult,
                isShowSubStatus);

            return timeCodeModel;
        }

        private async Task MapVideoTimeCodeResultAsync(
        VideoTimeCodeModel timeCode,
        VideoTimeCode videoTimeCode,
        VideoTimeCodeResultModel videoTimeCodeResult,
        bool isShowSubStatus)
        {
            var questionIds = timeCode.Exercises
                .SelectMany(e => e.Questions)
                .Select(q => q.Id)
                .ToList();

            if (!questionIds.Any())
            {
                timeCode.CorrectCount = videoTimeCodeResult.CorrectCount;
                timeCode.Status = videoTimeCodeResult.Status != EnumResultStatus.Done
                    ? EnumResultStatus.Process
                    : EnumResultStatus.Done;
                timeCode.VideoTimeCodeResult = GetVideoTimeCodeResult(videoTimeCodeResult, videoTimeCode);
                return;
            }

            var questionExplanationErrors = await _questionExplanationErrorRepository.ReadQueryable
                .WhereBulkContains(questionIds, x => x.QuestionId)
                .Where(x =>
                    x.Status == EnumProcessedStatus.NotProcessed &&
                    x.ObjectResultId == videoTimeCodeResult.VideoResultId)
                .ToListAsync();

            var questionShuffles = await _questionShuffleRepository.Queryable
                .WhereBulkContains(questionIds, x => x.QuestionId)
                .Where(x => x.StudentId == videoTimeCodeResult.StudentId)
                .ToListAsync();

            var videoTimeCodeAnswers = await _videoTimeCodeAnswerRepository.ReadQueryable
                .Where(x => x.VideoTimeCodeResultId == videoTimeCodeResult.Id)
                .ToListAsync();

            var listQuestionShuffleToSave = new List<QuestionShuffle>();

            foreach (var exerciseModel in timeCode.Exercises)
            {
                foreach (var questionModel in exerciseModel.Questions)
                {
                    var videoTimeCodeAnswer = videoTimeCodeAnswers.FirstOrDefault(x => x.QuestionId == questionModel.Id);
                    var isCheck = videoTimeCodeAnswer != null ? videoTimeCodeAnswer.Status == EnumAnswerStatus.Done : videoTimeCodeResult.Status == EnumResultStatus.Done;
                    if (!isCheck)
                    {
                        questionModel.Explanations = null;
                        questionModel.Explanation = null;
                    }

                    questionModel.CorrectStatus = GetCorrectStatus(videoTimeCodeAnswer);
                    questionModel.IsReportExplanation = questionExplanationErrors.Any(x => x.QuestionId == questionModel.Id);
                    questionModel.Config = _questionTypeConverter.QuestionTypeConverterObject(questionModel.Config, questionModel.QuestionType, isDisableAnswers: !isCheck).Item1;

                    var questionShuffle = questionShuffles
                        .FirstOrDefault(x => x.QuestionId == questionModel.Id);

                    (questionModel.Config, string? questionShuffleStr) =
                        _questionTypeConverter.QuestionShuffleConverterObject(
                            questionModel.Config,
                            questionModel.QuestionType,
                            isCheck,
                            questionShuffle?.ShuffleConfigs);

                    if (!string.IsNullOrEmpty(questionShuffleStr) &&
                        (questionShuffle == null || questionShuffle.ShuffleConfigStr != questionShuffleStr))
                    {
                        if (questionShuffle == null)
                        {
                            questionShuffle = new QuestionShuffle
                            {
                                QuestionId = questionModel.Id,
                                StudentId = videoTimeCodeResult.StudentId,
                                ShuffleConfigStr = questionShuffleStr
                            };
                        }
                        else
                        {
                            questionShuffle.ShuffleConfigStr = questionShuffleStr;
                        }

                        listQuestionShuffleToSave.Add(questionShuffle);
                    }

                    if (videoTimeCodeAnswer != null)
                    {
                        videoTimeCodeAnswer.CorrectCount = isCheck
                            ? videoTimeCodeAnswer.CorrectCount
                            : default;

                        videoTimeCodeAnswer.Answer = _answerTypeConverter.AnswerTypeConverterObject(
                            videoTimeCodeAnswer.Answer,
                            questionModel.QuestionType,
                            isShowSubStatus,
                            videoTimeCodeResult.Status,
                            videoTimeCode.TimeCodeType == EnumTimeCodeType.Standalone);

                        questionModel.CorrectStatus = GetCorrectStatus(videoTimeCodeAnswer, isShowSubStatus);
                        questionModel.ResultAnswer = _mapper.Map<AnswerModel>(videoTimeCodeAnswer);
                    }
                }
            }

            if (listQuestionShuffleToSave.Any())
            {
                await _questionShuffleRepository.SaveQuestionShufflesAsync(listQuestionShuffleToSave);
            }

            timeCode.CorrectCount = videoTimeCodeResult.CorrectCount;
            timeCode.Status = videoTimeCodeResult.Status != EnumResultStatus.Done
                ? EnumResultStatus.Process
                : EnumResultStatus.Done;
            timeCode.VideoTimeCodeResult = GetVideoTimeCodeResult(videoTimeCodeResult, videoTimeCode);
        }

        private static EnumCorrectStatus? GetCorrectStatus(VideoTimeCodeAnswer? videoTimeCodeAnswer, bool isShowAnswer = false)
        {
            EnumCorrectStatus? status = null;
            if (videoTimeCodeAnswer != null && videoTimeCodeAnswer.IsCorrect.HasValue)
            {
                status = EnumCorrectStatus.Process;
                if (isShowAnswer || videoTimeCodeAnswer.Status == EnumAnswerStatus.Done)
                {
                    status = videoTimeCodeAnswer.IsCorrect.Value ? EnumCorrectStatus.Correct : EnumCorrectStatus.Fail;
                }
            }

            return status;
        }

        private VideoTimeCodeResultModel? GetVideoTimeCodeResult(VideoTimeCodeResultModel? videoTimeCodeResult, VideoTimeCode videoTimeCode)
        {
            if (videoTimeCodeResult != null)
            {
                double remainingTime;
                if (videoTimeCode.TimeCodeType != EnumTimeCodeType.Standalone)
                {
                    remainingTime = videoTimeCodeResult.WorkingTime;
                }
                else
                {
                    if (videoTimeCodeResult.Status == EnumResultStatus.New)
                    {
                        remainingTime = videoTimeCodeResult.WorkingTime;
                    }
                    else if (videoTimeCodeResult.Status == EnumResultStatus.Process)
                    {
                        remainingTime = videoTimeCodeResult.RetryWorkingTime;
                    }
                    else
                    {
                        remainingTime = videoTimeCodeResult.RetryWorkingTime == default ? videoTimeCodeResult.WorkingTime : videoTimeCodeResult.RetryWorkingTime;
                    }
                }
                videoTimeCodeResult.RemainingTime = _dateTimeConverter.SetRemainingTime(videoTimeCode.ExecutionTime, remainingTime);
            }
            return videoTimeCodeResult;
        }

        private async Task<VideoTimeCodeModel> BuildVideoTimeCodeStaticModelInternalAsync(
        VideoTimeCode videoTimeCode,
        CancellationToken ct)
        {
            var timeCode = _mapper.Map<VideoTimeCodeModel>(videoTimeCode);

            var exercises = await (from ex in _exerciseRepository.Queryable.Include(x => x.Skill)
                                   join tce in _timeCodeExerciseRepository.Queryable
                                   on ex.Id equals tce.ExerciseId
                                   where tce.VideoTimeCodeId == videoTimeCode.Id
                                   select ex)
                                   .OrderBy(x => x.CreatedDate)
                                   .ToListAsync(ct);

            var exerciseQuestions = await _exerciseQuestionRepository.Queryable
                .Where(x => exercises.Select(e => e.Id).Contains(x.ExerciseId))
                .OrderBy(x => x.CreatedDate)
                .Select(x => new
                {
                    ExerciseId = x.ExerciseId,
                    Question = x.Question ?? new Question(),
                })
                .ToListAsync(ct);

            var questions = exerciseQuestions.Select(x => x.Question).ToList();
            var questionIds = questions.Select(x => x!.Id).ToList();

            timeCode.TotalCount = questionIds.Count;
            timeCode.Ungraded = questions.Any(x => x.Ungraded);
            timeCode.CorrectTotal = questions.Sum(x => x.CorrectTotal);
            timeCode.CourseSkills = exercises.Select(x => x.CourseSkill).Distinct().ToList();

            foreach (var exercise in exercises)
            {
                var exerciseModel = _mapper.Map<ExerciseModel>(exercise);

                var listQuestion = exerciseQuestions
                    .Where(x => x.ExerciseId == exercise.Id)
                    .Select(x => x.Question)
                    .ToList();

                foreach (var question in listQuestion)
                {
                    if (question == null)
                    {
                        continue;
                    }

                    var questionModel = _mapper.Map<QuestionModel>(question);

                    questionModel.ResultAnswer = null;
                    questionModel.CorrectStatus = null;
                    questionModel.IsReportExplanation = false;

                    // Config chỉ để hiển thị, disable trả lời
                    questionModel.Config = _questionTypeConverter
                        .QuestionTypeConverterObject(
                            question.Config,
                            question.QuestionType,
                            isDisableAnswers: false)
                        .Item1;

                    exerciseModel.Questions.Add(questionModel);
                }

                timeCode.Exercises.Add(exerciseModel);
            }

            return timeCode;
        }
    }
}
