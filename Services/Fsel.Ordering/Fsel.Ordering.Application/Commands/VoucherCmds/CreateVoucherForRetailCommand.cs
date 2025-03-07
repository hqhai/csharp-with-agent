// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.VoucherCmds
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Vouchers;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateVoucherForRetailCommand : CreateVoucherRetailCommandModel, IRequest<MethodResult<VoucherModel>>
    {
    }

    public class CreateVoucherForRetailCommandHandler : IRequestHandler<CreateVoucherForRetailCommand, MethodResult<VoucherModel>>
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly IMapper _mapper;
        private const string Name = "Voucher launching for retail";
        private readonly AppSetting _appSetting;

        public CreateVoucherForRetailCommandHandler(IVoucherRepository voucherRepository, IMapper mapper, AppSetting appSetting)

        {
            _voucherRepository = voucherRepository;
            _mapper = mapper;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<VoucherModel>> Handle(CreateVoucherForRetailCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<VoucherModel>();

            var currentDate = Common.Helpers.DateTimeHelper.ConvertTimeFromUtc(DateTime.UtcNow, EnumCountryKey.Vietnam);

            string code;
            do
            {
                code = NumberHelper.GenerateCode(8);
            } while (await _voucherRepository.Queryable
                .AnyAsync(p => p.Code.ToLower() == code.ToLower(), cancellationToken));

            var voucher = new Voucher()
            {
                Name = Name,
                Code = code,
                Value = _appSetting.VoucherConfigs?.VoucherForRetail?.Percent ?? 0,
                Quantity = _appSetting.VoucherConfigs?.VoucherForRetail?.Quantity ?? 0,
                StartDate = currentDate,
                EndDate = _appSetting.VoucherConfigs?.VoucherForRetail?.ExpiredDate ?? currentDate.AddMonths(1),
                ApplicableSubjects = new List<EnumApplicableSubjectsVoucher>() { EnumApplicableSubjectsVoucher.NewSale },
                Source = EnumVoucherSource.Retail,
                SourceUserId = request.UserId,
                VoucherPackages = new List<VoucherPackage>()
                {
                    new VoucherPackage()
                    {
                        PackageId = request.PackageId,
                    }
                }
            };

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
