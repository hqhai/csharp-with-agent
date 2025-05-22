// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserReferrals
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetSenderByCodeQuery : IRequest<MethodResult<SenderModel>>
    {
        public string? ReferralCode { get; set; }
    }

    public class GetSenderByCodeQueryHandler : IRequestHandler<GetSenderByCodeQuery, MethodResult<SenderModel>>
    {
        private readonly UserManager<User> _userManager;

        public GetSenderByCodeQueryHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<MethodResult<SenderModel>> Handle(GetSenderByCodeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<SenderModel>();
            if (string.IsNullOrEmpty(request.ReferralCode))
            {
                methodResult.Result = null;
                return methodResult;
            }
            var sender = await _userManager.Users.FirstOrDefaultAsync(p => p.Code == request.ReferralCode.Trim(), cancellationToken);

            if (sender == null)
            {
                methodResult.Result = null;
                return methodResult;
            }

            methodResult.Result = new SenderModel()
            {
                SenderId = sender.Id,
                Code = sender.Code,
                FullName = sender.FullName,
            };
            return methodResult;
        }
    }
}
