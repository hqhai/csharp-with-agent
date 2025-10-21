// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds.v1i1
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Application.Services.SenderService;
    using Fsel.Ordering.Application.Services.UserService;
    using Fsel.Ordering.Application.Services.UserService.Models;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SendMailPaymentCommand : IRequest<MethodResult<VoidMethodResult>>
    {
        public Guid OrderId { get; set; }
    }

    public class SendMailPaymentCommandHandler : IRequestHandler<SendMailPaymentCommand, MethodResult<VoidMethodResult>>
    {
        private readonly ISenderServices _serverServices;
        private readonly IOrderRepository _orderRepository;
        private readonly IUserService _userService;
        private readonly AppSetting _appSetting;

        public SendMailPaymentCommandHandler(ISenderServices serverServices, IOrderRepository orderRepository, IUserService userService, AppSetting appSetting)
        {
            _serverServices = serverServices;
            _orderRepository = orderRepository;
            _userService = userService;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<VoidMethodResult>> Handle(SendMailPaymentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<VoidMethodResult>();

            var order = await _orderRepository.Queryable.Include(p => p.Package).FirstOrDefaultAsync(p => p.Id == request.OrderId && p.Status == EnumOrderStatus.Payment && !p.IsTrial, cancellationToken);
            if (order == null)
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

            var expiredDate = !student.ExpiredDate.HasValue ? (order.ExpireDate.HasValue ? order.ExpireDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : string.Empty) : student.ExpiredDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

            var updatedDate = !order.UpdatedDate.HasValue ? order.CreatedDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : order.UpdatedDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

            var numberFormat = (NumberFormatInfo)CultureInfo.GetCultureInfo("vi-VN").NumberFormat.Clone();
            numberFormat.CurrencySymbol = "";

            var price = order.Price.ToString("C", numberFormat).Trim();

            var discount = order.DiscountPrice.ToString("C", numberFormat).Trim();

            var totalPrice = order.TotalPrice.ToString("C", numberFormat).Trim();

            var token = await _userService.SenderSettingGenerateToken(new UpdateSenderSettingCommandModel
            {
                UserId = order.UserId,
                Template = EnumSenderTemplate.MailPaymentForStudent
            });

            await _serverServices.SendEmailAsync(new SendEmailByTemplateCommandModel()
            {
                ToEmails = new List<string> { order.Email ?? string.Empty },
                Subject = "Biên nhận của bạn từ FSEL",
                Params = new
                {
                    FullName = order.FullName,
                    OrderCode = order.Code,
                    PaymentMethod = order.PaymentMethod.ToString(),
                    Discount = discount.ToString(CultureInfo.InvariantCulture),
                    CreatedDate = updatedDate,
                    ExpiredDate = expiredDate,
                    Package = GetPackageName(order.Package),
                    Price = price.ToString(CultureInfo.InvariantCulture),
                    TotalPrice = totalPrice.ToString(CultureInfo.InvariantCulture),
                    ContinueLearn = _appSetting.ResourceContent?.LmsWebsiteUrl,
                    AccessLink = string.Format(CultureInfo.InvariantCulture, _appSetting.ConstantUrl?.UpdateSenderSettingUrl ?? string.Empty, token?.Content?.Result ?? string.Empty)
                },
                Template = EnumSenderTemplate.MailPaymentForCustomer
            });

            if (!string.IsNullOrEmpty(order.Email) && !string.IsNullOrEmpty(student.User?.Email) && order.Email.ToLower(CultureInfo.InvariantCulture) != student.User?.Email.ToLower(CultureInfo.InvariantCulture))
            {
                await _serverServices.SendEmailAsync(new SendEmailByTemplateCommandModel()
                {
                    ToEmails = new List<string> { student.User?.Email ?? string.Empty },
                    Subject = "Chào mừng bạn đến với FSEL!",
                    Params = new
                    {
                        FullName = student.User?.FullName,
                        OrderCode = order.Code,
                        ExpiredDate = expiredDate,
                        ContinueLearn = _appSetting.ResourceContent?.LmsWebsiteUrl,
                        AccessLink = string.Format(CultureInfo.InvariantCulture, _appSetting.ConstantUrl?.UpdateSenderSettingUrl ?? string.Empty, token?.Content?.Result ?? string.Empty)
                    },
                    Template = EnumSenderTemplate.MailPaymentForStudent
                });
            }

            return methodResult;
        }

        private static string GetPackageName(Package? package)
        {
            if (package == null)
            {
                return string.Empty;
            }
            return package.MonthNumber == 1 ? "1 month" : $"{package.MonthNumber} months";
        }
    }
}
