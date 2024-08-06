// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserReferrals
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Application.Services.OrderService.Model;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Infrastructure.Repositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateUserReferralCommand : CreateUserReferralCommandModel, IRequest<MethodResult<VoidMethodResult>>
    {
    }

    public class CreateUserReferralCommandHandler : IRequestHandler<CreateUserReferralCommand, MethodResult<VoidMethodResult>>
    {
        private readonly IUserReferralRepository _userReferralRepository;

        public CreateUserReferralCommandHandler(IUserReferralRepository userReferralRepository)
        {
            _userReferralRepository = userReferralRepository;
        }

        public async Task<MethodResult<VoidMethodResult>> Handle(CreateUserReferralCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<VoidMethodResult>();

            if (request.SenderId == request.ReceiverId)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                return methodResult;
            }

            if (await _userReferralRepository.Queryable.AnyAsync(p => p.SenderId == request.SenderId && p.ReceiverId == request.ReceiverId, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist));
                return methodResult;
            }

            await _userReferralRepository.ExecuteTransactionAsync(async () =>
            {
                _userReferralRepository.Add(new UserReferral()
                {
                    SenderId = request.SenderId,
                    ReceiverId = request.ReceiverId,
                });
                await _userReferralRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });

            return methodResult;
        }
    }
}
