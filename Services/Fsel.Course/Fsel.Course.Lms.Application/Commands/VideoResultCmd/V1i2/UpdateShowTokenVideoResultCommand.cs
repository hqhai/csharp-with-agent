// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.VideoResultCmd.V1i2
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateShowTokenVideoResultCommand : IRequest<MethodResult<bool>>
    {
        public Guid VideoResultId { get; set; }
    }

    public class UpdateShowTokenVideoResultCommandHandler : IRequestHandler<UpdateShowTokenVideoResultCommand, MethodResult<bool>>
    {
        private readonly IVideoResultRepository _videoResultRepository;

        public UpdateShowTokenVideoResultCommandHandler(IVideoResultRepository videoResultRepository)
        {
            _videoResultRepository = videoResultRepository;
        }

        public async Task<MethodResult<bool>> Handle(UpdateShowTokenVideoResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var videoResult = await _videoResultRepository.GetByIdAsync(request.VideoResultId);
            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult));
                return methodResult;
            }
            if (videoResult.IsShowToken)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoResultErrorCode.CoinDisplayed), nameof(videoResult.IsShowToken), videoResult.IsShowToken);
                return methodResult;
            }
            videoResult.IsShowToken = true;
            await _videoResultRepository.ExecuteTransactionAsync(async () =>
            {
                await _videoResultRepository.BulkUpdateList(new List<VideoResult> { videoResult }, bulk =>
                {
                    bulk.ColumnInputExpression = c => new { c.IsShowToken };
                });

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
