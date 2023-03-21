using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.Videos;
using Fsel.Course.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Commands.VideoCmd
{
    public class UpdateVideoCommand : UpdateVideoCommandModel, IRequest<MethodResult<VideoModel>>
    {
    }

    public class UpdateVideoCommandHandler : IRequestHandler<UpdateVideoCommand, MethodResult<VideoModel>>
    {
        private readonly IVideoRepository _videoRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IExerciseRepository _excerciseRepository;
        private readonly IExerciseQuestionRepository _excerciseQuestionRepository;
        private readonly ITimeCodeExerciseRepository _timeCodeExerciseRepository;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly IMapper _mapper;

        public UpdateVideoCommandHandler(IVideoRepository videoRepository
            , IQuestionRepository questionRepository
            , IExerciseRepository excerciseRepository
            , IExerciseQuestionRepository excerciseQuestionRepository
            , ITimeCodeExerciseRepository timeCodeExerciseRepository
            , IVideoTimeCodeRepository videoTimeCodeRepository
            , IMapper mapper)
        {
            _videoRepository = videoRepository;
            _questionRepository = questionRepository;
            _excerciseRepository = excerciseRepository;
            _excerciseQuestionRepository = excerciseQuestionRepository;
            _timeCodeExerciseRepository = timeCodeExerciseRepository;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<VideoModel>> Handle(UpdateVideoCommand request, CancellationToken cancellationToken)
        {
            MethodResult<VideoModel> methodResult = new MethodResult<VideoModel>();

            #region Validation

            if (request.VideoTimeCodes == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(nameof(EnumVideoTimeCodeErrorCode.VTC04C));
                return methodResult;
            }

            // Lưu dữ liệu Video
            var video = await _videoRepository.GetIncludeByIdAsync(request.Id);
            if (video == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(nameof(EnumVideoErrorCode.VD03V));
                return methodResult;
            }
            video = _mapper.Map(request, video);

            var isVideoUsed = await _videoRepository.IsVideoUsed(request.Id);

            if (isVideoUsed)
            {
                methodResult.AddErrorBadRequest(
                    nameof(EnumVideoErrorCode.VD02V),
                    nameof(request.Id), request.Id);
                return methodResult;
            }

            request.VideoTimeCodes.ForEach(x =>
            {
                if (x == null)
                {
                    methodResult.StatusCode = StatusCodes.Status400BadRequest;
                    methodResult.AddError(nameof(EnumVideoTimeCodeErrorCode.VTC03V));
                }
                else
                {
                    VideoTimeCode videoTimeCode = video.VideoTimeCodes.ElementAt(request.VideoTimeCodes.IndexOf(x));
                    x.Exercises.ForEach(n =>
                    {
                        if (n == null)
                        {
                            methodResult.StatusCode = StatusCodes.Status400BadRequest;
                            methodResult.AddError(nameof(EnumExerciseErrorCode.E03V));
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
                                    methodResult.AddError(nameof(EnumQuestionErrorCode.Q03V));
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
