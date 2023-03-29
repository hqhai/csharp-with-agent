// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Course.Application.Services.UserServices;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.Videos;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Infrastructure.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using static System.Collections.Specialized.BitVector32;

namespace Fsel.Course.Application.Commands.VideoCmd
{
    public class CreateVideoCommand : CreateVideoCommandModel, IRequest<MethodResult<VideoModel>>
    {
    }

    public class CreateVideoCommandHandler : IRequestHandler<CreateVideoCommand, MethodResult<VideoModel>>
    {
        private readonly IVideoRepository _videoRepository;
        private readonly QuestionTypeValidation _questionTypeValidation;
        private readonly QuestionTypeCountConverter _questionTypeCountConverter;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public CreateVideoCommandHandler(IVideoRepository videoRepository
            , QuestionTypeValidation questionTypeValidation
            , QuestionTypeCountConverter questionTypeCountConverter
            , IMapper mapper
            , IUserService userService)
        {
            _videoRepository = videoRepository;
            _questionTypeValidation = questionTypeValidation;
            _questionTypeCountConverter = questionTypeCountConverter;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<MethodResult<VideoModel>> Handle(CreateVideoCommand request, CancellationToken cancellationToken)
        {
            MethodResult<VideoModel> methodResult = new MethodResult<VideoModel>();
            ArgumentNullException.ThrowIfNull(request);

            #region Validation

            ArgumentNullException.ThrowIfNull(request);
            if (request.VideoTimeCodes == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.VideoNotCorrect), nameof(request.VideoTimeCodes));
                return methodResult;
            }
            //var isTeacher = await _userService.GetTeacherByIdAsync(request.TeacherId);
            //var isCheck = isTeacher?.Content?.Result;
            //if (isCheck == null)
            //{
            //    methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.TeacherIdDoesNotExitst), nameof(request.TeacherId));
            //    return methodResult;
            //}

            Video video = _mapper.Map<Video>(request);

            request.VideoTimeCodes.ForEach(x =>
            {
                if (x == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumVideoTimeCodeErrorCode.VideoTimeCodeNotCorrect), nameof(request.VideoTimeCodes));
                }
                else
                {
                    VideoTimeCode videoTimeCode = video.VideoTimeCodes.ElementAt(request.VideoTimeCodes.IndexOf(x));
                    x.Exercises.ForEach(n =>
                    {
                        if (n == null)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumExerciseErrorCode.ExerciseNull), nameof(x.Exercises));
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
                                    methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionNotCorrect), nameof(n.Questions));
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
                                        methodResult.AddErrorBadRequest(question.ErrorMessages);
                                    }
                                }
                            });

                            if (!excercise.IsValid())
                            {
                                methodResult.AddErrorBadRequest(excercise.ErrorMessages);
                            }
                        }
                    });

                    if (!videoTimeCode.IsValid())
                    {
                        methodResult.AddErrorBadRequest(videoTimeCode.ErrorMessages);
                    }
                }
            });

            if (!video.IsValid())
            {
                methodResult.AddErrorBadRequest(video.ErrorMessages);
                return methodResult;
            }
            else if (!methodResult.IsOK)
            {
                return methodResult;
            }

            #endregion Validation

            await _videoRepository.ExecuteTransactionAsync(async () =>
            {
                video = _videoRepository.Add(video);

                await _videoRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<VideoModel>(video);
                return methodResult;
            });

            return methodResult;
        }
    }
}
