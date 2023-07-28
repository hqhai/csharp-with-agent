// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.VoucherCmds
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Vouchers;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateVoucherStatusCommand : UpdateVoucherStatusCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class UpdateVoucherStatusCommandHandler : IRequestHandler<UpdateVoucherStatusCommand, MethodResult<bool>>
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly IMapper _mapper;

        public UpdateVoucherStatusCommandHandler(IVoucherRepository voucherRepository, IMapper mapper)
        {
            _voucherRepository = voucherRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<bool>> Handle(UpdateVoucherStatusCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

            var voucher = await _voucherRepository.GetByIdAsync(request.Id);
            if (voucher == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id));
                return methodResult;
            }

            #endregion Validation

            await _voucherRepository.ExecuteTransactionAsync(async () =>
            {
                voucher.IsActive = request.IsActive;
                voucher = _voucherRepository.Update(voucher);
                await _voucherRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
