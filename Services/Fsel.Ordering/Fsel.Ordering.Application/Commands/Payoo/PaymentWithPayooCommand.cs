// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.Payoo
{
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Application.Services.PayooService;
    using Fsel.Ordering.Application.Services.PayooService.Models;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Helpers;
    using MediatR;

    public class PaymentWithPayooCommand : IRequest<MethodResult<PayooModel>>
    {
    }
    public class PaymentWithPayooCommandHandler : IRequestHandler<PaymentWithPayooCommand, MethodResult<PayooModel>>
    {
        private readonly IPayooService _payooService;
        private readonly AppSetting _appSetting;
        public PaymentWithPayooCommandHandler(IPayooService payooService, AppSetting appSetting)
        {
            _payooService = payooService;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<PayooModel>> Handle(PaymentWithPayooCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PayooModel>();
            string originalString = "Thanh toan hoa đon mua khoa hoc cho cong ty FSEL, chi tiet lien he voi cong ty FSEL tai 35 Lac Trung, Hai Ba Trung, Ha Noi, hoac so dien thoai: 0972123654, Hay lien he ngay khi co van de de duoc giai quyet ngay lam tuc, chung toi luon ho tro 24/7 moi luc moi noi.";

            // Encode the string
            string encodedString = HttpUtility.UrlEncode(originalString);
            var param = new
            {
                UserName = _appSetting.PayooConfig?.Username,
                ShopId = _appSetting.PayooConfig?.ShopId,
                Session = "007121170526",
                ShopDomain = _appSetting.PayooConfig?.ShopDomain,
                ShopBackUrl = HttpUtility.UrlEncode(_appSetting.PayooConfig?.ShopBackUrl),
                OrderNo = Guid.NewGuid().ToString(),
                OrderCashAmount = 10000,
                OrderShipDays = 1,
                OrderShipDate = "23/01/2024",
                OrderDescription = encodedString,
                NotifyUrl = HttpUtility.UrlEncode(_appSetting.PayooConfig?.NotifyUrl),
                ValidityTime = "20240125010101",
                CustomerName = "Khoa",
                CustomerPhone = "0972439693",
                CustomerAddress = "HN",
                CustomerEmail = "huukhoa@atlantic.edu.vn"
            };

            var path = ResourceSettings.Payoo;
            using StreamReader streamReader = new StreamReader(path);
            var body = await streamReader.ReadToEndAsync(cancellationToken);
            body = RemoveWhitespace(body);
            var @params = ObjectHelper.GetDictionary(param);
            @params.ForEach(item =>
            {
                body = body.Replace($"[{item.Key}]", item.Value, StringComparison.CurrentCultureIgnoreCase);
            });
            var checkSum = EncodeHelper.HmacSHA512(_appSetting.PayooConfig?.Key ?? string.Empty, body);

            var payooResult = await _payooService.Create(new CreatePayooModel
            {
                Data = body,
                CheckSum = checkSum,
                Refer = _appSetting.PayooConfig?.ShopDomain,
                Method = "Bank-account",
                Bank = "ABB"
            });
            methodResult.Result = payooResult.Content;
            return methodResult;
        }
        public static string RemoveWhitespace(string input)
        {
            StringBuilder sb = new StringBuilder();
            using (StringReader sr = new StringReader(input))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    sb.Append(line.Trim());
                }
            }
            return sb.ToString();
        }
    }
}
