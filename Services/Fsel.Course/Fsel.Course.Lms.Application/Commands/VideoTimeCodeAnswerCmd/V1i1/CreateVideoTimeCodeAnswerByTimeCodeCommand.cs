// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd.V1i1
{
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.VideoTimeCodeAnswers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Queries.VideoQuery;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MassTransit.Initializers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateVideoTimeCodeAnswerByTimeCodeCommand : CreateVideoTimeCodeAnswerV1i1CommandModel, IRequest<MethodResult<VideoTimeCodeModel>>
    {
    }

    public class CreateVideoTimeCodeAnswerByTimeCodeCommandHandler : IRequestHandler<CreateVideoTimeCodeAnswerByTimeCodeCommand, MethodResult<VideoTimeCodeModel>>
    {
        private readonly IVideoTimeCodeAnswerRepository _videoTimeCodeAnswerRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly VideoConverter _videoConverter;
        private readonly IMediator _mediator;
        private readonly DateTimeConverter _dateTimeConverter;
        private readonly IQuestionRepository _questionRepository;
        private readonly QuestionConverter _questionConverter;

        public CreateVideoTimeCodeAnswerByTimeCodeCommandHandler(
             IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository
            , IVideoResultRepository videoResultRepository
            , IExerciseRepository exerciseRepository
            , IVideoTimeCodeResultRepository videoTimeCodeResultRepository
            , IVideoTimeCodeRepository videoTimeCodeRepository
            , VideoConverter videoConverter
            , IMediator mediator
            , DateTimeConverter dateTimeConverter
            , IQuestionRepository questionRepository
            , QuestionConverter questionConverter)
        {
            _videoTimeCodeAnswerRepository = videoTimeCodeAnswerRepository;
            _videoResultRepository = videoResultRepository;
            _exerciseRepository = exerciseRepository;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _videoConverter = videoConverter;
            _mediator = mediator;
            _dateTimeConverter = dateTimeConverter;
            _questionRepository = questionRepository;
            _questionConverter = questionConverter;
        }

        public async Task<MethodResult<VideoTimeCodeModel>> Handle(CreateVideoTimeCodeAnswerByTimeCodeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<VideoTimeCodeModel>();
            var method = await CreateAnswer(request, cancellationToken);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }
            var (videoResult, videoTimeCode, videoTimeCodeResult) = method.Result;
            await _videoResultRepository.ExecuteTransactionAsync(async () =>
            {
                if (videoTimeCodeResult.Status == EnumResultStatus.New && request.IsSubmit)
                {
                    if (videoTimeCode.TimeCodeType == EnumTimeCodeType.Standalone)
                    {
                        videoResult.HighestStreak = await _videoConverter.GetHighestStreak(videoResult);
                    }
                    else
                    {
                        videoTimeCodeResult.HighestStreak = await _videoConverter.GetHighestStreak(videoTimeCodeResult);
                    }
                }

                await UpdateVideoTimeCodeResult(videoTimeCode, videoTimeCodeResult, request.IsSubmit, cancellationToken).ConfigureAwait(false);
                _videoResultRepository.Update(videoResult);
                await _videoResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });
            var videoTimeCodeMethod = await _mediator.Send(new GetTimeCodeDetailQuery { VideoTimeCodeId = request.VideoTimeCodeId, VideoId = videoResult.VideoId, LessonResultId = videoResult.LessonResultId, isShowSubStatus = videoTimeCodeResult.Status == EnumResultStatus.Process && request.IsSubmit, IsCreateAnswer = true }, cancellationToken);
            methodResult.Result = videoTimeCodeMethod.Result;
            return methodResult;
        }

        private async Task<MethodResult<(VideoResult, VideoTimeCode, VideoTimeCodeResult)>> CreateAnswer(CreateVideoTimeCodeAnswerByTimeCodeCommand request, CancellationToken cancellationToken)
        {
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
            foreach (var item in request.Answers)
            {
                var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                var exercise = question?.ExerciseQuestions.Select(x => x.Exercise).FirstOrDefault();
                var answer = await _videoTimeCodeAnswerRepository.GetAsync(videoTimeCode.Id, videoTimeCodeResult.VideoResultId, question?.Id, exercise?.Id ?? default);
                var questionResult = _questionConverter.HandleQuestionAnswer(question, item.Answer, request.IsSubmit, answer?.Answer, videoTimeCodeResult.Status == EnumResultStatus.Process, GetMandatoryAnswer(videoTimeCode, videoTimeCodeResult));
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
                    videoTimeCodeAnswers.Add(GetVideoTimeCodeAnswer(answer, questionItem, correctCount, answerConfig ?? item.Answer, request.IsSubmit, videoTimeCodeResult.Status));
                }
                else if (answer.Status != EnumAnswerStatus.Done)
                {
                    updateVideoTimeCodeAnswers.Add(GetVideoTimeCodeAnswer(answer, questionItem, correctCount, answerConfig ?? item.Answer, request.IsSubmit, videoTimeCodeResult.Status));
                }
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

        private static VideoTimeCodeAnswer GetVideoTimeCodeAnswer(VideoTimeCodeAnswer answer, Question question, int correctCount, object? answerConfig, bool isSubmit, EnumResultStatus status)
        {
            answer.Answer = answerConfig;
            answer.CorrectCount = correctCount;
            answer.Status = GetAnswerStatus(isSubmit, correctCount, question.CorrectTotal);
            answer.IsCorrect = answer.Status == EnumAnswerStatus.Done;
            answer.IsFirstSubmit = status == EnumResultStatus.New;
            return answer;
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
                videoTimeCodeResult.WorkingTime = _dateTimeConverter.GetWorkingTime(videoTimeCodeResult.WorkingTime, videoTimeCode.ExecutionTime, videoTimeCodeResult);
            }
            else if (videoTimeCodeResult.Status == EnumResultStatus.Process)
            {
                videoTimeCodeResult.RetryWorkingTime = _dateTimeConverter.GetWorkingTime(videoTimeCodeResult.RetryWorkingTime, videoTimeCode.ExecutionTime, videoTimeCodeResult);
            }
            if (isSubmit)
            {
                var (listSkillScore, skillScores, isDone) = await GetSkillScoresAsync(videoTimeCodeResult, cancellationToken);
                videoTimeCodeResult.Status = isDone ? EnumResultStatus.Process : EnumResultStatus.Done;
                videoTimeCodeResult.CorrectCountUngraded = (int)listSkillScore.Sum(x => x.CorrectCount);
                videoTimeCodeResult.CorrectTotalUngraded = (int)listSkillScore.Sum(x => x.TotalCount);
                videoTimeCodeResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
                videoTimeCodeResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
                videoTimeCodeResult.SkillScores = skillScores;
                videoTimeCodeResult.SkillScoreUngraded = listSkillScore;
            }
            videoTimeCodeResult.IsWorking = false;
            return videoTimeCodeResult;
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
            var methodResult = new MethodResult<(VideoResult, VideoTimeCode, VideoTimeCodeResult, IList<Question>)>();
            var videoResult = await _videoResultRepository.GetByIdAsync(request.VideoResultId);
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
            var videoTimeCodeResult = await _videoTimeCodeResultRepository.Queryable.FirstOrDefaultAsync(x => x.VideoResultId == videoResult.Id && x.VideoTimeCodeId == videoTimeCode.Id, cancellationToken);
            if (videoTimeCodeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoTimeCodeResult));
                return methodResult;
            }
            if (videoTimeCodeResult.Status == EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusDone), nameof(videoTimeCodeResult));
                return methodResult;
            }

            if (GetMandatoryAnswer(videoTimeCode, videoTimeCodeResult) && request.IsSubmit && (request.Answers == null || !request.Answers.Any()))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Answers));
                return methodResult;
            }

            videoResult.CurrentVideoTimeCodeId = videoTimeCode.Id;
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
                if (timeCodeId != videoTimeCode.Id)
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

        private bool GetMandatoryAnswer(VideoTimeCode videoTimeCode, VideoTimeCodeResult videoTimeCodeResult)
        {
            double workingTime = default;
            if (videoTimeCodeResult.Status == EnumResultStatus.New)
            {
                workingTime = _dateTimeConverter.GetWorkingTime(videoTimeCodeResult.WorkingTime, videoTimeCode.ExecutionTime, videoTimeCodeResult);
            }
            else if (videoTimeCodeResult.Status == EnumResultStatus.Process)
            {
                workingTime = _dateTimeConverter.GetWorkingTime(videoTimeCodeResult.RetryWorkingTime, videoTimeCode.ExecutionTime, videoTimeCodeResult);
            }
            return videoTimeCode.ExecutionTime == default || (workingTime < videoTimeCode.ExecutionTime && videoTimeCode.TimeCodeType == EnumTimeCodeType.Standalone);
        }

        private async Task<(IList<SkillScores>, IList<SkillScores>, bool)> GetSkillScoresAsync(VideoTimeCodeResult videoTimeCodeResult, CancellationToken cancellationToken)
        {
            var exerciseIds = await _videoTimeCodeAnswerRepository.Queryable.Where(x => x.VideoTimeCodeResultId == videoTimeCodeResult.Id).Select(x => x.ExerciseId).Distinct().ToListAsync(cancellationToken);
            var exercises = await _exerciseRepository.Queryable.Include(x => x.ExerciseQuestions)
                                    .ThenInclude(x => x.Question)
                                    .ThenInclude(x => x!.VideoTimeCodeAnswers.Where(x => x.VideoTimeCodeResultId == videoTimeCodeResult.Id))
                                    .Where(x => exerciseIds.Contains(x.Id)).ToListAsync(cancellationToken);
            var isDone = exercises.SelectMany(x => x.ExerciseQuestions)
                                    .Select(x => x.Question)
                                    .SelectMany(x => x!.VideoTimeCodeAnswers)
                                    .Any(x => x.Status != EnumAnswerStatus.Done);
            var skillScores = exercises.GroupBy(x => x.CourseSkill).Select(x => GetSkillScores(x));
            return (skillScores.Select(x => x.Item1).ToList(), skillScores.Select(x => x.Item2).ToList(), isDone);
        }

        private static (SkillScores, SkillScores) GetSkillScores(IGrouping<EnumCourseSkill, Exercise> exercise)
        {
            ArgumentNullException.ThrowIfNull(exercise);
            var questions = exercise.SelectMany(x => x.ExerciseQuestions).Select(x => x.Question);
            return (GetSkillScore(questions.Where(x => x != null && x.Ungraded).ToList(), exercise.Key), GetSkillScore(questions.ToList(), exercise.Key));
        }

        private static SkillScores GetSkillScore(IList<Question?>? questions, EnumCourseSkill courseSkill)
        {
            var answers = questions?.SelectMany(x => x!.VideoTimeCodeAnswers);
            var correctCount = answers?.Sum(x => x.CorrectCount) ?? default;
            return new SkillScores
            {
                CorrectCount = correctCount,
                CountQuestion = answers?.Count() ?? default,
                Skill = courseSkill,
                TotalCount = questions?.Sum(x => x!.CorrectTotal) ?? default,
                TotalQuestion = questions?.Count ?? default,
                Scores = correctCount.GetIeltsScore(courseSkill)
            };
        }
    }
}
