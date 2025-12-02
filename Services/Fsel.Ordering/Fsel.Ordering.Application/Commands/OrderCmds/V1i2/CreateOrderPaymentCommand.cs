// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds.V1i2
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Application.Commands.OrderCmds.v1i1;
    using Fsel.Ordering.Application.Commands.VoucherCmds;
    using Fsel.Ordering.Application.Queries.OrderQuery;
    using Fsel.Ordering.Application.Queues.Publishers;
    using Fsel.Ordering.Application.Services.CourseService;
    using Fsel.Ordering.Application.Services.UserService;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Enums;
    using Fsel.Ordering.Domain.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Orders.V1i2;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateOrderPaymentCommand : CreateOrderPaymentCommandModel, IRequest<MethodResult<OrderModel>>
    {
    }

    public class CreateOrderPaymentCommandHandler : IRequestHandler<CreateOrderPaymentCommand, MethodResult<OrderModel>>
    {
        private readonly IMapper _mapper;
        private readonly IOrderRepository _orderRepository;
        private readonly IMediator _mediator;
        private readonly IPackageRepository _packageRepository;
        private readonly IUserService _userService;
        private readonly IEventRepository _eventRepository;
        private readonly IVoucherRepository _voucherRepository;
        private readonly AddExpiredDateForStudentPublisher _addExpiredDateForStudentPublisher;
        private readonly AppSetting _appSetting;
        private readonly ILmsCourseService _lmsCourseService;

        public CreateOrderPaymentCommandHandler(IMapper mapper, IOrderRepository orderRepository, IMediator mediator, IPackageRepository packageRepository, IUserService userService, IEventRepository eventRepository, AddExpiredDateForStudentPublisher addExpiredDateForStudentPublisher, IVoucherRepository voucherRepository, AppSetting appSetting, ILmsCourseService lmsCourseService)
        {
            _mapper = mapper;
            _orderRepository = orderRepository;
            _mediator = mediator;
            _packageRepository = packageRepository;
            _userService = userService;
            _eventRepository = eventRepository;
            _addExpiredDateForStudentPublisher = addExpiredDateForStudentPublisher;
            _voucherRepository = voucherRepository;
            _appSetting = appSetting;
            _lmsCourseService = lmsCourseService;
        }

        public async Task<MethodResult<OrderModel>> Handle(CreateOrderPaymentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<OrderModel>();

            if (string.IsNullOrEmpty(request.FullName) || string.IsNullOrEmpty(request.Email))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            if (!request.Email.IsValidEmail())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                return methodResult;
            }

            Package? package = null;
            if (request.MonthNumber.HasValue)
            {
                package = await _packageRepository.Queryable.FirstOrDefaultAsync(p => p.MonthNumber == request.MonthNumber, cancellationToken);
            }
            else
            {
                package = await _packageRepository.Queryable.OrderBy(p => p.MonthNumber).FirstOrDefaultAsync(cancellationToken);
            }

            if (package == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(package));
                return methodResult;
            }

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            Event? @event = null;

            if (!string.IsNullOrEmpty(request.EventCode))
            {
                @event = await _eventRepository.Queryable.Include(p => p.PackageEvents).FirstOrDefaultAsync(p => p.Code.Trim().ToLower() == request.EventCode.Trim().ToLower(), cancellationToken);
            }
            else
            {
                @event = await _eventRepository.Queryable.Include(p => p.PackageEvents).FirstOrDefaultAsync(p => p.StartDate.HasValue && p.EndDate.HasValue && p.StartDate < currentDate && p.EndDate >= currentDate, cancellationToken);
            }

            if (@event == null)
            {
                @event = await _eventRepository.Queryable.Include(p => p.PackageEvents).FirstOrDefaultAsync(p => p.IsDefault, cancellationToken);
            }

            if (@event == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumEventErrorCode.EventNotExist), nameof(@event));
                return methodResult;
            }

            var packageEvent = @event.PackageEvents.FirstOrDefault(p => p.PackageId == package.Id);

            if (packageEvent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumEventErrorCode.MissingVersionOfPackage), nameof(packageEvent));
                return methodResult;
            }

            var discountPercent = 0;
            Guid? voucherId = null;

            if (!string.IsNullOrEmpty(request.VoucherCode))
            {
                var checkVoucher = await _mediator.Send(new CheckVoucherCommand()
                {
                    Code = request.VoucherCode,
                    PackageId = package.Id,
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
                discountPercent = voucher.Value;
                voucherId = voucher.Id;
            }

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

            var codeSend = await _mediator.Send(new GenerateRandomOrderQuery() { StudentCode = student.User?.Code }, cancellationToken).ConfigureAwait(false);
            string code = codeSend.Result ?? string.Empty;

            if (string.IsNullOrEmpty(code) || await _orderRepository.Queryable.AnyAsync(x => x.Code == code, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(code));
                return methodResult;
            }

            var newOrder = _mapper.Map<Order>(request);
            newOrder.PackageId = package.Id;
            newOrder.PaymentMethod = EnumPaymentMethodStatus.BankTransfer;
            newOrder.EventId = @event.Id;
            newOrder.Price = packageEvent.Price;
            newOrder.Code = code;
            newOrder.DiscountPercent = discountPercent;
            newOrder.DiscountPrice = (decimal)NumberHelper.ConvertDoublePercent(Convert.ToDouble(packageEvent.Price * discountPercent));
            newOrder.TotalPrice = packageEvent.Price - newOrder.DiscountPrice;
            newOrder.UserId = request.UserId;
            newOrder.Status = EnumOrderStatus.Payment;
            newOrder.ExpireDate = currentDate.AddMonths(package.MonthNumber);
            newOrder.RevenueType = request.IsRevenue ? EnumPaymentRevenueType.Revenue : EnumPaymentRevenueType.NotRevenue;
            newOrder.VoucherId = voucherId;
            newOrder.OrderTransactions.Add(new OrderTransaction()
            {
                Status = EnumOrderTransactionStatus.Success,
                RequestBody = request,
                Type = EnumOrderTransactionType.BankTransfer
            });
            if (!newOrder.IsValid())
            {
                methodResult.AddErrorBadRequest(newOrder.ErrorMessages);
                return methodResult;
            }

            await _orderRepository.ExecuteTransactionAsync(async () =>
            {
                newOrder = _orderRepository.Add(newOrder);
                await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                await _addExpiredDateForStudentPublisher.Publish(new AddExpiredDateForStudentQueueModel()
                {
                    StudentId = student.Id,
                    Month = request.MonthNumber.HasValue ? package.MonthNumber + packageEvent.MonthBonus : null,
                    Day = request.MonthNumber.HasValue ? packageEvent.DayBonus : null,
                    ExpiredDate = request.ExpireDate ?? null,
                }, cancellationToken);

                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });

            Thread.Sleep(3000);

            if (request.IsSendMail)
            {
                var createDate = newOrder.CreatedDate.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

                if (!newOrder.VoucherId.HasValue && _appSetting.VoucherConfigs?.VoucherForRetail?.StartDate <= createDate && _appSetting.VoucherConfigs.VoucherForRetail.EndDate >= createDate && currentDate < _appSetting.VoucherConfigs.VoucherForRetail.ExpiredDate)
                {
                    var voucher = await _mediator.Send(new CreateVoucherForRetailCommand()
                    {
                        UserId = newOrder.UserId,
                        PackageId = newOrder.PackageId ?? default,
                    }, cancellationToken).ConfigureAwait(false);
                    voucherId = voucher.Result?.Id;

                    await _mediator.Send(new SendMailPaymentWithVoucherCommand() { OrderId = newOrder.Id, VoucherId = voucherId ?? default }, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await _mediator.Send(new SendMailPaymentCommand() { OrderId = newOrder.Id }, cancellationToken);
                }
            }

            var updateNextUnitResult = await _lmsCourseService.UpdateNextUnit(newOrder.UserId);
            if (!updateNextUnitResult.IsSuccessStatusCode)
            {
                methodResult.AddError(updateNextUnitResult.Error);
                return methodResult;
            }

            return methodResult;
        }
    }
}
