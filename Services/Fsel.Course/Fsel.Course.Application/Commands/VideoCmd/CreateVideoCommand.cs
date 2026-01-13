// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.Videos;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Infrastructure.Common;
using Fsel.Course.Infrastructure.Common.VideoHelpers;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Course.Application.Commands.VideoCmd
{
    public class CreateVideoCommand : UpdateVideoCommandModel, IRequest<MethodResult<VideoModel>>
    {
    }

    public class CreateVideoCommandHandler : IRequestHandler<CreateVideoCommand, MethodResult<VideoModel>>
    {
        private readonly IVideoRepository _videoRepository;
        private readonly IMapper _mapper;
        private readonly ILevelRepository _levelRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly QuestionConverter _questionConverter;

        public CreateVideoCommandHandler(IVideoRepository videoRepository
            , IMapper mapper
            , ILevelRepository levelRepository
            , ICategoryRepository categoryRepository
            , QuestionConverter questionConverter)
        {
            _videoRepository = videoRepository;
            _mapper = mapper;
            _levelRepository = levelRepository;
            _categoryRepository = categoryRepository;
            _questionConverter = questionConverter;
        }

        public async Task<MethodResult<VideoModel>> Handle(CreateVideoCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoModel> methodResult = new MethodResult<VideoModel>();

            #region Validate

            var factory = VideoFactory.Create(request, _mapper, _questionConverter);
            var video = factory.Build();
            if (await video.ValidateDuplicateVideo(_videoRepository).ConfigureAwait(false))
            {
                methodResult.AddErrorBadRequest(video.ErrorMessages);
                return methodResult;
            }
            if (!await video.ValidateLevel(_levelRepository).ConfigureAwait(false))
            {
                methodResult.AddErrorBadRequest(video.ErrorMessages);
                return methodResult;
            }
            if (!await video.ValidateProgram(_categoryRepository).ConfigureAwait(false))
            {
                methodResult.AddErrorBadRequest(video.ErrorMessages);
                return methodResult;
            }
            if (!video.ValidateVideoPercentConfigs())
            {
                methodResult.AddErrorBadRequest(video.ErrorMessages);
                return methodResult;
            }
            if (!video.IsValid())
            {
                methodResult.AddErrorBadRequest(video.ErrorMessages);
                return methodResult;
            }

            #endregion Validate

            var method = factory.ValidateQuestions(request);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }

            await _videoRepository.ExecuteTransactionAsync(async () =>
            {
                video = _videoRepository.Add(video);
                await _videoRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<VideoModel>(video);
                return methodResult;
            });

            return methodResult;
        }
    }
}
