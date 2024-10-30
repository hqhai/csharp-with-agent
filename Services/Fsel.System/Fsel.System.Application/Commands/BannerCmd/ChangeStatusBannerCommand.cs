// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.BannerCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ChangeStatusBannerCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class ChangeStatusBannerCommandHandler : IRequestHandler<ChangeStatusBannerCommand, MethodResult<bool>>
    {
        private readonly IBannerRepository _bannerRepository;

        public ChangeStatusBannerCommandHandler(IBannerRepository bannerRepository)
        {
            _bannerRepository = bannerRepository;
        }
        public async Task<MethodResult<bool>> Handle(ChangeStatusBannerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var banner = await _bannerRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (banner == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(banner), request.Id);
                return methodResult;
            }

            await _bannerRepository.ExecuteTransactionAsync(async () =>
            {
                if (banner.Status)
                {
                    banner.Status = false;
                }
                else
                {
                    banner.Status = true;
                }

                _bannerRepository.Update(banner);
                await _bannerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
