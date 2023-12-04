// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd
{
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
        private readonly QuestionConverter _questionConverter;
        private readonly DateTimeConverter _dateTimeConverter;
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly FinishOneUnitTestPublisher _finishOneUnitTestPublisher;
        private readonly IQuestionRepository _questionRepository;

        public CreateVideoTimeCodeAnswerCommandHandler(
             IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository
            , IVideoResultRepository videoResultRepository
            , QuestionConverter questionConverter
            , DateTimeConverter dateTimeConverter
            , IVideoTimeCodeResultRepository videoTimeCodeResultRepository
            , FinishOneUnitTestPublisher finishOneUnitTestPublisher
            , IQuestionRepository questionRepository)
        {
            _videoTimeCodeAnswerRepository = videoTimeCodeAnswerRepository;
            _videoResultRepository = videoResultRepository;
            _questionConverter = questionConverter;
            _dateTimeConverter = dateTimeConverter;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _finishOneUnitTestPublisher = finishOneUnitTestPublisher;
            _questionRepository = questionRepository;
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

            double workingTime = 0;

            if (videoTimeCodeResult.Status == EnumResultStatus.New)
            {
                workingTime = _dateTimeConverter.GetWorkingTime(videoTimeCodeResult.WorkingTime, videoTimeCode.ExecutionTime, videoTimeCodeResult);
            }
            else if (videoTimeCodeResult.Status == EnumResultStatus.Process)
            {
                workingTime = _dateTimeConverter.GetWorkingTime(videoTimeCodeResult.RetryWorkingTime, videoTimeCode.ExecutionTime, videoTimeCodeResult);
            }
            var isMandatoryAnswer = videoTimeCode.ExecutionTime == 0 || (workingTime <= videoTimeCode.ExecutionTime && videoTimeCode.TimeCodeType == EnumTimeCodeType.Standalone);

            foreach (var item in request.Answers)
            {
                var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                var exercise = question?.ExerciseQuestions.Select(x => x.Exercise).FirstOrDefault();
                var exerciseId = exercise?.Id ?? default;
                var answer = await _videoTimeCodeAnswerRepository.GetAsync(videoTimeCodeId, videoResult.Id, question?.Id, exerciseId);
                var questionResult = _questionConverter.HandleQuestionAnswer(question, item.Answer, true, answer?.Answer, videoTimeCodeResult.Status == EnumResultStatus.Process, isMandatoryAnswer);
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
                        Answer = answerConfig ?? item.Answer,
                        VideoTimeCodeId = videoTimeCodeId,
                        ExerciseId = exerciseId,
                        QuestionId = questionItem.Id,
                        VideoTimeCodeResultId = videoTimeCodeResult.Id,
                        VideoResultId = videoResult.Id,
                        CorrectCount = questionItem.Ungraded ? default : correctCount,
                        Status = GetAnswerStatus(videoTimeCode.TimeCodeType, correctCount, questionItem.CorrectTotal),
                        IsCorrect = correctCount == questionItem.CorrectTotal,
                    };

                    videoTimeCodeAnswers.Add(answer);
                }
                else
                {
                    answer.Answer = answerConfig ?? item.Answer;
                    answer.Status = EnumAnswerStatus.Done;
                    answer.IsCorrect = correctCount == questionItem.CorrectTotal;
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
            skillScores = GetSkillScores(skillScores);
            videoTimeCodeResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
            videoTimeCodeResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
            videoTimeCodeResult.Percent = skillScores.Sum(x => x.CorrectCount).GetPercent(skillScores.Sum(x => x.TotalCount));
            videoTimeCodeResult.SkillScores = skillScores;
            await _videoTimeCodeAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                if (videoTimeCode.TimeCodeType == EnumTimeCodeType.UnitTest)
                {
                    await _finishOneUnitTestPublisher.Publish(videoResult, cancellationToken);
                }
                if (videoTimeCodeResult.Status == EnumResultStatus.New)
                {
                    videoTimeCodeResult.WorkingTime = _dateTimeConverter.GetWorkingTime(videoTimeCodeResult.WorkingTime, videoTimeCode.ExecutionTime, videoTimeCodeResult);
                }
                else if (videoTimeCodeResult.Status == EnumResultStatus.Process)
                {
                    videoTimeCodeResult.RetryWorkingTime = _dateTimeConverter.GetWorkingTime(videoTimeCodeResult.RetryWorkingTime, videoTimeCode.ExecutionTime, videoTimeCodeResult);
                }
                if (videoTimeCodeAnswers.Any())
                {
                    videoTimeCodeResult.Status = EnumResultStatus.Process;
                    if (videoTimeCode?.TimeCodeType != EnumTimeCodeType.Standalone || skillScores.Sum(x => x.TotalCount) == videoTimeCodeAnswers.Sum(x => x.CorrectCount))
                    {
                        videoTimeCodeResult.Status = EnumResultStatus.Done;
                    }

                    await _videoTimeCodeAnswerRepository.AddList(videoTimeCodeAnswers);
                    await _videoTimeCodeAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
                else if (updateVideoTimeCodeAnswers.Any())
                {
                    videoTimeCodeResult.Status = EnumResultStatus.Done;
                    _videoTimeCodeAnswerRepository.UpdateList(updateVideoTimeCodeAnswers);
                    await _videoTimeCodeAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
                _videoTimeCodeResultRepository.Update(videoTimeCodeResult);
                await _videoTimeCodeResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                _videoResultRepository.Update(videoResult);
                await _videoResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
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

        private async Task<(IList<Question>?, VideoTimeCode?)> GetQuestionsAndVideoTimeCodeAsyns(IList<Guid> questionIds)
        {
            var questions = await _questionRepository.GetIncludeTimeCodeByIdAsync(questionIds);
            var exercise = questions?.SelectMany(x => x.ExerciseQuestions).Select(x => x.Exercise).FirstOrDefault();
            var videoTimeCode = exercise?.TimeCodeExercises.Select(x => x.VideoTimeCode).FirstOrDefault();
            return (questions, videoTimeCode);
        }
    }
}
