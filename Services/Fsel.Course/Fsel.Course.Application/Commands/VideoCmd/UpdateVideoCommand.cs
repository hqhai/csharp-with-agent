// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Course.Domain.Entities;
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
        private readonly QuestionTypeCountConverter _questionTypeCountConverter;
        private readonly QuestionTypeValidation _questionTypeValidation;

        public UpdateVideoCommandHandler(IVideoRepository videoRepository
            , IMapper mapper
            , QuestionTypeCountConverter questionTypeCountConverter
            , QuestionTypeValidation questionTypeValidation
            )
        {
            _videoRepository = videoRepository;
            _mapper = mapper;
            _questionTypeCountConverter = questionTypeCountConverter;
            _questionTypeValidation = questionTypeValidation;
        }

        public async Task<MethodResult<VideoModel>> Handle(UpdateVideoCommand request, CancellationToken cancellationToken)
        {
            MethodResult<VideoModel> methodResult = new MethodResult<VideoModel>();
            ArgumentNullException.ThrowIfNull(request);

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

            #region Tạm thời không validate VideoUsed

            //var isVideoUsed = await _videoRepository.IsVideoUsed(request.Id);
            //if (isVideoUsed)
            //{
            //    methodResult.AddErrorBadRequest(
            //        nameof(EnumVideoErrorCode.VideoUsed),
            //        nameof(request.Id), request.Id);
            //    return methodResult;
            //}

            #endregion Tạm thời không validate VideoUsed

            // Lưu dữ liệu Video
            var video = await _videoRepository.GetIncludeByIdAsync(request.Id);
            if (video == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(nameof(EnumVideoErrorCode.VideoNotCorrect));
                return methodResult;
            }
            video = _mapper.Map(request, video);

            request.VideoTimeCodes.ForEach(x =>
            {
                if (x == null)
                {
                    methodResult.StatusCode = StatusCodes.Status400BadRequest;
                    methodResult.AddError(nameof(EnumVideoTimeCodeErrorCode.VideoTimeCodeNotCorrect));
                }
                else
                {
                    VideoTimeCode videoTimeCode = video.VideoTimeCodes.ElementAt(request.VideoTimeCodes.IndexOf(x));
                    x.Exercises.ForEach(n =>
                    {
                        if (n == null)
                        {
                            methodResult.StatusCode = StatusCodes.Status400BadRequest;
                            methodResult.AddError(nameof(EnumExerciseErrorCode.ExerciseNull));
                        }
                        else
                        {
                            Exercise excercise = _mapper.Map<Exercise>(n);
                            videoTimeCode.TimeCodeExercises.Add(new TimeCodeExercise
                            {
                                Exercise = excercise
                            });
                            n.Questions.ForEach(q =>
                            {
                                if (q == null)
                                {
                                    methodResult.StatusCode = StatusCodes.Status400BadRequest;
                                    methodResult.AddError(nameof(EnumQuestionErrorCode.QuestionNotCorrect));
                                }
                                else
                                {
                                    Question question = _mapper.Map<Question>(q);
                                    var ischeck = _questionTypeValidation.TryParseQuestionType(question.Config, question.QuestionType);
                                    if (!ischeck)
                                    {
                                        methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.ConfigIsInTheWrongFormat), nameof(question.Config));
                                    }
                                    question.CorrectTotal = _questionTypeCountConverter.GetTotalCorrectByQuestionType(question.Config, question.QuestionType);
                                    excercise.ExerciseQuestions.Add(new ExerciseQuestion
                                    {
                                        Question = question
                                    });

                                    if (!question.IsValid())
                                    {
                                        methodResult.StatusCode = StatusCodes.Status400BadRequest;
                                        methodResult.AddResultFromErrorList(question.ErrorMessages);
                                    }
                                }
                            });

                            if (!excercise.IsValid())
                            {
                                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                                methodResult.AddResultFromErrorList(excercise.ErrorMessages);
                            }
                        }
                    });

                    if (!videoTimeCode.IsValid())
                    {
                        methodResult.StatusCode = StatusCodes.Status400BadRequest;
                        methodResult.AddResultFromErrorList(videoTimeCode.ErrorMessages);
                    }
                }
            });

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
