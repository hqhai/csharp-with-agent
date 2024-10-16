// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.VoucherQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetUserVoucherLockByUserQuery : IRequest<MethodResult<UserVoucherLockModel>>
    {
    }

    public class GetUserVoucherLockByUserQueryHandler : IRequestHandler<GetUserVoucherLockByUserQuery, MethodResult<UserVoucherLockModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IUserVoucherLockRepository _userVoucherLockRepository;
        private readonly IMapper _mapper;

        public GetUserVoucherLockByUserQueryHandler(AuthContext authContext, IUserVoucherLockRepository userVoucherLockRepository, IMapper mapper)
        {
            _authContext = authContext;
            _userVoucherLockRepository = userVoucherLockRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<UserVoucherLockModel>> Handle(GetUserVoucherLockByUserQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<UserVoucherLockModel>();
            var userVoucherLock = await _userVoucherLockRepository.Queryable.FirstOrDefaultAsync(p => p.CreatedUserId == _authContext.CurrentUserId, cancellationToken);
            methodResult.Result = _mapper.Map<UserVoucherLockModel>(userVoucherLock);
            return methodResult;
        }
    }
}
