// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.Videos;
using Fsel.Course.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Course.Application.Commands.VideoCmd
{
    public class CreateVideoCommand : CreateVideoCommandModel, IRequest<MethodResult<VideoModel>>
    {
    }

    public class CreateVideoCommandHandler : IRequestHandler<CreateVideoCommand, MethodResult<VideoModel>>
    {
        private readonly IVideoRepository _videoRepository;
        private readonly IMapper _mapper;

        public CreateVideoCommandHandler(IVideoRepository videoRepository
            , IMapper mapper)
        {
            _videoRepository = videoRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<VideoModel>> Handle(CreateVideoCommand request, CancellationToken cancellationToken)
        {
            MethodResult<VideoModel> methodResult = new MethodResult<VideoModel>();

            #region Validation

            ArgumentNullException.ThrowIfNull(request);
            if (request.VideoTimeCodes == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.VideoNotCorrect), nameof(request.VideoTimeCodes));
                return methodResult;
            }

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
