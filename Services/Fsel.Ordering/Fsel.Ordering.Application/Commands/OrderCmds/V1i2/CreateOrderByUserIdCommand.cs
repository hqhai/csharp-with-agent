// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds.V1i2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Application.Commands.VoucherCmds;
    using Fsel.Ordering.Application.Queries.OrderQuery;
    using Fsel.Ordering.Application.Queues.Publishers;
    using Fsel.Ordering.Application.Services.UserService;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Enums;
    using Fsel.Ordering.Domain.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Orders.V1i2;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateOrderByUserIdCommand : CreateOrderByUserIdCommandModel, IRequest<MethodResult<OrderModel>>
    {
    }

    public class CreateOrderByUserIdCommandHandler : IRequestHandler<CreateOrderByUserIdCommand, MethodResult<OrderModel>>
    {
        private readonly IMapper _mapper;
        private readonly IOrderRepository _orderRepository;
        private readonly IMediator _mediator;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly IUserService _userService;
        private readonly IEventRepository _eventRepository;
        private readonly IVoucherRepository _voucherRepository;

        public CreateOrderByUserIdCommandHandler(IMapper mapper,
            IOrderRepository orderRepository,
            IMediator mediator,
            NotificationMessagePublisher notificationMessagePublisher,
            IUserService userService,
            IEventRepository eventRepository,
            IVoucherRepository voucherRepository)
        {
            _mapper = mapper;
            _orderRepository = orderRepository;
            _mediator = mediator;
            _notificationMessagePublisher = notificationMessagePublisher;
            _userService = userService;
            _eventRepository = eventRepository;
            _voucherRepository = voucherRepository;
        }

        public async Task<MethodResult<OrderModel>> Handle(CreateOrderByUserIdCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<OrderModel>();

            if (string.IsNullOrEmpty(request.FullName) || string.IsNullOrEmpty(request.Email))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            if (!string.IsNullOrEmpty(request.PhoneNumber) && !request.PhoneNumber.IsValidPhoneNumber() || !request.Email.IsValidEmail())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                return methodResult;
            }

            if (request.IsInvoice && (string.IsNullOrEmpty(request.CompanyName) || string.IsNullOrEmpty(request.CompanyAddress) || string.IsNullOrEmpty(request.CompanyTaxCode) || string.IsNullOrEmpty(request.CompanyEmail)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            if (request.IsInvoice && !request.CompanyEmail.IsValidEmail())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                return methodResult;
            }

            var @event = await _eventRepository.Queryable.Include(p => p.PackageEvents).FirstOrDefaultAsync(p => p.Id == request.EventId, cancellationToken);
            if (@event == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumEventErrorCode.EventNotExist), nameof(@event));
                return methodResult;
            }

            if (!@event.IsDefault)
            {
                var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);
                if (@event.StartDate > currentDate || @event.EndDate < currentDate)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumEventErrorCode.EventHasExpired), nameof(@event));
                    return methodResult;
                }
            }

            var packageEvent = @event.PackageEvents.FirstOrDefault(p => p.PackageId == request.PackageId);
            if (packageEvent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(@event));
                return methodResult;
            }

            var studentResult = await _userService.GetStudentByUserIdAsync(request.UserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return methodResult;
            }
            var student = studentResult.Content?.Result;

            var codeSend = await _mediator.Send(new GenerateRandomOrderQuery() { StudentCode = student?.Human?.Code }, cancellationToken).ConfigureAwait(false);
            string code = codeSend.Result ?? string.Empty;

            if (string.IsNullOrEmpty(code) || await _orderRepository.Queryable.AnyAsync(x => x.Code == code, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(code));
                return methodResult;
            }

            decimal discountPrice = 0;
            decimal totalPrice = packageEvent.Price;
            int discountPercent = 0;
            Guid? voucherId = null;

            if (!string.IsNullOrEmpty(request.VoucherCode))
            {
                var checkVoucher = await _mediator.Send(new CheckVoucherCommand()
                {
                    Code = request.VoucherCode,
                    PackageId = request.PackageId,
                    EventId = request.EventId,
                    UserId = request.UserId,
                }, cancellationToken);

                if (!checkVoucher.IsOK)
                {
                    methodResult.AddError(checkVoucher.ErrorMessages);
                    return methodResult;
                }

                var voucher = await _voucherRepository.GetByIdAsync(checkVoucher.Result?.VoucherId ?? default);
                if (voucher == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.VoucherNotExist));
                    return methodResult;
                }

                if (voucher.Category == EnumVoucherCategory.Percent)
                {
                    discountPrice = (decimal)NumberHelper.ConvertDoublePercent(Convert.ToDouble(packageEvent.Price * voucher.Value));
                    totalPrice = packageEvent.Price - discountPrice;
                    discountPercent = voucher.Value;
                }
                else if (voucher.Category == EnumVoucherCategory.Money)
                {
                    discountPrice = voucher.Value;
                    totalPrice = discountPrice < packageEvent.Price ? packageEvent.Price - discountPrice : 0;
                }
                else if (voucher.Category == EnumVoucherCategory.Month)
                {
                    discountPrice = 0;
                    totalPrice = packageEvent.Price;
                }

                voucherId = voucher.Id;
            }

            var order = await _orderRepository.Queryable.FirstOrDefaultAsync(p => p.UserId == request.UserId && p.Status == EnumOrderStatus.New && !p.IsTrial, cancellationToken);

            if (order != null)
            {
                var updateOrderResult = await _mediator.Send(new UpdateOrderCommand()
                {
                    Order = order,
                    Request = request,
                    DiscountPercent = discountPercent,
                    DiscountPrice = discountPrice,
                    TotalPrice = totalPrice,
                    Price = packageEvent.Price,
                    StudentCode = student?.Human?.Code,
                    VoucherId = voucherId,
                }, cancellationToken).ConfigureAwait(false);

                if (!updateOrderResult.IsOK)
                {
                    methodResult.AddError(updateOrderResult.ErrorMessages);
                    return methodResult;
                }

                methodResult.Result = updateOrderResult.Result;
                return methodResult;
            }

            var newOrder = _mapper.Map<Order>(request);

            newOrder.VoucherId = voucherId;
            newOrder.Price = packageEvent.Price;
            newOrder.Code = code;
            newOrder.DiscountPercent = discountPercent;
            newOrder.DiscountPrice = discountPrice;
            newOrder.TotalPrice = totalPrice;
            newOrder.UserId = request.UserId;

            if (!newOrder.IsValid())
            {
                methodResult.AddError(newOrder.ErrorMessages);
                return methodResult;
            }

            await _orderRepository.ExecuteTransactionAsync(async () =>
            {
                newOrder = _orderRepository.Add(newOrder);
                await _orderRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                SendNotify(newOrder.Id, newOrder.UserId, cancellationToken);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<OrderModel>(newOrder);
                return methodResult;
            });

            if (newOrder.TotalPrice == 0)
            {
                var changeStatusOrdersResult = await _mediator.Send(new ChangeStatusOrderCommand()
                {
                    OrderIds = new[] { newOrder.Id },
                    RevenueType = EnumPaymentRevenueType.Revenue,
                    Status = EnumOrderStatus.Payment
                }, cancellationToken);

                if (!changeStatusOrdersResult.IsOK)
                {
                    methodResult.AddError(changeStatusOrdersResult.ErrorMessages);
                    return methodResult;
                }
            }

            return methodResult;
        }

        private async void SendNotify(Guid orderId, Guid senderId, CancellationToken cancellationToken)
        {
            await _notificationMessagePublisher.Publish(new NotificationSendingQueueModel
            {
                Roles = new List<EnumRole> { EnumRole.Admin },
                ObjectId = orderId,
                Type = EnumNotificationType.Text,
                Content = EnumNotificationContent.OrderCreate,
                SenderId = senderId,
                PlatformCode = EnumPlatformCode.LMSAdmin
            }, cancellationToken);
        }
    }
}
