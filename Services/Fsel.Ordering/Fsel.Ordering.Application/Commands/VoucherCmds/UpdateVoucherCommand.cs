// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.VoucherCmds
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Vouchers;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateVoucherCommand : UpdateVoucherCommandModel, IRequest<MethodResult<VoucherModel>>
    {
    }

    public class UpdateVoucherCommandHandler : IRequestHandler<UpdateVoucherCommand, MethodResult<VoucherModel>>
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly IPackageRepository _packageRepository;
        private readonly IMapper _mapper;

        public UpdateVoucherCommandHandler(IVoucherRepository voucherRepository, IPackageRepository packageRepository, IMapper mapper)
        {
            _voucherRepository = voucherRepository;
            _packageRepository = packageRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<VoucherModel>> Handle(UpdateVoucherCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VoucherModel> methodResult = new MethodResult<VoucherModel>();

            #region Validation

            if (request.StartDate > request.EndDate)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.VoucherStartTimeMustSoonerThanEndTime), nameof(request.EndDate), request.EndDate);
                return methodResult;
            }

            var voucher = await _voucherRepository.GetIncludeByIdAsync(request.Id);
            if (voucher == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id));
                return methodResult;
            }
            if (!voucher.IsValid())
            {
                methodResult.AddErrorBadRequest(voucher.ErrorMessages);
                return methodResult;
            }

            #endregion Validation

            voucher.VoucherPackages = request.VoucherPackages!.Select((x) => new VoucherPackage
            {
                Percentage = x.Percentage,
                PackageId = x.PackageId
            }).ToList();
            await _voucherRepository.ExecuteTransactionAsync(async () =>
            {
                voucher = _voucherRepository.Update(voucher);
                await _voucherRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<VoucherModel>(voucher);
                return methodResult;
            });

            return methodResult;
        }
    }
}
