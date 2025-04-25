// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ClassForumCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.FFmpegServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Refit;

    public class CheckFFmpegCommand : IRequest<MethodResult<bool>>
    {
        public IFormFile? FormFile { get; set; }

        public Guid ClassForumId { get; set; }
    }

    public class CheckFFmpegCommandHandler : IRequestHandler<CheckFFmpegCommand, MethodResult<bool>>
    {
        private readonly IFFmpegServices _fFmpegServices;
        private readonly IClassForumRepository _classForumRepository;
        private const double MinTimeValid = 0.35;
        private const int MinDBFS = -35;
        private const int MaxDBFS = -5;

        public CheckFFmpegCommandHandler(IFFmpegServices fFmpegServices,
                                         IClassForumRepository classForumRepository)
        {
            _fFmpegServices = fFmpegServices;
            _classForumRepository = classForumRepository;
        }

        public async Task<MethodResult<bool>> Handle(CheckFFmpegCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.FormFile);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            await using var stream = request.FormFile.OpenReadStream();
            var filePart = new StreamPart(stream, request.FormFile.FileName, request.FormFile.ContentType);

            var infoFileResult = await _fFmpegServices.GetInfoAudio(filePart);

            if (!infoFileResult.IsSuccessStatusCode)
            {
                methodResult.AddError(infoFileResult.Error);
                return methodResult;
            }

            var infoFile = infoFileResult.Content;
            if (infoFile == null || infoFile.AudioInfo == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(infoFile));
                return methodResult;
            }

            var classForum = await _classForumRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.ClassForumId, cancellationToken);
            if (classForum == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForum));
                return methodResult;
            }

            // check thời lượng file
            var timeTagert = Math.Ceiling(classForum.TaggetTimeLimit * MinTimeValid);
            if (infoFile.AudioInfo.AudioTime < timeTagert)
            {
                methodResult.AddErrorBadRequest(nameof(EnumFFmpegErrorCode.AudioTooShort), nameof(timeTagert));
                return methodResult;
            }

            // kiểm tra âm lượng
            if (infoFile.AudioInfo.MeanVolume < MinDBFS && infoFile.AudioInfo.MeanVolume > MaxDBFS)
            {
                methodResult.AddErrorBadRequest(nameof(EnumFFmpegErrorCode.AudioTooQuiet), nameof(infoFile.AudioInfo.MeanVolume));
                return methodResult;
            }

            return methodResult;
        }
    }
}
