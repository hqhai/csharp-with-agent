// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.BannerCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class DeleteBannerCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteBannerCommandHandler : IRequestHandler<DeleteBannerCommand, MethodResult<bool>>
    {
        private readonly IBannerRepository _bannerRepository;

        public DeleteBannerCommandHandler(IBannerRepository bannerRepository)
        {
            _bannerRepository = bannerRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteBannerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var banner = await _bannerRepository.GetByIdAsync(request.Id);
            if (banner == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(banner));
                return methodResult;
            }

            await _bannerRepository.ExecuteTransactionAsync(async () =>
            {
                await _bannerRepository.DeleteAsync(banner);
                await _bannerRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
