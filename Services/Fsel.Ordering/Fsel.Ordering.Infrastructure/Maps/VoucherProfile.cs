// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Common.Helpers;
    using Fsel.Core.Extensions;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Models.CommandModels.Vouchers;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Ordering.Domain.Models.EntityModels.IntegrationModel;

    public class VoucherProfile : Profile
    {
        public VoucherProfile()
        {
            CreateMap<Voucher, VoucherModel>().IgnoreAllNonExisting();
            CreateMap<CreateVoucherCommandModel, Voucher>().IgnoreAllNonExisting();
            CreateMap<UpdateVoucherCommandModel, Voucher>().IgnoreAllNonExisting();
            CreateMap<UserVoucherLock, UserVoucherLockModel>().IgnoreAllNonExisting();
            CreateMap<CreateVoucherTranslationModel, VoucherTranslation>().IgnoreAllNonExisting();
            CreateMap<HistoryVoucherModel, ExportHistoryVoucherAutoModel>().IgnoreAllNonExisting();
            CreateMap<VoucherModel, ExportVoucherModel>().IgnoreAllNonExisting();

            CreateMap<VoucherTranslation, Voucher>().IgnoreEntity()?.ReverseMap();
            CreateMap<Voucher, VoucherModel>().IgnoreAllNonExisting()?.MapTranslations<Voucher, VoucherModel, VoucherTranslation>();

            CreateMap<Voucher, VoucherIntegrationModel>().IgnoreAllNonExisting();
        }
    }
}
