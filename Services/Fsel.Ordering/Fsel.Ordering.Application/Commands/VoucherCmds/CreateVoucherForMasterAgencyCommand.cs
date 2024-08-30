// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.VoucherCmds
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Application.Services.SystemService;
    using Fsel.Ordering.Application.Services.SystemService.Models;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Vouchers;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateVoucherForMasterAgencyCommand : CreateVoucherForMasterAgencyCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateVoucherForMasterAgencyCommandHandler : IRequestHandler<CreateVoucherForMasterAgencyCommand, MethodResult<bool>>
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly IPackageRepository _packageRepository;
        private readonly ISystemService _systemService;
        private const string Name = "Voucher launching for MA";

        public CreateVoucherForMasterAgencyCommandHandler(IVoucherRepository voucherRepository, IPackageRepository packageRepository, ISystemService systemService)
        {
            _voucherRepository = voucherRepository;
            _packageRepository = packageRepository;
            _systemService = systemService;
        }

        public async Task<MethodResult<bool>> Handle(CreateVoucherForMasterAgencyCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var currentDate = Common.Helpers.DateTimeHelper.ConvertTimeFromUtc(DateTime.UtcNow, EnumCountryKey.Vietnam);

            var package = await _packageRepository.Queryable.FirstOrDefaultAsync(p => p.MonthNumber == request.Package, cancellationToken);
            if (package == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            };

            var codes = new List<string>();

            while (true)
            {
                string code;
                do
                {
                    code = NumberHelper.GenerateCode(8);
                } while (await _voucherRepository.Queryable
                    .AnyAsync(p => p.Code.ToLower() == code.ToLower(), cancellationToken));

                if (!codes.Contains(code))
                {
                    codes.Add(code);
                }

                if (codes.Count == request.Quantity)
                {
                    break;
                }
            }

            var vouchers = codes.Select(p => new Voucher()
            {
                Name = Name,
                Code = p,
                Percent = request.Percent,
                Quantity = request.Quantity,
                StartDate = currentDate,
                EndDate = request.ExpiredDate,
                VoucherType = EnumVoucherType.NewSale,
                Source = EnumVoucherSource.MasterAgency,
                SourceName = request.MasterAgency,
                VoucherPackages = new List<VoucherPackage>()
                {
                    new VoucherPackage()
                    {
                        PackageId = package.Id,
                    }
                }
            });

            var voucher = vouchers.First();

            if (!voucher.IsValid())
            {
                methodResult.AddErrorBadRequest(voucher.ErrorMessages);
                return methodResult;
            }

            await _voucherRepository.ExecuteTransactionAsync(async () =>
            {
                await _voucherRepository.AddList(vouchers);
                await _voucherRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                await _systemService.AddVouchersForMAIntoGGSheet(new AddVouchersForMAIntoGoogleSheetCommandModel()
                {
                    MACode = request.MasterAgency,
                    Package = $"{request.Package} months",
                    ExpiredDate = request.ExpiredDate.ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                    Vouchers = codes,
                }).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
