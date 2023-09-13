// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.UserVoucher
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListUserVoucherQuery : IRequest<MethodResult<IList<UserVoucherModel>>>
    {
    }

    public class GetListUserVoucherQueryHandler : IRequestHandler<GetListUserVoucherQuery, MethodResult<IList<UserVoucherModel>>>
    {
        private readonly IUserVoucherRepository _userVoucherRepository;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;

        public GetListUserVoucherQueryHandler(IUserVoucherRepository userVoucherRepository, AuthContext authContext, IMapper mapper)
        {
            _userVoucherRepository = userVoucherRepository;
            _authContext = authContext;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<UserVoucherModel>>> Handle(GetListUserVoucherQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<IList<UserVoucherModel>> methodResult = new MethodResult<IList<UserVoucherModel>>();

            var userVoucherModel = await _userVoucherRepository.Queryable
                            .Include(x => x.Voucher)
                            .Where(x => x.UserId == _authContext.CurrentUserId && x.Status == EnumUserVoucherStatus.NotUsed && (x.Voucher!.StartDate <= DateTime.Now && DateTime.Now <= x.Voucher.EndDate))
                            .Select(x => new UserVoucherModel
                            {
                                Id = x.Id,
                                UserId = x.UserId,
                                VoucherId = x.VoucherId,
                                Status = x.Status,
                                StartDate = x.Voucher!.StartDate,
                                EndDate = x.Voucher.EndDate,
                                VoucherName = x.Voucher.Name,
                                VoucherPackages = x.Voucher.VoucherPackages.Select(x => new VoucherPackageModel
                                {
                                    Id = x.Id,
                                    VoucherId = x.VoucherId,
                                    CreatedDate = x.CreatedDate,
                                    Percentage = x.Percentage,
                                    PackageId = x.PackageId,
                                    DiscountedPrice = (double)x.Package!.Price - (x.Percentage * NumberHelper.ConvertDoublePercent((double)x.Package!.Price)),
                                    Package = _mapper.Map<PackageModel>(x.Package)
                                }).ToList(),
                            }).ToListAsync(cancellationToken);

            methodResult.Result = userVoucherModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
