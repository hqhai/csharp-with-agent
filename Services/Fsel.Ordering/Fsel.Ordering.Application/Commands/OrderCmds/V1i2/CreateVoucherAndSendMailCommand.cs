// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds.V1i2
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Ordering.Application.Commands.VoucherCmds;
    using Fsel.Ordering.Application.Services.UserService;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class CreateVoucherAndSendMailCommand : IRequest<MethodResult<bool>>
    {
        public Guid UserId { get; set; }
        public Guid PackageId { get; set; }
    }

    public class CreateVoucherAndSendMailCommandHandler : IRequestHandler<CreateVoucherAndSendMailCommand, MethodResult<bool>>
    {
        private readonly IUserService _userService;
        private readonly IMediator _mediator;
        private readonly IOrderRepository _orderRepository;

        public CreateVoucherAndSendMailCommandHandler(IUserService userService, IMediator mediator, IOrderRepository orderRepository)
        {
            _userService = userService;
            _mediator = mediator;
            _orderRepository = orderRepository;
        }

        public async Task<MethodResult<bool>> Handle(CreateVoucherAndSendMailCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var studentResult = await _userService.GetStudentByUserIdAsync(request.UserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return methodResult;
            }
            var student = studentResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            var voucherResult = await _mediator.Send(new CreateVoucherForRetailCommand()
            {
                UserId = request.UserId,
                PackageId = request.PackageId,
            }, cancellationToken).ConfigureAwait(false);
            if (!voucherResult.IsOK)
            {
                methodResult.AddError(voucherResult.ErrorMessages);
                return methodResult;
            }
            var voucherId = voucherResult.Result?.Id ?? default;

            var order = await _orderRepository.Queryable.Where(p => p.DiscountPrice == 0 && !p.VoucherId.HasValue && !p.IsTrial && p.Status == EnumOrderStatus.Payment && p.UserId == request.UserId).OrderByDescending(p => p.CreatedDate).FirstOrDefaultAsync(cancellationToken);

            if (order == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            await _mediator.Send(new SendMailPaymentWithVoucherCommand() { OrderId = order.Id, VoucherId = voucherId }, cancellationToken).ConfigureAwait(false);

            return methodResult;
        }
    }
}
