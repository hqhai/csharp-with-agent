// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd
{
    using System.Linq.Dynamic.Core;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.VideoTimeCodeAnswers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateVideoTimeCodeAnswerCommand : CreateVideoTimeCodeAnswerCommandModel, IRequest<MethodResult<VideoTimeCodeModel>>
    {
    }

    public class CreateVideoTimeCodeAnswerCommandHandler : IRequestHandler<CreateVideoTimeCodeAnswerCommand, MethodResult<VideoTimeCodeModel>>
    {
        private readonly IVideoTimeCodeAnswerRepository _videoTimeCodeAnswerRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IMapper _mapper;
        private readonly FinishOneUnitTestPublisher _finishOneUnitTestPublisher;
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly AnswerTypeConverter _answerTypeConverter;

        public CreateVideoTimeCodeAnswerCommandHandler(
             IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository
            , IVideoResultRepository videoResultRepository
            , ILessonResultRepository lessonResultRepository
            , IMapper mapper
            , FinishOneUnitTestPublisher finishOneUnitTestPublisher
            , QuestionTypeConverter questionTypeConverter
            , IVideoTimeCodeRepository videoTimeCodeRepository
            , IQuestionRepository questionRepository
            , AnswerTypeConverter answerTypeConverter)
        {
            _videoTimeCodeAnswerRepository = videoTimeCodeAnswerRepository;
            _videoResultRepository = videoResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _mapper = mapper;
            _finishOneUnitTestPublisher = finishOneUnitTestPublisher;
            _questionTypeConverter = questionTypeConverter;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _questionRepository = questionRepository;
            _answerTypeConverter = answerTypeConverter;
        }

        public async Task<MethodResult<VideoTimeCodeModel>> Handle(CreateVideoTimeCodeAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoTimeCodeModel> methodResult = new MethodResult<VideoTimeCodeModel>();

            #region Validation

            if (request.Answers == null || request.Answers.Any(x => x.Answer == null) || request.Answers.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Answers));
                return methodResult;
            }

            var lessonResult = await _lessonResultRepository.Queryable.Include(x => x.VideoResult).FirstOrDefaultAsync(x => x.Id == request.LessonResultId, cancellationToken);
            if (lessonResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lessonResult));
                return methodResult;
            }
            var videoResult = lessonResult.VideoResult;

            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult));
                return methodResult;
            }

            var questions = await _questionRepository.GetIncludeTimeCodeByIdAsync(request.Answers.Select(x => x.QuestionId).ToList());
            if (questions == null || questions.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questions));
                return methodResult;
            }

            var exercise = questions.SelectMany(x => x.ExerciseQuestions).Select(x => x.Exercise).FirstOrDefault();
            var videoTimeCodeQuestion = exercise?.TimeCodeExercises.Select(x => x.VideoTimeCode).FirstOrDefault();

            #region Chặn Time Code Chưa Done

            //var videoTimeCode = await _videoTimeCodeRepository.Queryable.Include(x => x.VideoTimeCodeAnswers.Where(x => x.VideoResultId == videoResult.Id)).Where(x => x.Id == videoResult.CurrentVideoTimeCodeId).FirstOrDefaultAsync(cancellationToken);
            //if (videoTimeCode != null && videoTimeCode.Id != videoTimeCodeQuestion?.Id && videoTimeCode.VideoTimeCodeAnswers.Any() && videoTimeCode.VideoTimeCodeAnswers.All(x => x.Status == EnumCurrentStatus.Process))
            //{
            //    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(EnumVideoTimeCodeErrorCode.VideoTimeCodePreviousNotDone));
            //    return methodResult;
            //}

            #endregion Chặn Time Code Chưa Done

            videoResult.CurrentVideoTimeCodeId = videoTimeCodeQuestion?.Id ?? default;

            #endregion Validation

            var videoTimeCodeAnswers = new List<VideoTimeCodeAnswer>();
            var updateVideoTimeCodeAnswers = new List<VideoTimeCodeAnswer>();
            int correctTotal = 0;
            foreach (var item in request.Answers)
            {
                var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                if (question == null || question.Config == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(question));
                    return methodResult;
                }
                var exerciseId = question.ExerciseQuestions.Select(x => x.Exercise).FirstOrDefault()?.Id ?? default;
                var answer = await _videoTimeCodeAnswerRepository.GetAsync(videoResult.Id, question.Id, exerciseId, videoTimeCodeQuestion?.Id);
                var (answerConfig, correctCount) = _answerTypeConverter.GetTotalCorrectByAsnwerType(item.Answer, question.Config, question.QuestionType);
                if (!string.IsNullOrEmpty(item.Answer?.ToString()) && answerConfig == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumVideoTimeCodeAnswerErrorCode.AnswerIsInTheWrongFormat), nameof(item.Answer), item.Answer);
                    return methodResult;
                }
                if (answer == null)
                {
                    answer = new VideoTimeCodeAnswer
                    {
                        Answer = answerConfig ?? item.Answer,
                        VideoTimeCodeId = videoTimeCodeQuestion?.Id ?? default,
                        ExerciseId = exerciseId,
                        QuestionId = question.Id,
                        VideoResultId = videoResult.Id,
                        CorrectCount = question.Ungraded ? default : correctCount,
                        Status = GetEnumTimeCodeType(videoTimeCodeQuestion?.TimeCodeType, correctCount, correctTotal)
                    };

                    videoTimeCodeAnswers.Add(answer);
                }
                else
                {
                    answer.Answer = answerConfig ?? item.Answer;
                    answer.Status = EnumCurrentStatus.Done;
                    answer.CorrectCount = question.Ungraded ? default : correctCount;
                    updateVideoTimeCodeAnswers.Add(answer);
                }
                correctTotal += question.CorrectTotal;
            }

            await _videoTimeCodeAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                if (videoTimeCodeQuestion?.TimeCodeType == EnumTimeCodeType.UnitTest)
                {
                    await _finishOneUnitTestPublisher.Publish(videoResult, cancellationToken);
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
                _videoResultRepository.Update(videoResult);
                await _videoResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                return methodResult;
            });
            var videoTimeCode = await _videoTimeCodeRepository.Queryable.Include(x => x.TimeCodeExercises)
                .ThenInclude(x => x.Exercise)
                .ThenInclude(x => x!.ExerciseQuestions)
                .ThenInclude(x => x.Question)
                .ThenInclude(x => x!.VideoTimeCodeAnswers.Where(x => x.VideoResultId == videoResult.Id))
                .FirstOrDefaultAsync(x => x.Id == videoResult.CurrentVideoTimeCodeId, cancellationToken);
            var videoTimeCodeModel = videoTimeCode != null ? new VideoTimeCodeModel
            {
                Id = videoTimeCode.Id,
                TotalCount = videoTimeCode.TimeCodeExercises.Where(x => !x.IsDeleted && x.Exercise != null).Select(x => x.Exercise).SelectMany(x => x!.ExerciseQuestions.Where(x => !x.IsDeleted && x.Question != null)).OrderBy(x => x!.CreatedDate).Select(m => m.Question).Count(),
                DisplayTime = videoTimeCode.DisplayTime,
                ExecutionTime = videoTimeCode.ExecutionTime,
                TimeCodeType = videoTimeCode.TimeCodeType,
                VideoId = videoTimeCode.VideoId,
                Ungraded = videoTimeCode.TimeCodeExercises.Select(x => x.Exercise).SelectMany(x => x!.ExerciseQuestions).Select(x => x.Question).FirstOrDefault()!.Ungraded,
                CorrectCount = videoTimeCode.VideoTimeCodeAnswers.Count > 0 ? videoTimeCode.VideoTimeCodeAnswers.Sum(x => x.CorrectCount) : 0,
                CorrectTotal = videoTimeCode.TimeCodeExercises.Select(x => x.Exercise).SelectMany(x => x!.ExerciseQuestions).Select(x => x.Question).Sum(x => x!.CorrectTotal),
                Status = (videoTimeCode.VideoTimeCodeAnswers.Count > 0 && videoTimeCode.VideoTimeCodeAnswers.All(y => videoResult != null && y.VideoResultId == videoResult.Id && y.Status == EnumCurrentStatus.Done)) ? EnumCurrentStatus.Done : EnumCurrentStatus.Process,
                Exercises = videoTimeCode.TimeCodeExercises.Where(n => !n.IsDeleted && n.Exercise != null).OrderBy(x => x!.CreatedDate).Select(n => n.Exercise).Select(n => new ExerciseModel
                {
                    Id = n!.Id,
                    MediaPost = n.MediaPost,
                    CourseSkill = n.CourseSkill,
                    Questions = n.ExerciseQuestions.Where(m => m.Question != null).OrderBy(x => x!.CreatedDate).Select(m => m.Question).Select(m => new QuestionModel()
                    {
                        Id = m!.Id,
                        QuestionType = m.QuestionType,
                        CorrectTotal = m.CorrectTotal,
                        Explanation = m.Explanation,
                        Ungraded = m.Ungraded,
                        Config = _questionTypeConverter.QuestionTypeConverterObject(m.Config, m.QuestionType, isDisableAnswers: !(m.VideoTimeCodeAnswers?.FirstOrDefault()?.Status == EnumCurrentStatus.Done)).Item1,
                        ResultAnswer = _mapper.Map<AnswerModel>(m.VideoTimeCodeAnswers?.FirstOrDefault(x => x.VideoResultId == videoResult.Id))
                    }).ToList(),
                }).ToList(),
            } : null;
            methodResult.Result = videoTimeCodeModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static EnumCurrentStatus GetEnumTimeCodeType(EnumTimeCodeType? timeCodeType, int correctCount, int correctTotal)
        {
            if (timeCodeType == EnumTimeCodeType.Standalone)
            {
                if (correctCount == correctTotal)
                {
                    return EnumCurrentStatus.Done;
                }
                else
                {
                    return EnumCurrentStatus.Process;
                }
            }
            else
            {
                return EnumCurrentStatus.Done;
            }
        }
    }
}
