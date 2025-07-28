// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Common.Helpers;
using Fsel.Course.Domain.Entities;
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
        private readonly IMediator _mediator;

        public UpdateVideoCommandHandler(IVideoRepository videoRepository
            , IMapper mapper
            , VideoConverter videoConverter
            , ICategoryRepository categoryRepository
            , ILevelRepository levelRepository
            , IVideoResultRepository videoResultRepository
            , IMediator mediator)
        {
            _videoRepository = videoRepository;
            _mapper = mapper;
            _videoConverter = videoConverter;
            _categoryRepository = categoryRepository;
            _levelRepository = levelRepository;
            _videoResultRepository = videoResultRepository;
            _mediator = mediator;
        }

        public async Task<MethodResult<VideoModel>> Handle(UpdateVideoCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoModel> methodResult = new MethodResult<VideoModel>();

            #region Validation

            #region Validate New

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

            #region Tạm thời không validate isTeacher

            //var isTeacher = await _userService.GetTeacherByIdAsync(request.TeacherId);
            //var isCheck = isTeacher?.Content?.Result;
            //if (isCheck == null)
            //{
            //    methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.TeacherIdDoesNotExitst), nameof(request.TeacherId));
            //    return methodResult;
            //}

            #endregion Tạm thời không validate isTeacher

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
            if (await _videoResultRepository.Queryable.AnyAsync(x => x.VideoId == request.Id, cancellationToken))
            {
                video.VersionStatus = EnumVersionStatus.OldVersion;
                var model = _mapper.Map<CreateVideoCommandModel>((UpdateVideoCommandModel)request);
                model.OriginalId = video.OriginalId ?? video.Id;
                await _mediator.Send(model.Serialize().Deserialize<CreateVideoCommand>(), cancellationToken).ConfigureAwait(false);
            }
            else
            {
                _mapper.Map(request, video);
                if (!video.IsValid())
                {
                    methodResult.AddErrorBadRequest(video.ErrorMessages);
                    return methodResult;
                }
                var method = await _videoConverter.UpdateTimeCodeToVideo(video, request);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
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
