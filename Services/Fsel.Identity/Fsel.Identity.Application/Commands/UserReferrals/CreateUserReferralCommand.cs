// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserReferrals
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.OrderService.Model;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateUserReferralCommand : CreateUserReferralCommandModel, IRequest<MethodResult<VoidMethodResult>>
    {
    }

    public class CreateUserReferralCommandHandler : IRequestHandler<CreateUserReferralCommand, MethodResult<VoidMethodResult>>
    {
        private readonly IUserReferralRepository _userReferralRepository;
        private readonly UserManager<User> _userManager;
        private readonly AuthContext _authContext;

        public CreateUserReferralCommandHandler(IUserReferralRepository userReferralRepository, UserManager<User> userManager, AuthContext authContext)
        {
            _userReferralRepository = userReferralRepository;
            _userManager = userManager;
            _authContext = authContext;
        }

        public async Task<MethodResult<VoidMethodResult>> Handle(CreateUserReferralCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<VoidMethodResult>();

            var receiverId = request.ReceiverId ?? _authContext.CurrentUserId;

            request.ReferralCode = request.ReferralCode?.Trim() ?? string.Empty;
            var sender = await _userManager.Users.FirstOrDefaultAsync(p => p.Human != null && p.Human.Code != null && p.Human.Code == request.ReferralCode, cancellationToken);
            if (sender == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.ReferralCode));
                return methodResult;
            }

            if (sender.Id == request.ReceiverId)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.ReferralCode));
                return methodResult;
            }

            if (await _userReferralRepository.Queryable.AnyAsync(p => p.ReceiverId == request.ReceiverId, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(UserReferral));
                return methodResult;
            }

            await _userReferralRepository.ExecuteTransactionAsync(async () =>
            {
                _userReferralRepository.Add(new UserReferral()
                {
                    SenderId = sender.Id,
                    ReceiverId = receiverId,
                    Type = request.UserReferralType
                });
                await _userReferralRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });

            return methodResult;
        }
    }
}
