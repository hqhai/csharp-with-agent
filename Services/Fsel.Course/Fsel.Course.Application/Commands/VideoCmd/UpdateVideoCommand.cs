// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.Videos;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Infrastructure.Common;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Course.Application.Commands.VideoCmd
{
    public class UpdateVideoCommand : UpdateVideoCommandModel, IRequest<MethodResult<VideoModel>>
    {
    }

    public class UpdateVideoCommandHandler : IRequestHandler<UpdateVideoCommand, MethodResult<VideoModel>>
    {
        private readonly IVideoRepository _videoRepository;
        private readonly IMapper _mapper;
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IQuestionRepository _questionRepository;

        public UpdateVideoCommandHandler(IVideoRepository videoRepository
            , IMapper mapper
            , QuestionTypeConverter questionTypeConverter
            , IExerciseRepository exerciseRepository
            , IQuestionRepository questionRepository)
        {
            _videoRepository = videoRepository;
            _mapper = mapper;
            _questionTypeConverter = questionTypeConverter;
            _exerciseRepository = exerciseRepository;
            _questionRepository = questionRepository;
        }

        public async Task<MethodResult<VideoModel>> Handle(UpdateVideoCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoModel> methodResult = new MethodResult<VideoModel>();

            #region Validation

            if (request.VideoTimeCodes == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(nameof(EnumVideoTimeCodeErrorCode.DisplayTimeGreaterThan1));
                return methodResult;
            }

            #region Tạm thời không validate isTeacher

            //var isTeacher = await _userService.GetTeacherByIdAsync(request.TeacherId);
            //var isCheck = isTeacher?.Content?.Result;
            //if (isCheck == null)
            //{
            //    methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.TeacherIdDoesNotExitst), nameof(request.TeacherId));
            //    return methodResult;
            //}

            #endregion Tạm thời không validate isTeacher

            var isVideoUsed = await _videoRepository.IsVideoUsed(request.Id);
            if (isVideoUsed)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.VideoUsed), nameof(request.Id), request.Id);
                return methodResult;
            }

            var listTimeCodeType = request.VideoTimeCodes.Select(x => x.TimeCodeType).ToList();

            if (listTimeCodeType.Contains(EnumTimeCodeType.UnitTest) && listTimeCodeType.Contains(EnumTimeCodeType.SkillTest))
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.CanNotUnitTestAndSkillTestAtTheSameTime), nameof(request.VideoTimeCodes));
                return methodResult;
            }

            // Lưu dữ liệu Video
            var video = await _videoRepository.GetIncludeByIdAsync(request.Id);
            if (video == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(nameof(EnumVideoErrorCode.VideoNotCorrect));
                return methodResult;
            }

            List<Exercise> exercises = video.VideoTimeCodes.SelectMany(x => x.TimeCodeExercises).Select(x => x.Exercise!).ToList();
            List<Question> questions = exercises.SelectMany(x => x.ExerciseQuestions).Select(x => x.Question!).ToList();
            _mapper.Map(request, video);

            video.VideoTimeCodes = new List<VideoTimeCode>();
            foreach (var timeCode in request.VideoTimeCodes)
            {
                if (timeCode == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumVideoTimeCodeErrorCode.VideoTimeCodeNotCorrect));
                }
                var newTimeCode = _mapper.Map<VideoTimeCode>(timeCode);
                newTimeCode.TimeCodeExercises = new List<TimeCodeExercise>();
                if (timeCode!.Exercises == null || timeCode.Exercises.Count == 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumExerciseErrorCode.ExercisesNull), nameof(timeCode.Exercises), timeCode.Exercises);
                }
                foreach (var exercise in timeCode.Exercises!)
                {
                    if (exercise == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumExerciseErrorCode.ExerciseNull));
                    }
                    var newExercise = _mapper.Map<Exercise>(exercise);
                    newExercise.ExerciseQuestions = new List<ExerciseQuestion>();
                    if (exercise!.Questions == null || exercise.Questions.Count == 0)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionNull), nameof(exercise.Questions), exercise.Questions);
                    }
                    foreach (var question in exercise.Questions!)
                    {
                        if (question == null)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionNotCorrect));
                        }
                        var newQuestion = _mapper.Map<Question>(question);
                        newExercise.ExerciseQuestions.Add(new ExerciseQuestion
                        {
                            Exercise = newExercise,
                            Question = newQuestion
                        });
                        var (config, correctTotal) = _questionTypeConverter.QuestionTypeConverterObject(question!.Config, question.QuestionType, isShowCorrectTotal: !question.Ungraded, false);
                        if (config == null)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.ConfigIsInTheWrongFormat), nameof(question.Config));
                        }
                        newQuestion.CorrectTotal = correctTotal;
                        if (!newQuestion.IsValid())
                        {
                            methodResult.StatusCode = StatusCodes.Status400BadRequest;
                            methodResult.AddResultFromErrorList(newQuestion.ErrorMessages);
                        }
                    }
                    newTimeCode.TimeCodeExercises.Add(new TimeCodeExercise
                    {
                        Exercise = newExercise
                    });
                    if (!newExercise.IsValid())
                    {
                        methodResult.StatusCode = StatusCodes.Status400BadRequest;
                        methodResult.AddResultFromErrorList(newExercise.ErrorMessages);
                    }
                }
                video.VideoTimeCodes.Add(newTimeCode);
                if (!newTimeCode.IsValid())
                {
                    methodResult.StatusCode = StatusCodes.Status400BadRequest;
                    methodResult.AddResultFromErrorList(newTimeCode.ErrorMessages);
                }
            }
            if (!video.IsValid())
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddResultFromErrorList(video.ErrorMessages);
                return methodResult;
            }
            else if (!methodResult.IsOK)
            {
                return methodResult;
            }

            #endregion Validation

            await _videoRepository.ExecuteTransactionAsync(async () =>
            {
                foreach (var item in exercises)
                {
                    await _exerciseRepository.DeleteAsync(item);
                }
                await _exerciseRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                foreach (var item in questions)
                {
                    await _questionRepository.DeleteAsync(item);
                }
                await _questionRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                video = _videoRepository.Update(video);
                await _videoRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<VideoModel>(video);
                return methodResult;
            });

            return methodResult;
        }
    }
}
