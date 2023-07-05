// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.VoucherCmds
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Domain.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Vouchers;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateVoucherStatusCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
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

            var voucher = await _voucherRepository.GetIncludeByIdAsync(request.Id);
            if (voucher == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.VoucherNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }
            if (!voucher.IsValid())
            {
                methodResult.AddErrorBadRequest(voucher.ErrorMessages);
                return methodResult;
            }

            #endregion Validation

            await _voucherRepository.ExecuteTransactionAsync(async () =>
            {
                voucher.IsActive = true;
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
