// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.Videos;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Infrastructure.Common;
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
        private readonly IVideoSubFilePathRepository _videoSubFilePathRepository;

        public UpdateVideoCommandHandler(IVideoRepository videoRepository
            , IMapper mapper
            , VideoConverter videoConverter,
IVideoSubFilePathRepository videoSubFilePathRepository)
        {
            _videoRepository = videoRepository;
            _mapper = mapper;
            _videoConverter = videoConverter;
            _videoSubFilePathRepository = videoSubFilePathRepository;
        }

        public async Task<MethodResult<VideoModel>> Handle(UpdateVideoCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoModel> methodResult = new MethodResult<VideoModel>();

            #region Validation

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

            #endregion Validation

            await _videoRepository.ExecuteTransactionAsync(async () =>
            {
                var method = await _videoConverter.DeleteExerciseToVideo(video);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }

                var videoSubFilePaths = await _videoSubFilePathRepository.Queryable.Where(p => p.VideoId == video.Id).ToListAsync(cancellationToken);
                if (videoSubFilePaths.Any())
                {
                    await _videoSubFilePathRepository.DeleteListAsync(videoSubFilePaths);
                }

                _mapper.Map(request, video);
                method = await _videoConverter.UpdateTimeCodeToVideo(video, request);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }

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
