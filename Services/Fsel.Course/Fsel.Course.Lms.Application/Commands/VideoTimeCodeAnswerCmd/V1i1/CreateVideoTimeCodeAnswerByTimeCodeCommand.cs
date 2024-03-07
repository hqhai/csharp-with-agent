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
    using Fsel.Course.Lms.Application.Commands.VideoResultCmd;
    using Fsel.Course.Lms.Application.Queries.VideoQuery;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
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
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;
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
            , IUserService userService
            , ISystemService systemService
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
            _userService = userService;
            _systemService = systemService;
            _mediator = mediator;
            _dateTimeConverter = dateTimeConverter;
            _questionRepository = questionRepository;
            _questionConverter = questionConverter;
        }

        public async Task<MethodResult<VideoTimeCodeModel>> Handle(CreateVideoTimeCodeAnswerByTimeCodeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<VideoTimeCodeModel>();
            var method = await HandleAnswerAsync(request, cancellationToken);
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

                await UpdateVideoTimeCodeResult(videoTimeCode, videoTimeCodeResult, request.IsSubmit, cancellationToken);
                _videoResultRepository.Update(videoResult);
                await _videoResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });

            if (videoTimeCodeResult.Status == EnumResultStatus.Done)
            {
                await UpdateVideoResultAsync(videoResult, cancellationToken);
            }
            var videoTimeCodeMethod = await _mediator.Send(new GetTimeCodeDetailQuery
            {
                VideoTimeCodeId = request.VideoTimeCodeId,
                VideoId = videoResult.VideoId,
                LessonResultId = videoResult.LessonResultId,
                IsShowSubStatus = videoTimeCodeResult.Status == EnumResultStatus.Process && request.IsSubmit,
                IsCreateAnswer = true
            }, cancellationToken);
            methodResult.Result = videoTimeCodeMethod.Result;
            return methodResult;
        }

        private async Task<MethodResult<(VideoResult, VideoTimeCode, VideoTimeCodeResult)>> HandleAnswerAsync(CreateVideoTimeCodeAnswerByTimeCodeCommand request, CancellationToken cancellationToken)
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
            var isTimeFeft = GetMandatoryAnswer(videoTimeCode, videoTimeCodeResult);
            foreach (var item in request.Answers)
            {
                var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                var exercise = question?.ExerciseQuestions.Select(x => x.Exercise).FirstOrDefault();
                var answer = await _videoTimeCodeAnswerRepository.GetAsync(videoTimeCode.Id, videoTimeCodeResult.VideoResultId, question?.Id, exercise?.Id ?? default);
                var questionResult = _questionConverter.HandleQuestionAnswer(question, item.Answer, request.IsSubmit, answer?.Answer, videoTimeCodeResult.Status == EnumResultStatus.Process, isTimeFeft);
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
                        VideoTimeCodeId = videoTimeCode.Id,
                        ExerciseId = exercise?.Id ?? default,
                        QuestionId = questionItem.Id,
                        VideoTimeCodeResultId = videoTimeCodeResult.Id,
                        VideoResultId = videoTimeCodeResult.VideoResultId,
                    };
                    videoTimeCodeAnswers.Add(GetVideoTimeCodeAnswer(answer, questionItem, correctCount, answerConfig ?? item.Answer, request.IsSubmit, videoTimeCodeResult.Status, isAnswered));
                }
                else if (answer.Status != EnumAnswerStatus.Done)
                {
                    updateVideoTimeCodeAnswers.Add(GetVideoTimeCodeAnswer(answer, questionItem, correctCount, answerConfig ?? item.Answer, request.IsSubmit, videoTimeCodeResult.Status, isAnswered));
                }
            }
            if (videoTimeCodeAnswers.Any())
            {
                await _videoTimeCodeAnswerRepository.AddList(videoTimeCodeAnswers);
                await _videoTimeCodeAnswerRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
            else if (updateVideoTimeCodeAnswers.Any())
            {
                _videoTimeCodeAnswerRepository.UpdateList(updateVideoTimeCodeAnswers);
                await _videoTimeCodeAnswerRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
            return methodResult;
        }

        private async Task UpdateVideoResultAsync(VideoResult videoResult, CancellationToken cancellationToken)
        {
            var videoTimeCodes = await _videoTimeCodeRepository.Queryable.Include(x => x.VideoTimeCodeResults.Where(x => x.VideoResultId == videoResult.Id)).Where(x => x.VideoId == videoResult.VideoId).ToListAsync(cancellationToken);

            var videoTimeCodeResults = videoTimeCodes.SelectMany(x => x.VideoTimeCodeResults).Where(x => x.VideoResultId == videoResult.Id && x.Status == EnumResultStatus.Done).ToList();
            if (videoTimeCodes.Count == videoTimeCodeResults.Count)
            {
                await _mediator.Send(new ReviewLessonVideoCommand { LessonResultId = videoResult.LessonResultId }, cancellationToken);
            }
        }

        private static VideoTimeCodeAnswer GetVideoTimeCodeAnswer(VideoTimeCodeAnswer answer, Question question, int correctCount, object? answerConfig, bool isSubmit, EnumResultStatus status, bool isAnswered)
        {
            answer.Answer = answerConfig;
            answer.CorrectCount = correctCount;
            answer.Status = GetAnswerStatus(isSubmit, correctCount, question.CorrectTotal);
            answer.IsCorrect = isAnswered ? correctCount == question.CorrectTotal : null;
            answer.IsFirstSubmit = status == EnumResultStatus.New;
            return answer;
        }

        private async Task UpdateVideoTimeCodeResult(VideoTimeCode videoTimeCode, VideoTimeCodeResult videoTimeCodeResult, bool isSubmit, CancellationToken cancellationToken)
        {
            videoTimeCodeResult = await GetVideoTimeCodeResultAsync(videoTimeCodeResult, videoTimeCode, isSubmit, cancellationToken);
            if (videoTimeCodeResult.Status == EnumResultStatus.Done)
            {
                var tokens = new List<int?> { videoTimeCodeResult.TokenDone, videoTimeCodeResult.TokenHighestStreak, videoTimeCodeResult.TokenQuestionReward, videoTimeCodeResult.TokenSuperFire };
                await _userService.UpdateStudentByTokenAsync(new UpdateStudentByTokenModel
                {
                    NumberOfToken = tokens.Where(x => x.HasValue).Sum(x => x!.Value),
                    StudentId = videoTimeCodeResult.StudentId,
                }).ConfigureAwait(false);
            }
            _videoTimeCodeResultRepository.Update(videoTimeCodeResult);
            await _videoTimeCodeResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task<VideoTimeCodeResult> GetVideoTimeCodeResultAsync(VideoTimeCodeResult videoTimeCodeResult, VideoTimeCode videoTimeCode, bool isSubmit, CancellationToken cancellationToken)
        {
            if (videoTimeCode.TimeCodeType != EnumTimeCodeType.Standalone)
            {
                videoTimeCodeResult.WorkingTime = _dateTimeConverter.GetWorkingTime(videoTimeCodeResult.WorkingTime, videoTimeCode.ExecutionTime, videoTimeCodeResult);
                videoTimeCodeResult.Status = EnumResultStatus.Process;
            }
            else
            {
                if (videoTimeCodeResult.Status == EnumResultStatus.New)
                {
                    videoTimeCodeResult.WorkingTime = _dateTimeConverter.GetWorkingTime(videoTimeCodeResult.WorkingTime, videoTimeCode.ExecutionTime, videoTimeCodeResult);
                }
                else if (videoTimeCodeResult.Status == EnumResultStatus.Process)
                {
                    videoTimeCodeResult.RetryWorkingTime = _dateTimeConverter.GetWorkingTime(videoTimeCodeResult.RetryWorkingTime, videoTimeCode.ExecutionTime, videoTimeCodeResult);
                }
            }

            if (isSubmit)
            {
                var (skillScoreUngradeds, skillScores, isDone) = await GetSkillScoresAsync(videoTimeCodeResult, cancellationToken);
                if (skillScoreUngradeds != null && skillScoreUngradeds.Any())
                {
                    videoTimeCodeResult.CorrectCountUngraded = (int)skillScoreUngradeds.Sum(x => x.CorrectCount);
                    videoTimeCodeResult.CorrectTotalUngraded = (int)skillScoreUngradeds.Sum(x => x.TotalCount);
                }
                videoTimeCodeResult.Status = isDone ? EnumResultStatus.Process : EnumResultStatus.Done;
                videoTimeCodeResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
                videoTimeCodeResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
                videoTimeCodeResult.SkillScores = skillScores;
                videoTimeCodeResult.SkillScoreUngraded = skillScoreUngradeds;
                videoTimeCodeResult = await GetTokenVideoTimeCodeResult(videoTimeCodeResult, videoTimeCode);
            }
            videoTimeCodeResult.IsWorking = false;
            return videoTimeCodeResult;
        }

        private async Task<VideoTimeCodeResult> GetTokenVideoTimeCodeResult(VideoTimeCodeResult videoTimeCodeResult, VideoTimeCode videoTimeCode)
        {
            var isSuperFireModeResult = await _userService.CheckSuperFireModeAsync();
            if (!isSuperFireModeResult.IsSuccessStatusCode)
            {
                return videoTimeCodeResult;
            }
            var tokenConfigs = await GetTokenConfig(videoTimeCode);
            var isSuperFireMode = isSuperFireModeResult.Content?.Result ?? default;
            if (videoTimeCode.TimeCodeType == EnumTimeCodeType.Standalone)
            {
                var configQuestionReward = tokenConfigs?.FirstOrDefault(x => x.Mission == EnumTokenMission.QuestionReward).GetTokenNumber<TokenNumber>(isSuperFireMode);
                videoTimeCodeResult.TokenQuestionReward = configQuestionReward?.Number * videoTimeCodeResult.CorrectCount;
            }
            else
            {
                var configDone = tokenConfigs?.FirstOrDefault(x => x.Mission == EnumTokenMission.TestDone).GetTokenNumber<TokenNumber>(isSuperFireMode);
                var configHighestStreak = tokenConfigs?.FirstOrDefault(x => x.Mission == EnumTokenMission.HighestStreak).GetTokenNumber<TokenNumber>(isSuperFireMode);
                videoTimeCodeResult.TokenDone = configDone?.Number;
                videoTimeCodeResult.TokenHighestStreak = configHighestStreak?.Number * videoTimeCodeResult.HighestStreak ?? default;
            }
            var configSuperFire = tokenConfigs?.FirstOrDefault(x => x.Mission == EnumTokenMission.SuperFire).GetTokenNumber<TokenNumber>(isSuperFireMode);
            videoTimeCodeResult.TokenSuperFire = configSuperFire?.Number;
            return videoTimeCodeResult;
        }

        private async Task<IList<TokenConfigModel>?> GetTokenConfig(VideoTimeCode videoTimeCode)
        {
            if (videoTimeCode.TimeCodeType == EnumTimeCodeType.Standalone)
            {
                var misstions = new List<string> { nameof(EnumTokenMission.QuestionReward), nameof(EnumTokenMission.SuperFire) };
                var tokenConfigResults = await _systemService.GetTokenConfigsAsync(new GetTokenConfigsQueryModel
                {
                    Feature = EnumTokenFeature.TimeCode,
                    Missions = string.Join(",", misstions)
                });
                return tokenConfigResults?.Content?.Result;
            }
            else
            {
                var misstions = new List<string> { nameof(EnumTokenMission.HighestStreak), nameof(EnumTokenMission.TestDone), nameof(EnumTokenMission.SuperFire) };
                var tokenConfigResults = await _systemService.GetTokenConfigsAsync(new GetTokenConfigsQueryModel
                {
                    Feature = videoTimeCode.TimeCodeType == EnumTimeCodeType.UnitTest ? EnumTokenFeature.UnitTest : EnumTokenFeature.SkillTest,
                    Missions = string.Join(",", misstions)
                });
                return tokenConfigResults?.Content?.Result;
            }
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
            return videoTimeCode.TimeCodeType == EnumTimeCodeType.Standalone && (videoTimeCode.ExecutionTime == default || workingTime < videoTimeCode.ExecutionTime);
        }

        private async Task<(IList<SkillScores>?, IList<SkillScores>, bool)> GetSkillScoresAsync(VideoTimeCodeResult videoTimeCodeResult, CancellationToken cancellationToken)
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
            return (skillScores.Where(x => x.Item1 != null).Select(x => x.Item1!).ToList(), skillScores.Where(x => x.Item2 != null).Select(x => x.Item2!).ToList(), isDone);
        }

        private static (SkillScores?, SkillScores?) GetSkillScores(IGrouping<EnumCourseSkill, Exercise> exercise)
        {
            ArgumentNullException.ThrowIfNull(exercise);
            var questions = exercise.SelectMany(x => x.ExerciseQuestions).Select(x => x.Question);
            return (GetSkillScore(questions.Where(x => x != null && x.Ungraded).ToList(), exercise.Key), GetSkillScore(questions.Where(x => x != null && !x.Ungraded).ToList(), exercise.Key));
        }

        private static SkillScores? GetSkillScore(IList<Question?>? questions, EnumCourseSkill courseSkill)
        {
            if (questions != null && questions.Any())
            {
                var answers = questions.SelectMany(x => x!.VideoTimeCodeAnswers);
                var correctCount = answers?.Sum(x => x.CorrectCount) ?? default;
                return new SkillScores
                {
                    CorrectCount = correctCount,
                    CountQuestion = answers?.Count() ?? default,
                    Skill = courseSkill,
                    TotalCount = questions.Sum(x => x!.CorrectTotal),
                    TotalQuestion = questions.Count,
                    Scores = correctCount.GetIeltsScore(courseSkill)
                };
            }
            return null;
        }
    }
}
