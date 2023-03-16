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

            if (request.VideoTimeCodes == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(nameof(EnumVideoErrorCode.VD03V));
                return methodResult;
            }

            Video video = _mapper.Map<Video>(request);

            request.VideoTimeCodes.ForEach(x =>
            {
                if (x == null)
                {
                    methodResult.StatusCode = StatusCodes.Status400BadRequest;
                    methodResult.AddErrorMessage(nameof(EnumVideoTimeCodeErrorCode.VTC03V));
                }
                else
                {
                    VideoTimeCode videoTimeCode = video.VideoTimeCodes.ElementAt(request.VideoTimeCodes.IndexOf(x));
                    x.Excercises.ForEach(n =>
                    {
                        if (n == null)
                        {
                            methodResult.StatusCode = StatusCodes.Status400BadRequest;
                            methodResult.AddErrorMessage(nameof(EnumExcerciseErrorCode.E03V));
                        }
                        else
                        {
                            Excercise excercise = _mapper.Map<Excercise>(n);
                            videoTimeCode.TimeCodeExcercises.Add(new TimeCodeExcercise
                            {
                                Excercise = excercise
                            });
                            n.Questions.ForEach(q =>
                            {
                                if (q == null)
                                {
                                    methodResult.StatusCode = StatusCodes.Status400BadRequest;
                                    methodResult.AddErrorMessage(nameof(EnumQuestionErrorCode.Q03V));
                                }
                                else
                                {
                                    Question question = _mapper.Map<Question>(q);
                                    excercise.ExcerciseQuestions.Add(new ExcerciseQuestion
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
