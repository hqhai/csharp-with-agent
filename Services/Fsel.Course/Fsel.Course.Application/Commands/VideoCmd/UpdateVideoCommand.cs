// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Base.Interfaces;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.Videos;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Infrastructure.Common;
using Fsel.Course.Infrastructure.Common.VideoHelpers;
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
        private readonly IMapper _mapper;
        private readonly VideoConverter _videoConverter;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILevelRepository _levelRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IVideoSubFilePathRepository _videoSubFilePathRepository;
        private readonly IVersionEntityUpdater<Video> _versionEntityUpdater;
        private readonly QuestionConverter _questionConverter;

        public UpdateVideoCommandHandler(IVideoRepository videoRepository
            , IMapper mapper
            , VideoConverter videoConverter
            , ICategoryRepository categoryRepository
            , ILevelRepository levelRepository
            , IVideoResultRepository videoResultRepository
            , IVideoSubFilePathRepository videoSubFilePathRepository
            , IVersionEntityUpdater<Video> versionEntityUpdater
            , QuestionConverter questionConverter)
        {
            _videoRepository = videoRepository;
            _mapper = mapper;
            _videoConverter = videoConverter;
            _categoryRepository = categoryRepository;
            _levelRepository = levelRepository;
            _videoResultRepository = videoResultRepository;
            _videoSubFilePathRepository = videoSubFilePathRepository;
            _versionEntityUpdater = versionEntityUpdater;
            _questionConverter = questionConverter;
        }

        public async Task<MethodResult<VideoModel>> Handle(UpdateVideoCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoModel> methodResult = new MethodResult<VideoModel>();

            #region Validation

            var video = await _videoRepository.GetIncludeByIdAsync(request.Id);
            if (video == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(video));
                return methodResult;
            }
            if (video.VersionStatus == EnumVersionStatus.OldVersion)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.NotEdited), nameof(video.VersionStatus), video.VersionStatus);
                return methodResult;
            }
            var factory = VideoFactory.Create(request, _mapper, _questionConverter);
            var newVersionVideo = factory.Build(originalId: video.OriginalId);
            if (await newVersionVideo.ValidateDuplicateVideo(_videoRepository).ConfigureAwait(false))
            {
                methodResult.AddErrorBadRequest(newVersionVideo.ErrorMessages);
                return methodResult;
            }

            if (!await newVersionVideo.ValidateLevel(_levelRepository).ConfigureAwait(false))
            {
                methodResult.AddErrorBadRequest(newVersionVideo.ErrorMessages);
                return methodResult;
            }
            if (!await newVersionVideo.ValidateProgram(_categoryRepository).ConfigureAwait(false))
            {
                methodResult.AddErrorBadRequest(newVersionVideo.ErrorMessages);
                return methodResult;
            }
            if (!newVersionVideo.ValidateVideoPercentConfigs())
            {
                methodResult.AddErrorBadRequest(newVersionVideo.ErrorMessages);
                return methodResult;
            }
            if (!newVersionVideo.IsValid())
            {
                methodResult.AddErrorBadRequest(newVersionVideo.ErrorMessages);
                return methodResult;
            }
            var methodQuestion = factory.ValidateQuestions(request);
            if (!methodQuestion.IsOK)
            {
                methodResult.AddErrorBadRequest(methodQuestion.ErrorMessages);
                return methodResult;
            }

            #endregion Validation

            if (await _videoResultRepository.Queryable.AnyAsync(x => x.VideoId == request.Id, cancellationToken))
            {
                await _versionEntityUpdater.UpdateEntity(video, newVersionVideo,
                      async (_, entity) => true,
                      async (oldEntity, newEntity) =>
                      {
                          await Task.Yield();
                      }
                  );
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<VideoModel>(newVersionVideo);
            }
            else
            {
                _mapper.Map(request, video);
                if (!video.IsValid())
                {
                    methodResult.AddErrorBadRequest(video.ErrorMessages);
                    return methodResult;
                }

                var videoSubFilePaths = await _videoSubFilePathRepository.Queryable.Where(p => p.VideoId == video.Id).ToListAsync(cancellationToken);
                if (videoSubFilePaths.Any())
                {
                    await _videoSubFilePathRepository.DeleteListAsync(videoSubFilePaths);
                }

                var method = await _videoConverter.UpdateTimeCodeToVideo(video, request);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
                await _videoRepository.ExecuteTransactionAsync(async () =>
                {
                    video = _videoRepository.Update(video);
                    await _videoRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    methodResult.StatusCode = StatusCodes.Status200OK;
                    methodResult.Result = _mapper.Map<VideoModel>(video);
                    return methodResult;
                });
            }

            return methodResult;
        }
    }
}
