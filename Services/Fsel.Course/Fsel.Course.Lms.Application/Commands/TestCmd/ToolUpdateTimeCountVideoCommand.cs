// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.TestCmd
{
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ToolUpdateTimeCountVideoCommand : IRequest<MethodResult<bool>>
    {
    }

    public class ToolUpdateTimeCountVideoCommandHandler : IRequestHandler<ToolUpdateTimeCountVideoCommand, MethodResult<bool>>
    {
        private readonly IVideoRepository _videoRepository;

        public ToolUpdateTimeCountVideoCommandHandler(IVideoRepository videoRepository)
        {
            _videoRepository = videoRepository;
        }

        public async Task<MethodResult<bool>> Handle(ToolUpdateTimeCountVideoCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            var videos = await _videoRepository.Queryable.ToListAsync(cancellationToken);
            foreach (var item in videos)
            {
                if (item.TimeCount.HasValue)
                {
                    continue;
                }
                item.TimeCount = MediaHelper.GetMediaDurationAsync(item.VideoFilePath);
            }
            await _videoRepository.BulkUpdateList(videos, bulk =>
            {
                bulk.ColumnInputExpression = x => new { x.TimeCount };
            });
            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
