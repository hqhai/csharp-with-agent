// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerV1i1Cmd
{
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.VideoTimeCodeAnswers;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateVideoTimeCodeAnswerByTimeCodeCommand : CreateVideoTimeCodeAnswerV1i1CommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateVideoTimeCodeAnswerByTimeCodeCommandHandler : IRequestHandler<CreateVideoTimeCodeAnswerByTimeCodeCommand, MethodResult<bool>>
    {
        private readonly IVideoTimeCodeAnswerRepository _videoTimeCodeAnswerRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly VideoConverter _videoConverter;
        private readonly IQuestionRepository _questionRepository;
        private readonly QuestionConverter _questionConverter;

        public CreateVideoTimeCodeAnswerByTimeCodeCommandHandler(
             IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository
            , IVideoResultRepository videoResultRepository
            , IExerciseRepository exerciseRepository
            , IVideoTimeCodeResultRepository videoTimeCodeResultRepository
            , IVideoTimeCodeRepository videoTimeCodeRepository
            , VideoConverter videoConverter
            , IQuestionRepository questionRepository
            , QuestionConverter questionConverter)
        {
            _videoTimeCodeAnswerRepository = videoTimeCodeAnswerRepository;
            _videoResultRepository = videoResultRepository;
            _exerciseRepository = exerciseRepository;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _videoConverter = videoConverter;
            _questionRepository = questionRepository;
            _questionConverter = questionConverter;
        }

        public async Task<MethodResult<bool>> Handle(CreateVideoTimeCodeAnswerByTimeCodeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var method = await CreateAnswer(request, cancellationToken);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }
            var (videoResult, videoTimeCode, videoTimeCodeResult) = method.Result;
            await _videoResultRepository.ExecuteTransactionAsync(async () =>
            {
                await UpdateVideoTimeCodeResult(videoTimeCode, videoTimeCodeResult, request.IsSubmit, cancellationToken).ConfigureAwait(false);
                _videoResultRepository.Update(videoResult);
                await _videoResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.Result = true;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });
            return methodResult;
        }

        private async Task<MethodResult<(VideoResult, VideoTimeCode, VideoTimeCodeResult)>> CreateAnswer(CreateVideoTimeCodeAnswerByTimeCodeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            var methodResult = new MethodResult<(VideoResult, VideoTimeCode, VideoTimeCodeResult)>();
            var method = await Validate(request, cancellationToken);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }
            var (videoResult, videoTimeCode, videoTimeCodeResult, questions) = method.Result;
            if (request.Answers != null && request.Answers.Any())
            {
                var methodCreateAnswer = await CreateAnswerAsync(request, questions, videoTimeCode, videoTimeCodeResult, cancellationToken);
                if (!methodCreateAnswer.IsOK)
                {
                    methodResult.AddErrorBadRequest(methodCreateAnswer.ErrorMessages);
                    return methodResult;
                }
            }
            var isDone = videoTimeCode.TimeCodeType != EnumTimeCodeType.Standalone || videoTimeCodeResult.Status == EnumResultStatus.Process;
            if (request.IsSubmit)
            {
                await _videoConverter.UpdateVideoAnswers(videoTimeCode, videoTimeCodeResult, isDone);
            }
            methodResult.Result = (videoResult, videoTimeCode, videoTimeCodeResult);
            return methodResult;
        }

        private async Task<MethodResult<bool>> CreateAnswerAsync(CreateVideoTimeCodeAnswerByTimeCodeCommand request, IList<Question> questions, VideoTimeCode videoTimeCode, VideoTimeCodeResult videoTimeCodeResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(questions);
            ArgumentNullException.ThrowIfNull(request.Answers);
            var methodResult = new MethodResult<bool>();
            var videoTimeCodeAnswers = new List<VideoTimeCodeAnswer>();
            var updateVideoTimeCodeAnswers = new List<VideoTimeCodeAnswer>();
            List<SkillScores> skillScores = new List<SkillScores>();
            foreach (var item in request.Answers)
            {
                var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                var exercise = question?.ExerciseQuestions.Select(x => x.Exercise).FirstOrDefault();
                var answer = await _videoTimeCodeAnswerRepository.GetAsync(videoTimeCode.Id, videoTimeCodeResult.VideoResultId, question?.Id, exercise?.Id ?? default);
                var questionResult = _questionConverter.HandleQuestionAnswer(question, item.Answer, request.IsSubmit, answer?.Answer, videoTimeCodeResult.Status == EnumResultStatus.Process, videoTimeCode.ExecutionTime == 0);
                if (!questionResult.IsOK)
                {
                    methodResult.AddErrorBadRequest(questionResult.ErrorMessages);
                    return methodResult;
                }
                var (questionItem, answerConfig, correctCount) = questionResult.Result;
                if (answer == null)
                {
                    answer = new VideoTimeCodeAnswer
                    {
                        VideoTimeCodeId = videoTimeCode.Id,
                        ExerciseId = exercise?.Id ?? default,
                        QuestionId = questionItem.Id,
                        VideoTimeCodeResultId = videoTimeCodeResult.Id,
                        VideoResultId = videoTimeCodeResult.VideoResultId,
                    };
                    videoTimeCodeAnswers.Add(answer);
                }
                else
                {
                    updateVideoTimeCodeAnswers.Add(answer);
                }
                answer.Answer = answerConfig ?? item.Answer;
                answer.CorrectCount = questionItem.Ungraded ? default : correctCount;
                answer.Status = GetAnswerStatus(request.IsSubmit, correctCount, questionItem.CorrectTotal);
                answer.IsCorrect = GetAnswerStatus(request.IsSubmit, correctCount, questionItem.CorrectTotal) == EnumAnswerStatus.Done;
            }
            if (videoTimeCodeAnswers.Any())
            {
                await _videoTimeCodeAnswerRepository.AddList(videoTimeCodeAnswers);
                await _videoTimeCodeAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            else if (updateVideoTimeCodeAnswers.Any())
            {
                _videoTimeCodeAnswerRepository.UpdateList(updateVideoTimeCodeAnswers);
                await _videoTimeCodeAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            return methodResult;
        }

        private async Task UpdateVideoTimeCodeResult(VideoTimeCode videoTimeCode, VideoTimeCodeResult videoTimeCodeResult, bool isSubmit, CancellationToken cancellationToken)
        {
            videoTimeCodeResult = await GetVideoTimeCodeResultAsync(videoTimeCodeResult, videoTimeCode, isSubmit, cancellationToken);
            _videoTimeCodeResultRepository.Update(videoTimeCodeResult);
            await _videoTimeCodeResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task<VideoTimeCodeResult> GetVideoTimeCodeResultAsync(VideoTimeCodeResult videoTimeCodeResult, VideoTimeCode videoTimeCode, bool isSubmit, CancellationToken cancellationToken)
        {
            if (videoTimeCodeResult.Status == EnumResultStatus.New)
            {
                videoTimeCodeResult.WorkingTime += _videoConverter.GetWorkingTime(videoTimeCodeResult, videoTimeCode);
            }
            else if (videoTimeCodeResult.Status == EnumResultStatus.Process)
            {
                videoTimeCodeResult.RetryWorkingTime += _videoConverter.GetWorkingTime(videoTimeCodeResult, videoTimeCode);
            }
            if (isSubmit)
            {
                var (listSkillScore, isDone) = await GetSkillScores(videoTimeCode, videoTimeCodeResult, cancellationToken);
                videoTimeCodeResult.Status = isDone ? EnumResultStatus.Process : EnumResultStatus.Done;
                videoTimeCodeResult.CorrectCount = (int)listSkillScore.Sum(x => x.CorrectCount);
                videoTimeCodeResult.CorrectTotal = (int)listSkillScore.Sum(x => x.TotalCount);
                videoTimeCodeResult.SkillScores = listSkillScore;
            }
            videoTimeCodeResult.IsWorking = false;
            return videoTimeCodeResult;
        }

        private static VideoTimeCodeResult? GetVideoTimeCodeResult(VideoResult videoResult, Guid videoTimeCodeId)
        {
            return videoResult.VideoTimeCodeResults.FirstOrDefault(x => x.VideoTimeCodeId == videoTimeCodeId && x.VideoResultId == videoResult.Id);
        }

        private static EnumAnswerStatus GetAnswerStatus(bool isSubmit, int correctCount, int correctTotal)
        {
            if (correctCount == correctTotal && isSubmit)
            {
                return EnumAnswerStatus.Done;
            }
            return EnumAnswerStatus.Process;
        }

        public async Task<MethodResult<(VideoResult, VideoTimeCode, VideoTimeCodeResult, IList<Question>)>> Validate(CreateVideoTimeCodeAnswerByTimeCodeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<(VideoResult, VideoTimeCode, VideoTimeCodeResult, IList<Question>)> methodResult = new MethodResult<(VideoResult, VideoTimeCode, VideoTimeCodeResult, IList<Question>)>();
            var videoResult = await _videoResultRepository.Queryable.Include(x => x.VideoTimeCodeResults).FirstOrDefaultAsync(x => x.Id == request.VideoResultId, cancellationToken);
            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult));
                return methodResult;
            }
            var videoTimeCode = await _videoTimeCodeRepository.GetByIdAsync(request.VideoTimeCodeId);
            if (videoTimeCode == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.VideoTimeCodeId));
                return methodResult;
            }
            var videoTimeCodeResult = GetVideoTimeCodeResult(videoResult, request.VideoTimeCodeId);
            if (videoTimeCodeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoTimeCodeResult));
                return methodResult;
            }
            if (videoTimeCodeResult.Status == EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoResultErrorCode.VideoTimeCodeResultDone), nameof(videoTimeCodeResult));
                return methodResult;
            }
            videoResult.CurrentVideoTimeCodeId = request.VideoTimeCodeId;
            var questionIds = request.Answers.Select(x => x.QuestionId).ToList();
            var questions = new List<Question>();
            if (questionIds.Any())
            {
                questions = await _questionRepository.GetListAsync(request.Answers.Select(x => x.QuestionId).ToList());
                if (questions == null || !questions.Any())
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questions));
                    return methodResult;
                }
                var timeCodeId = questions.SelectMany(x => x.ExerciseQuestions).Select(x => x.Exercise).SelectMany(x => x!.TimeCodeExercises).Select(x => x.VideoTimeCodeId).FirstOrDefault();
                if (timeCodeId != request.VideoTimeCodeId)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.VideoTimeCodeId));
                    return methodResult;
                }
            }

            #region Chặn Time Code Chưa Done

            //var videoTimeCode = await _videoTimeCodeRepository.Queryable.Include(x => x.VideoTimeCodeAnswers.Where(x => x.VideoResultId == videoResult.Id)).Where(x => x.Id == videoResult.CurrentVideoTimeCodeId).FirstOrDefaultAsync(cancellationToken);
            //if (videoTimeCode != null && videoTimeCode.Id != videoTimeCodeQuestion?.Id && videoTimeCode.VideoTimeCodeAnswers.Any() && videoTimeCode.VideoTimeCodeAnswers.All(x => x.Status == EnumAnswerStatus.Process))
            //{
            //    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(EnumVideoTimeCodeErrorCode.VideoTimeCodePreviousNotDone));
            //    return methodResult;
            //}

            #endregion Chặn Time Code Chưa Done

            methodResult.Result = (videoResult, videoTimeCode, videoTimeCodeResult, questions);
            return methodResult;
        }

        private static SkillScores GetSkillScore(IGrouping<EnumCourseSkill, Exercise> exercise)
        {
            ArgumentNullException.ThrowIfNull(exercise);
            var questions = exercise.SelectMany(x => x.ExerciseQuestions).Select(x => x.Question).ToList();
            var answers = questions.SelectMany(x => x!.VideoTimeCodeAnswers).ToList();
            var correctCount = answers.Sum(x => x.CorrectCount);
            return new SkillScores
            {
                CorrectCount = correctCount,
                CountQuestion = answers.Count,
                Skill = exercise.Key,
                TotalCount = questions.Sum(x => x!.CorrectTotal),
                TotalQuestion = questions.Count,
                Scores = correctCount.GetIeltsScore(exercise.Key)
            };
        }

        private async Task<(IList<SkillScores>, bool)> GetSkillScores(VideoTimeCode videoTimeCode, VideoTimeCodeResult videoTimeCodeResult, CancellationToken cancellationToken)
        {
            var exerciseIds = await _videoTimeCodeAnswerRepository.Queryable.Where(x => x.VideoTimeCodeId == videoTimeCode.Id && x.VideoTimeCodeResultId == videoTimeCodeResult.Id && x.VideoResultId == videoTimeCodeResult.VideoResultId).Select(x => x.ExerciseId).Distinct().ToListAsync(cancellationToken);
            var exercises = await _exerciseRepository.Queryable.Include(x => x.ExerciseQuestions)
                                    .ThenInclude(x => x.Question)
                                    .ThenInclude(x => x!.VideoTimeCodeAnswers.Where(x => x.VideoTimeCodeResultId == videoTimeCodeResult.Id))
                                    .Where(x => exerciseIds.Contains(x.Id))
                                    .ToListAsync(cancellationToken);
            var isDone = exercises.SelectMany(x => x.ExerciseQuestions).Select(x => x.Question).SelectMany(x => x!.VideoTimeCodeAnswers).Any(x => x.Status != EnumAnswerStatus.Done);
            var skillScores = exercises.GroupBy(x => x.CourseSkill).Select(x => GetSkillScore(x)).ToList();
            return (skillScores, isDone);
        }
    }
}
