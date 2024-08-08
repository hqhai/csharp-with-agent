// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserReferrals
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class CheckReferralCodeQuery : IRequest<MethodResult<bool>>
    {
        public string? ReferralCode { get; set; }
    }

    public class CheckReferralCodeQueryHandler : IRequestHandler<CheckReferralCodeQuery, MethodResult<bool>>
    {
        private readonly UserManager<User> _userManager;

        public CheckReferralCodeQueryHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<MethodResult<bool>> Handle(CheckReferralCodeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var sender = await _userManager.Users.Include(p => p.Human).FirstOrDefaultAsync(p => p.Human != null && !string.IsNullOrEmpty(p.Human.Code) && p.Human.Code.Trim().ToLower() == request.ReferralCode.Trim().ToLower(), cancellationToken);
            methodResult.Result = sender != null;
            return methodResult;
        }
    }
}
