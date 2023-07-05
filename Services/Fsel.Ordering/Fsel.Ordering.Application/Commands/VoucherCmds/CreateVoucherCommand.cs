// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.VoucherCmds
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Vouchers;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

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

            Voucher voucher = _mapper.Map<Voucher>(request);
            if (request.StartDate < request.EndDate)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.VoucherStartTimeMustSoonerThanEndTime), nameof(request.EndDate), request.EndDate);
                return methodResult;
            }
            if (!voucher.IsValid())
            {
                methodResult.AddErrorBadRequest(voucher.ErrorMessages);
                return methodResult;
            }
            if (request.VoucherPackages != null && request.VoucherPackages.Count > 0)
            {
                if (_packageRepository.IsIdsInValid(request.VoucherPackages.Select(x => x.PackageId).ToList()))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumPackageErrorCode.PackageNotExist));
                    return methodResult;
                }
                voucher.VoucherPackages = request.VoucherPackages!.Select((x) => new VoucherPackage
                {
                    Percentage = x.Percentage,
                    PackageId = x.PackageId
                }).ToList();
            }
            if (request.StartDate >= DateTime.Now && DateTime.Now <= request.EndDate)
            {
                voucher.IsActive = true;
            }
            else
            {
                voucher.IsActive = false;
            }

            await _voucherRepository.ExecuteTransactionAsync(async () =>
            {
                voucher.IsGlobal = true;
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
