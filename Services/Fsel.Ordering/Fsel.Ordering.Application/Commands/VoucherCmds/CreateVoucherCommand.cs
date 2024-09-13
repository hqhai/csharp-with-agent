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
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateVoucherCommand : CreateVoucherCommandModel, IRequest<MethodResult<VoucherModel>>
    {
    }

    public class CreateVoucherCommandHandler : IRequestHandler<CreateVoucherCommand, MethodResult<VoucherModel>>
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly IPackageRepository _packageRepository;
        private readonly IMapper _mapper;

        public CreateVoucherCommandHandler(IVoucherRepository voucherRepository, IPackageRepository packageRepository, IMapper mapper)
        {
            _voucherRepository = voucherRepository;
            _packageRepository = packageRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<VoucherModel>> Handle(CreateVoucherCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VoucherModel> methodResult = new MethodResult<VoucherModel>();

            if (request.EndDate.HasValue && request.StartDate.Date > request.EndDate.Value.Date)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.VoucherStartTimeMustSoonerThanEndTime), nameof(request.EndDate), request.EndDate);
                return methodResult;
            }
            if (request.PackageIds == null || request.PackageIds.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required), nameof(request.PackageIds));
                return methodResult;
            }
            if (_packageRepository.IsIdsInValid(request.PackageIds))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.PackageIds));
                return methodResult;
            }
            if (!string.IsNullOrEmpty(request.Code) && await _voucherRepository.Queryable.AnyAsync(p => p.Code.ToLower() == request.Code.ToLower(), cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Code));
                return methodResult;
            }

            Voucher voucher = _mapper.Map<Voucher>(request);
            voucher.Source = EnumVoucherSource.Admin;
            request.PackageIds.ForEach(p => voucher.VoucherPackages.Add(new VoucherPackage()
            {
                PackageId = p
            }));

            if (!voucher.IsValid())
            {
                methodResult.AddErrorBadRequest(voucher.ErrorMessages);
                return methodResult;
            }

            await _voucherRepository.ExecuteTransactionAsync(async () =>
            {
                voucher = _voucherRepository.Add(voucher);
                await _voucherRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<VoucherModel>(voucher);
                return methodResult;
            });

            return methodResult;
        }
    }
}
