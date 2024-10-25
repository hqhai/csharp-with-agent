// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.Videos;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Infrastructure.Common;
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
        private readonly VideoConverter _videoConverter;
        private readonly IMapper _mapper;

        public CreateVideoCommandHandler(IVideoRepository videoRepository
            , VideoConverter videoConverter
            , IMapper mapper)
        {
            _videoRepository = videoRepository;
            _videoConverter = videoConverter;
            _mapper = mapper;
        }

        public async Task<MethodResult<VideoModel>> Handle(CreateVideoCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoModel> methodResult = new MethodResult<VideoModel>();

            #region Validation

            Video video = _mapper.Map<Video>(request);
            var method = await _videoConverter.CreateTimeCodeToVideo(video, request);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
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
