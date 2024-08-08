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
    using Microsoft.EntityFrameworkCore;

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
            if (!string.IsNullOrEmpty(request.Code) && await _voucherRepository.Queryable.AnyAsync(p => p.Code.ToLower() == request.Code.ToLower() && p.Id != request.Id, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Code));
                return methodResult;
            }

            #endregion Validation

            var voucher = await _voucherRepository.Queryable.Include(p => p.VoucherPackages).FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
            if (voucher == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(voucher));
                return methodResult;
            }

            var currentPackageIds = voucher.VoucherPackages.Select(p => p.PackageId).ToList();
            var packageIds = request.PackageIds.Where(p => !currentPackageIds.Contains(p)).ToList();

            await _voucherRepository.ExecuteTransactionAsync(async () =>
            {
                _mapper.Map(request, voucher);

                foreach (var item in voucher.VoucherPackages.ToList())
                {
                    if (!request.PackageIds.Contains(item.PackageId))
                    {
                        voucher.VoucherPackages.Remove(item);
                    };
                }

                foreach (var item in packageIds)
                {
                    voucher.VoucherPackages.Add(new VoucherPackage()
                    {
                        PackageId = item
                    });
                }

                if (!voucher.IsValid())
                {
                    methodResult.AddError(voucher.ErrorMessages);
                    return methodResult;
                }

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
