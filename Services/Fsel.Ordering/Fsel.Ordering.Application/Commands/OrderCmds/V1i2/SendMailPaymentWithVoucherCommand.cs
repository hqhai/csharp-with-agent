// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds.V1i2
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Application.Services.SenderService;
    using Fsel.Ordering.Application.Services.UserService;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SendMailPaymentWithVoucherCommand : IRequest<MethodResult<VoidMethodResult>>
    {
        public Guid OrderId { get; set; }
        public Guid VoucherId { get; set; }
    }

    public class SendMailPaymentWithVoucherCommandHandler : IRequestHandler<SendMailPaymentWithVoucherCommand, MethodResult<VoidMethodResult>>
    {
        private readonly ISenderServices _serverServices;
        private readonly IOrderRepository _orderRepository;
        private readonly IUserService _userService;
        private readonly AppSetting _appSetting;
        private readonly IVoucherRepository _voucherRepository;

        public SendMailPaymentWithVoucherCommandHandler(ISenderServices serverServices, IOrderRepository orderRepository, IUserService userService, AppSetting appSetting, IVoucherRepository voucherRepository)
        {
            _serverServices = serverServices;
            _orderRepository = orderRepository;
            _userService = userService;
            _appSetting = appSetting;
            _voucherRepository = voucherRepository;
        }

        public async Task<MethodResult<VoidMethodResult>> Handle(SendMailPaymentWithVoucherCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<VoidMethodResult>();

            var order = await _orderRepository.Queryable.Include(p => p.Package).FirstOrDefaultAsync(p => p.Id == request.OrderId, cancellationToken);
            if (order == null)
            {
                return methodResult;
            }
            var voucher = await _voucherRepository.Queryable.FirstOrDefaultAsync(p => p.Id == request.VoucherId, cancellationToken);
            if (voucher == null)
            {
                return methodResult;
            }
            var studentResult = await _userService.GetStudentByUserIdAsync(order.UserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                return methodResult;
            }
            var student = studentResult.Content?.Result;
            if (student == null)
            {
                return methodResult;
            }

            var expiredDate = !student.ExpiredDate.HasValue ? string.Empty : student.ExpiredDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

            var updatedDate = !order.UpdatedDate.HasValue ? order.CreatedDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : order.UpdatedDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

            var price = order.Price.ToString("C", new CultureInfo("vi-VN"));

            var totalPrice = order.TotalPrice.ToString("C", new CultureInfo("vi-VN"));

            await _serverServices.SendEmailAsync(new SendEmailByTemplateCommandModel()
            {
                ToEmails = new List<string> { student.User?.Email ?? string.Empty },
                Subject = "Biên nhận của bạn từ FSEL",
                Params = new
                {
                    FullName = order.FullName,
                    OrderCode = order.Code,
                    PaymentMethod = order.PaymentMethod.ToString(),
                    Discount = order.DiscountPrice,
                    CreatedDate = updatedDate,
                    ExpiredDate = expiredDate,
                    Package = order.Package?.MonthNumber,
                    Price = price.ToString(CultureInfo.InvariantCulture),
                    TotalPrice = totalPrice.ToString(CultureInfo.InvariantCulture),
                    ContinueLearn = _appSetting.ResourceContent?.LmsWebsiteUrl,
                    Voucher = voucher.Code,
                    Date = _appSetting.VoucherConfigs?.VoucherForRetail?.StartDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                },
                Template = EnumSenderTemplate.MailPaymentWithVoucher
            });
            return methodResult;
        }
    }
}
