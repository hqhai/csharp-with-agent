// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Common.Enums;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Infrastructure.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Commands.VideoCmd
{
    public class DeleteVideoCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteVideoCommandHandler : IRequestHandler<DeleteVideoCommand, MethodResult<bool>>
    {
        private readonly IVideoRepository _videoRepository;
        private readonly VideoConverter _videoConverter;

        public DeleteVideoCommandHandler(IVideoRepository videoRepository, VideoConverter videoConverter)
        {
            _videoRepository = videoRepository;
            _videoConverter = videoConverter;
        }

        public async Task<MethodResult<bool>> Handle(DeleteVideoCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

            var video = await _videoRepository.Queryable
                                           .Include(i => i.VideoTimeCodes.Where(x => !x.IsDeleted))
                                           .ThenInclude(x => x.TimeCodeExercises.Where(x => !x.IsDeleted && x.Exercise != null))
                                           .ThenInclude(x => x.Exercise)
                                           .ThenInclude(x => x!.ExerciseQuestions.Where(x => !x.IsDeleted))
                                           .ThenInclude(x => x.Question)
                                           .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (video == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(video));
                return methodResult;
            }
            if (video.VersionStatus == EnumVersionStatus.OldVersion)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.NotDelete), nameof(video.VersionStatus), video.VersionStatus);
                return methodResult;
            }
            var isVideoUsed = await _videoRepository.IsVideoUsed(request.Id);
            if (isVideoUsed)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.VideoUsed), nameof(request.Id), request.Id);
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
                var result = await _videoRepository.DeleteAsync(video);
                await _videoRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;
                return methodResult;
            });

            return methodResult;
        }
    }
}
