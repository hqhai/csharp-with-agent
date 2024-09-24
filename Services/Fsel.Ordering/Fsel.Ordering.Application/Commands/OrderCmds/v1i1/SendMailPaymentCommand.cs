// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds.v1i1
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Application.Services.SenderService;
    using Fsel.Ordering.Application.Services.UserService;
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

            var order = await _orderRepository.Queryable.Include(p => p.Package).FirstOrDefaultAsync(p => p.Id == request.OrderId, cancellationToken);
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

            var expiredDate = !student.ExpiredDate.HasValue ? string.Empty : student.ExpiredDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

            await _serverServices.SendEmailAsync(new SendEmailByTemplateCommandModel()
            {
                ToEmails = new List<string> { student.Human?.Email ?? string.Empty },
                Subject = "Chào mừng bạn đến với FSEL!",
                Params = new
                {
                    FullName = student.Human?.FullName,
                    OrderCode = order.Code,
                    ExpiredDate = expiredDate,
                    ContinueLearn = _appSetting.ResourceContent?.LmsWebsiteUrl
                },
                Template = EnumSenderTemplate.MailPaymentForStudent
            });

            if (!string.IsNullOrEmpty(order.Email) && !string.IsNullOrEmpty(student.Human?.Email) && order.Email.ToLower(CultureInfo.InvariantCulture) != student.Human?.Email.ToLower(CultureInfo.InvariantCulture))
            {
                var updatedDate = !order.UpdatedDate.HasValue ? order.CreatedDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : order.UpdatedDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

                var price = order.Price.ToString("C", new CultureInfo("vi-VN"));

                var totalPrice = order.TotalPrice.ToString("C", new CultureInfo("vi-VN"));

                var discount = order.DiscountPrice.ToString("C", new CultureInfo("vi-VN"));

                await _serverServices.SendEmailAsync(new SendEmailByTemplateCommandModel()
                {
                    ToEmails = new List<string> { order.Email ?? string.Empty },
                    Subject = "Biên nhận của bạn từ FSEL",
                    Params = new
                    {
                        FullName = order.FullName,
                        OrderCode = order.Code,
                        PaymentMethod = order.PaymentMethod.ToString(),
                        CreatedDate = updatedDate,
                        ExpiredDate = expiredDate,
                        Package = GetPackageName(order.Package),
                        Price = price.ToString(CultureInfo.InvariantCulture),
                        Discount = discount.ToString(CultureInfo.InvariantCulture),
                        TotalPrice = totalPrice.ToString(CultureInfo.InvariantCulture),
                        ContinueLearn = _appSetting.ResourceContent?.LmsWebsiteUrl
                    },
                    Template = EnumSenderTemplate.MailPaymentForCustomer
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
            if (package.MonthNumber == 1)
            {
                return "1 month";
            }
            else if (package.MonthNumber == 6)
            {
                return "6 months";
            }
            else
            {
                return "12 months";
            }
        }
    }
}
