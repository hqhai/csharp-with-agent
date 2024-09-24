// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.StudentCmd
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteListDataUserCommand : IRequest<MethodResult<bool>>
    {
        public Guid UserId { get; set; }
    }
    public class DeleteListDataUserCommandHandler : IRequestHandler<DeleteListDataUserCommand, MethodResult<bool>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserVoucherRepository _userVoucherRepository;
        private readonly IUserReferralRepository _userReferralRepository;

        public DeleteListDataUserCommandHandler(IOrderRepository orderRepository, IUserVoucherRepository userVoucherRepository, IUserReferralRepository userReferralRepository)
        {
            _orderRepository = orderRepository;
            _userVoucherRepository = userVoucherRepository;
            _userReferralRepository = userReferralRepository;
        }
        public async Task<MethodResult<bool>> Handle(DeleteListDataUserCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var order = await _orderRepository.Queryable
                                              .Where(x => x.UserId == request.UserId)
                                              .ToListAsync(cancellationToken);
            if (order.Count != 0)
            {
                await _orderRepository.DeleteListAsync(order);
                await _orderRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken).ConfigureAwait(false);
            }

            var userVoucher = await _userVoucherRepository.Queryable
                                                          .Where(x => x.UserId == request.UserId)
                                                          .ToListAsync(cancellationToken);
            if (userVoucher.Count != 0)
            {
                await _userVoucherRepository.DeleteListAsync(userVoucher);
                await _userVoucherRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken).ConfigureAwait(false);
            }

            var userReferral = await _userReferralRepository.Queryable
                                                            .Where(x => x.ReceiverId == request.UserId)
                                                            .ToListAsync(cancellationToken);
            if (userReferral.Count != 0)
            {
                await _userReferralRepository.DeleteListAsync(userReferral);
                await _userReferralRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken).ConfigureAwait(false);
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }
    }
}
