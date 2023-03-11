using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Course.Common.Models.Commands.Videos;
using Fsel.Course.Common.Models.Entities;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
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
        private readonly IQuestionRepository _questionRepository;
        private readonly IExcerciseRepository _excerciseRepository;
        private readonly IExcerciseQuestionRepository _excerciseQuestionRepository;
        private readonly ITimeCodeExcerciseRepository _timeCodeExcerciseRepository;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly IMapper _mapper;

        public CreateVideoCommandHandler(IVideoRepository videoRepository
            , IQuestionRepository questionRepository
            , IExcerciseRepository excerciseRepository
            , IExcerciseQuestionRepository excerciseQuestionRepository
            , ITimeCodeExcerciseRepository timeCodeExcerciseRepository
            , IVideoTimeCodeRepository videoTimeCodeRepository
            , IMapper mapper)
        {
            _videoRepository = videoRepository;
            _questionRepository = questionRepository;
            _excerciseRepository = excerciseRepository;
            _excerciseQuestionRepository = excerciseQuestionRepository;
            _timeCodeExcerciseRepository = timeCodeExcerciseRepository;
            _videoTimeCodeRepository = videoTimeCodeRepository;
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

            // Lưu dữ liệu Video

            Video video = _mapper.Map<Video>(request);

            request.VideoTimeCodes.ForEach(x =>
            {
                if (x == null)
                {
                    methodResult.StatusCode = StatusCodes.Status400BadRequest;
                    methodResult.AddErrorMessage(nameof(EnumVideoErrorCode.VD03V));
                }
                else
                {
                    VideoTimeCode videoTimeCode = video.VideoTimeCodes[request.VideoTimeCodes.IndexOf(x)];
                    x.Excercises.ForEach(n =>
                    {
                        if (n == null)
                        {
                            methodResult.StatusCode = StatusCodes.Status400BadRequest;
                            methodResult.AddErrorMessage(nameof(EnumVideoErrorCode.VD03V));
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
                                    methodResult.AddErrorMessage(nameof(EnumVideoErrorCode.VD03V));
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