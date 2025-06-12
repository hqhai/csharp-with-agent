using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Sender.Application.Services.ZaloServices;
using Fsel.Sender.Application.Services.ZaloServices.Models;
using Fsel.Sender.Domain.Entities;
using Fsel.Sender.Domain.IRepositories;
using Fsel.Sender.Domain.ValueSettings;
using Fsel.Shared.Enums;
using Fsel.Shared.Models.ShareModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Fsel.Sender.Application.Commands.SendZaloCmd
{
    public class SendSMSByZaloCommand : SendSMSByZaloCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class SendSMSByZaloCommandHandler : IRequestHandler<SendSMSByZaloCommand, MethodResult<bool>>
    {
        private readonly IZaloService _zaloService;
        private readonly AppSetting _appSetting;
        private readonly HttpClient _httpClient;
        private readonly IMessageHistoryRepository _messageHistoryRepository;
        private const string SuccessCode = "000";
        private readonly ILogger<SendSMSByZaloCommand> _logger;

        public SendSMSByZaloCommandHandler(IZaloService zaloService, AppSetting appSetting, HttpClient httpClient, IMessageHistoryRepository messageHistoryRepository, ILogger<SendSMSByZaloCommand> logger)
        {
            _zaloService = zaloService;
            _appSetting = appSetting;
            _httpClient = httpClient;
            _messageHistoryRepository = messageHistoryRepository;
            _logger = logger;
        }

        public async Task<MethodResult<bool>> Handle(SendSMSByZaloCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var brandName = _appSetting.ZaloConfig?.BrandName;

            var token = _appSetting.ZaloConfig?.Token;

            var templateId = GetTemplateId(request.Template, _appSetting);

            if (string.IsNullOrEmpty(brandName) || string.IsNullOrEmpty(templateId) || string.IsNullOrEmpty(token) || request.PhoneNumbers == null || !request.PhoneNumbers.Any())
            {
                var phonenumber = request.PhoneNumbers?.FirstOrDefault();
                _logger.LogError($"SendSMSByZaloCommand: config null(brandName: {brandName}, token: {token})or request PhoneNumbers null(phoneNumnber: {phonenumber})");
                return methodResult;
            }

            var requests = new List<SendSMSByZaloRequestMessageModel>();
            var messageHistories = new List<MessageHistory>();

            request.PhoneNumbers.ForEach(x =>
            {
                var smsId = Guid.NewGuid().ToString();
                var smsRequest = new SendSMSByZaloRequestMessageModel()
                {
                    To = x,
                    RequestID = smsId,
                    Scheduled = string.Empty,
                    TemplateId = templateId,
                    TemplateData = request.Params,
                    UseUnicode = request.UseUnicode
                };
                requests.Add(smsRequest);
                messageHistories.Add(new MessageHistory()
                {
                    To = x,
                    Type = EnumMessageHistoryType.Zalo,
                    SMSId = smsId,
                    Status = EnumMessageHistoryStatus.False,
                    RequestBody = smsRequest.Serialize()
                });
            });

            var requestModel = new SendSMSByZaloRequestModel()
            {
                From = brandName,
                Type = request.Type,
                ServiceType = 1,
                Messages = requests
            };

            var result = await _zaloService.SendSMSAsync(requestModel, token);

            messageHistories.ForEach(x =>
            {
                var response = result?.Messages?.FirstOrDefault(p => p.RequestId == x.SMSId);
                x.Status = response != null && response.ErrorCode == SuccessCode ? EnumMessageHistoryStatus.Success : EnumMessageHistoryStatus.False;
                x.ResponseBody = response != null ? response.Serialize() : null;
            });

            await _messageHistoryRepository.ExecuteTransactionAsync(async () =>
            {
                await _messageHistoryRepository.AddList(messageHistories);
                await _messageHistoryRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }

        private static string? GetTemplateId(EnumZaloTemplate template, AppSetting appSetting)
        {
            if (template == EnumZaloTemplate.OTP)
            {
                return appSetting.ZaloConfig?.OTPTemplateId;
            }

            return string.Empty;
        }
    }
}
