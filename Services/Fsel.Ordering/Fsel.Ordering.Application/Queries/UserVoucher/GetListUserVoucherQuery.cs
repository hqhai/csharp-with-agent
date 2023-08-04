// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.UserVoucher
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
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

        public GetListUserVoucherQueryHandler(IUserVoucherRepository userVoucherRepository, AuthContext authContext)
        {
            _userVoucherRepository = userVoucherRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<IList<UserVoucherModel>>> Handle(GetListUserVoucherQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<IList<UserVoucherModel>> methodResult = new MethodResult<IList<UserVoucherModel>>();

            var userVoucherModel = await _userVoucherRepository.Queryable
                            .Include(x => x.Voucher)
                            .Where(x => x.UserId == _authContext.CurrentUserId)
                            .Select(x => new UserVoucherModel
                            {
                                Id = x.Id,
                                UserId = x.UserId,
                                VoucherId = x.VoucherId,
                                Status = x.Status,
                                StartDate = x.Voucher!.StartDate,
                                EndDate = x.Voucher.EndDate,
                                VoucherName = x.Voucher.Name,
                            }).ToListAsync(cancellationToken);

            methodResult.Result = userVoucherModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
