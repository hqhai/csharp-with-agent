// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd
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
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Shared.Enums;
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
        private readonly IExerciseRepository _exerciseRepository;
        private readonly VideoConverter _videoConverter;
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly FinishOneUnitTestPublisher _finishOneUnitTestPublisher;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly QuestionConverter _questionConverter;

        public CreateVideoTimeCodeAnswerCommandHandler(
             IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository
            , IVideoResultRepository videoResultRepository
            , IExerciseRepository exerciseRepository
            , VideoConverter videoConverter
            , IVideoTimeCodeResultRepository videoTimeCodeResultRepository
            , FinishOneUnitTestPublisher finishOneUnitTestPublisher
            , IVideoTimeCodeRepository videoTimeCodeRepository
            , IQuestionRepository questionRepository
            , QuestionConverter questionConverter)
        {
            _videoTimeCodeAnswerRepository = videoTimeCodeAnswerRepository;
            _videoResultRepository = videoResultRepository;
            _exerciseRepository = exerciseRepository;
            _videoConverter = videoConverter;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _finishOneUnitTestPublisher = finishOneUnitTestPublisher;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _questionRepository = questionRepository;
            _questionConverter = questionConverter;
        }

        public async Task<MethodResult<bool>> Handle(CreateVideoTimeCodeAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

            var method = await Validate(request, cancellationToken);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }
            var (videoResult, videoTimeCode, videoTimeCodeResult, questions) = method.Result;

            #endregion Validation

            var videoTimeCodeAnswers = new List<VideoTimeCodeAnswer>();
            var updateVideoTimeCodeAnswers = new List<VideoTimeCodeAnswer>();
            var skillScores = new List<SkillScores>();
            foreach (var item in request.Answers)
            {
                var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                var questionResult = _questionConverter.HandleQuestionAnswer(question, item.Answer, videoTimeCode.ExecutionTime == 0);
                if (!questionResult.IsOK)
                {
                    methodResult.AddErrorBadRequest(questionResult.ErrorMessages);
                    return methodResult;
                }
                var (questionItem, answerConfig, correctCount) = questionResult.Result;
                var exercise = questionItem.ExerciseQuestions.Select(x => x.Exercise).FirstOrDefault();
                var exerciseId = exercise?.Id ?? default;
                var answer = await _videoTimeCodeAnswerRepository.GetAsync(videoTimeCode.Id, videoResult.Id, questionItem.Id, exerciseId);
                if (answer == null)
                {
                    answer = new VideoTimeCodeAnswer
                    {
                        Answer = answerConfig ?? item.Answer,
                        VideoTimeCodeId = videoTimeCode.Id,
                        ExerciseId = exerciseId,
                        QuestionId = questionItem.Id,
                        VideoTimeCodeResultId = videoTimeCodeResult.Id,
                        VideoResultId = videoResult.Id,
                        CorrectCount = questionItem.Ungraded ? default : correctCount,
                        Status = GetAnswerStatus(videoTimeCode.TimeCodeType, correctCount, questionItem.CorrectTotal)
                    };

                    videoTimeCodeAnswers.Add(answer);
                }
                else
                {
                    answer.Answer = answerConfig ?? item.Answer;
                    answer.Status = EnumAnswerStatus.Done;
                    answer.CorrectCount = questionItem.Ungraded ? default : correctCount;
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
            await _videoResultRepository.ExecuteTransactionAsync(async () =>
            {
                await SaveVideoTimeCodeAnswer(videoTimeCodeAnswers, updateVideoTimeCodeAnswers, videoTimeCode, videoTimeCodeResult, cancellationToken).ConfigureAwait(false);
                await UpdateVideoTimeCodeAnswer(videoTimeCode, videoTimeCodeResult, skillScores, request.IsSubmit, cancellationToken).ConfigureAwait(false);
                _videoResultRepository.Update(videoResult);
                await _videoResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                return methodResult;
            });
            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<IList<SkillScores>> GetSkillScores(VideoTimeCode videoTimeCode, VideoTimeCodeResult videoTimeCodeResult, CancellationToken cancellationToken)
        {
            var exerciseIds = await _videoTimeCodeAnswerRepository.Queryable.Where(x => x.VideoTimeCodeId == videoTimeCode.Id && x.VideoTimeCodeResultId == videoTimeCodeResult.Id && x.VideoResultId == videoTimeCodeResult.VideoResultId).Select(x => x.ExerciseId).Distinct().ToListAsync(cancellationToken);
            var exercises = await _exerciseRepository.Queryable.Include(x => x.ExerciseQuestions)
                                    .ThenInclude(x => x.Question)
                                    .ThenInclude(x => x.VideoTimeCodeAnswers)
                                    .Where(x => exerciseIds.Contains(x.Id))
                                    .ToListAsync(cancellationToken);
            var skillScores = exercises.GroupBy(x => x.CourseSkill).Select(x => GetSkillScore(x)).ToList();
            return skillScores;
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
            }).ToList();
        }

        private async Task UpdateVideoTimeCodeAnswer(VideoTimeCode videoTimeCode, VideoTimeCodeResult videoTimeCodeResult, IList<SkillScores> skillScores, bool isSubmit, CancellationToken cancellationToken)
        {
            if (isSubmit)
            {
                await _videoConverter.UpdateVideoAnswers(videoTimeCode, videoTimeCodeResult);
                skillScores = await GetSkillScores(videoTimeCode, videoTimeCodeResult, cancellationToken);
                videoTimeCodeResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
                videoTimeCodeResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
            }
            else
            {
                skillScores = GetSkillScores(skillScores);
            }
            videoTimeCodeResult.SkillScores = skillScores;
            videoTimeCodeResult.IsWorking = false;
            _videoTimeCodeResultRepository.Update(videoTimeCodeResult);
            await _videoTimeCodeResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
        }

        private static SkillScores GetSkillScore(IGrouping<EnumCourseSkill, Exercise> exercise)
        {
            ArgumentNullException.ThrowIfNull(exercise);
            var questions = exercise.SelectMany(x => x.ExerciseQuestions).Select(x => x.Question);
            var answers = questions.SelectMany(x => x!.VideoTimeCodeAnswers);
            var correctCount = answers.Sum(x => x.CorrectCount);
            return new SkillScores
            {
                CorrectCount = correctCount,
                CountQuestion = answers.Count(),
                Skill = exercise.Key,
                TotalCount = questions.Sum(x => x!.CorrectTotal),
                TotalQuestion = questions.Count(),
                Scores = correctCount.GetIeltsScore(exercise.Key)
            };
        }

        private async Task<VideoTimeCodeResult> SaveVideoTimeCodeAnswer(IList<VideoTimeCodeAnswer> videoTimeCodeAnswers, IList<VideoTimeCodeAnswer> updateVideoTimeCodeAnswers, VideoTimeCode videoTimeCode, VideoTimeCodeResult videoTimeCodeResult, CancellationToken cancellationToken)
        {
            if (videoTimeCodeAnswers.Any())
            {
                if (videoTimeCode.TimeCodeType != EnumTimeCodeType.Standalone || videoTimeCodeAnswers.All(x => x.Status == EnumAnswerStatus.Done))
                {
                    videoTimeCodeResult.Status = EnumResultStatus.Done;
                }
                await _videoTimeCodeAnswerRepository.AddList(videoTimeCodeAnswers);
            }
            else if (updateVideoTimeCodeAnswers.Any())
            {
                videoTimeCodeResult.Status = EnumResultStatus.Done;
                _videoTimeCodeAnswerRepository.UpdateList(updateVideoTimeCodeAnswers);
            }
            await _videoTimeCodeAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return videoTimeCodeResult;
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
                    StudentId = videoResult.StudentId
                };
                videoTimeCodeResult = _videoTimeCodeResultRepository.Add(videoTimeCodeResult);
                await _videoTimeCodeResultRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
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

        public async Task<MethodResult<(VideoResult, VideoTimeCode, VideoTimeCodeResult, IList<Question>)>> Validate(CreateVideoTimeCodeAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<(VideoResult, VideoTimeCode, VideoTimeCodeResult, IList<Question>)> methodResult = new MethodResult<(VideoResult, VideoTimeCode, VideoTimeCodeResult, IList<Question>)>();
            if (!request.Answers.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Answers));
                return methodResult;
            }

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
            var videoTimeCodeResult = await GetVideoTimeCodeResultAsync(videoResult, request.VideoTimeCodeId);
            videoResult.CurrentVideoTimeCodeId = request.VideoTimeCodeId;
            var questions = await _questionRepository.GetListAsync(request.Answers.Select(x => x.QuestionId).ToList());
            if (questions == null || !questions.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questions));
                return methodResult;
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
    }
}
