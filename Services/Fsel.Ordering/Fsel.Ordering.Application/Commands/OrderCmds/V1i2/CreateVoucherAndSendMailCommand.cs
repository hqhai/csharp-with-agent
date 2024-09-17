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
    using Fsel.Ordering.Domain.Models.CommandModels.Orders.V1i2;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class CreateVoucherAndSendMailCommand : CreateVoucherAndSendMailCommandModels, IRequest<MethodResult<bool>>
    {
    }

    public class CreateVoucherAndSendMailCommandHandler : IRequestHandler<CreateVoucherAndSendMailCommand, MethodResult<bool>>
    {
        private readonly IUserService _userService;
        private readonly IMediator _mediator;
        private readonly IOrderRepository _orderRepository;
        private readonly IPackageRepository _packageRepository;

        public CreateVoucherAndSendMailCommandHandler(IUserService userService, IMediator mediator, IOrderRepository orderRepository, IPackageRepository packageRepository)
        {
            _userService = userService;
            _mediator = mediator;
            _orderRepository = orderRepository;
            _packageRepository = packageRepository;
        }

        public async Task<MethodResult<bool>> Handle(CreateVoucherAndSendMailCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (request.UserVoucher == null || request.UserVoucher.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            foreach (var item in request.UserVoucher)
            {
                if (string.IsNullOrEmpty(item.Email))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }
                var studentResult = await _userService.GetStudentByEmail(item.Email);
                if (!studentResult.IsSuccessStatusCode)
                {
                    methodResult.AddError(studentResult.Error);
                    return methodResult;
                }
                var student = studentResult.Content?.Result;
                if (student == null || student.Human == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }
                var package = await _packageRepository.Queryable.FirstOrDefaultAsync(p => p.MonthNumber == item.MonthNumber, cancellationToken);
                if (package == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }
                var voucherResult = await _mediator.Send(new CreateVoucherForRetailCommand()
                {
                    UserId = student.Human.UserId ?? default,
                    PackageId = package.Id,
                }, cancellationToken).ConfigureAwait(false);
                if (!voucherResult.IsOK)
                {
                    methodResult.AddError(voucherResult.ErrorMessages);
                    return methodResult;
                }
                var voucherId = voucherResult.Result?.Id ?? default;

                var order = await _orderRepository.Queryable.Where(p => p.DiscountPrice == 0 && !p.VoucherId.HasValue && !p.IsTrial && p.Status == EnumOrderStatus.Payment && p.UserId == student.Human.UserId).OrderByDescending(p => p.CreatedDate).FirstOrDefaultAsync(cancellationToken);

                if (order == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }

                await _mediator.Send(new SendMailPaymentWithVoucherCommand() { OrderId = order.Id, VoucherId = voucherId }, cancellationToken).ConfigureAwait(false);
            }

            return methodResult;
        }
    }
}
