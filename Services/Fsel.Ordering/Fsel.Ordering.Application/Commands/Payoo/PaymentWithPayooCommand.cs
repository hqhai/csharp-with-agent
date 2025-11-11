// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.Payoo
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Services.PayooService;
    using Fsel.Ordering.Application.Services.PayooService.Models;
    using Fsel.Ordering.Application.Services.UserService;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;

    public class PaymentWithPayooCommand : IRequest<MethodResult<PayooModel>>
    {
        public Guid OrderId { get; set; }
    }

    public class PaymentWithPayooCommandHandler : IRequestHandler<PaymentWithPayooCommand, MethodResult<PayooModel>>
    {
        private readonly IPayooService _payooService;
        private readonly AppSetting _appSetting;
        private readonly IOrderRepository _orderRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly IOrderTransactionRepository _orderTransactionRepository;

        public PaymentWithPayooCommandHandler(IPayooService payooService, AppSetting appSetting, IOrderRepository orderRepository, IUserService userService, AuthContext authContext, IOrderTransactionRepository orderTransactionRepository)
        {
            _payooService = payooService;
            _appSetting = appSetting;
            _orderRepository = orderRepository;
            _userService = userService;
            _authContext = authContext;
            _orderTransactionRepository = orderTransactionRepository;
        }

        public async Task<MethodResult<PayooModel>> Handle(PaymentWithPayooCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PayooModel>();

            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return methodResult;
            }
            var student = studentResult.Content?.Result;

            var validityTime = DateTime.UtcNow.AddMinutes(30).ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("yyyyMMddHHmmss", CultureInfo.CurrentCulture);

            await _orderTransactionRepository.ExecuteTransactionAsync(async () =>
            {
                var orderTransaction = new OrderTransaction() { Type = EnumOrderTransactionType.Payoo, Status = EnumOrderTransactionStatus.Fail, OrderId = order.Id };
                orderTransaction = _orderTransactionRepository.Add(orderTransaction);
                await _orderTransactionRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                var orderDescription = string.Format(CultureInfo.InvariantCulture, PaymentSetting.Payoo.OrderDescription, order.Code);

                var param = new
                {
                    UserName = _appSetting.PayooConfig?.Username,
                    ShopId = _appSetting.PayooConfig?.ShopId,
                    ShopTitle = _appSetting.PayooConfig?.ShopTitle,
                    ShopDomain = _appSetting.PayooConfig?.ShopDomain,
                    ShopBackUrl = _appSetting.PayooConfig?.ShopBackUrl,
                    OrderCashAmount = order.TotalPrice,
                    OrderDescription = orderDescription,
                    NotifyUrl = _appSetting.PayooConfig?.NotifyUrl,
                    ValidityTime = validityTime,
                    OrderCode = order.Code,
                    CustomerName = student?.Human?.FullName,
                    CustomerPhone = student?.Human?.PhoneNumber,
                    CustomerAddress = student?.Human?.Address,
                    CustomerEmail = student?.Human?.Email,
                    StudentCode = student?.Human?.Code,
                    Email = _appSetting.ResourceContent?.Email,
                    Hotline = _appSetting.ResourceContent?.HotLine,
                };

                using StreamReader streamReader = new StreamReader(ResourceSettings.Payoo);

                var body = await streamReader.ReadToEndAsync(cancellationToken);

                body = Shared.Helpers.StringHelper.RemoveWhitespace(body);

                var @params = ObjectHelper.GetDictionary(param);

                @params.ForEach(item =>
                {
                    body = body.Replace($"[{item.Key}]", item.Value, StringComparison.CurrentCultureIgnoreCase);
                });

                orderTransaction.RequestBody = body;
                orderTransaction = _orderTransactionRepository.Update(orderTransaction);
                await _orderTransactionRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                var checkSum = EncodeHelper.GenerateChecksum(_appSetting.PayooConfig?.Key ?? string.Empty, body);
                var payooResult = await _payooService.Create(new CreatePayooModel
                {
                    Data = body,
                    CheckSum = checkSum,
                    Refer = _appSetting.PayooConfig?.ShopDomain,
                });

                methodResult.Result = payooResult.Content;
                return methodResult;
            });
            return methodResult;
        }
    }
}
