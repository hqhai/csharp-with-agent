// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.Videos;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Infrastructure.Common;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

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
        private readonly ILevelRepository _levelRepository;
        private readonly ICategoryRepository _categoryRepository;

        public CreateVideoCommandHandler(IVideoRepository videoRepository
            , VideoConverter videoConverter
            , IMapper mapper
            , ILevelRepository levelRepository
            , ICategoryRepository categoryRepository
            )
        {
            _videoRepository = videoRepository;
            _videoConverter = videoConverter;
            _mapper = mapper;
            _levelRepository = levelRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<MethodResult<VideoModel>> Handle(CreateVideoCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoModel> methodResult = new MethodResult<VideoModel>();

            #region Validate New

            if (request.OriginalId.HasValue)
            {
                var videoOriginal = await _videoRepository.GetByIdAsync(request.OriginalId.Value);
                if (videoOriginal == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.OriginalId), request.OriginalId);
                    return methodResult;
                }
            }
            request.OriginalId = request.OriginalId ?? Guid.NewGuid();

            Category? program = null;
            if (request.ProgramId.HasValue)
            {
                program = await _categoryRepository.Queryable.Where(x => x.Id == request.ProgramId && x.Type == EnumTypeCategory.Program).FirstOrDefaultAsync(cancellationToken);
                if (program == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.ProgramId), request.ProgramId);
                    return methodResult;
                }
            }
            Level? level = null;
            if (request.LevelId.HasValue)
            {
                level = await _levelRepository.GetByIdAsync(request.LevelId.Value);
                if (level == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.LevelId), request.LevelId);
                    return methodResult;
                }
            }
            if (level != null && program != null && level.ProgramId != request.ProgramId)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.ProgramId), request.ProgramId);
                return methodResult;
            }

            #endregion Validate New

            #region Validation

            Video video = _mapper.Map<Video>(request);
            if (!video.IsValid())
            {
                methodResult.AddErrorBadRequest(video.ErrorMessages);
                return methodResult;
            }

            if (request.OriginalId.HasValue)
            {
                var countVersion = await _videoRepository.Queryable.Where(x => x.OriginalId == request.OriginalId.Value).CountAsync(cancellationToken);
                video.Version = countVersion + 1;
            }
            video.VersionStatus = EnumVersionStatus.LastVersion;
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
