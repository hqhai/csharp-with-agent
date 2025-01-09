// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Application.Commands.SendSMSCmd
{
    using System.Globalization;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Sender.Application.Services.SMSServices;
    using Fsel.Sender.Application.Services.SMSServices.Models;
    using Fsel.Sender.Domain.Entities;
    using Fsel.Sender.Domain.IRepositories;
    using Fsel.Sender.Domain.Models.Commands;
    using Fsel.Sender.Domain.ValueSettings;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class SendSMSByIRISCommand : SendSMSCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class SendSMSByIRISCommandHandler : IRequestHandler<SendSMSByIRISCommand, MethodResult<bool>>
    {
        private readonly IIRISService _sMSService;
        private readonly AppSetting _appSetting;
        private readonly IMessageHistoryRepository _messageHistoryRepository;

        public SendSMSByIRISCommandHandler(IIRISService sMSService, AppSetting appSetting, IMessageHistoryRepository messageHistoryRepository)
        {
            _sMSService = sMSService;
            _appSetting = appSetting;
            _messageHistoryRepository = messageHistoryRepository;
        }

        public async Task<MethodResult<bool>> Handle(SendSMSByIRISCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var username = _appSetting.IRISConfig?.UserName;
            var password = _appSetting.IRISConfig?.Password;
            var grantType = _appSetting.IRISConfig?.GrantType;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(grantType) || request.PhoneNumbers == null || request.PhoneNumbers.Count == 0 || request.Info == null)
            {
                return methodResult;
            }

            string credentials = $"{username}:{password}";

            string encodeStr = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));

            string authorizationHeader = $"Basic {encodeStr}";

            var tokenResult = await _sMSService.GetToken(new SMSTokenRequestModel() { GrantType = grantType }, authorizationHeader);
            if (!tokenResult.IsSuccessStatusCode)
            {
                return methodResult;
            }
            var token = $"{tokenResult.Content?.TokenType} {tokenResult.Content?.AccessToken}";

            var requests = new SendSMSRequestModels();
            var messageHistories = new List<MessageHistory>();

            string? content = string.Empty;
            if (request.Info.Template.HasValue)
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format(CultureInfo.InvariantCulture, SenderSettings.SMSTemplateFileName, request.Info.Template.Value.ToString()));
                using StreamReader streamReader = new StreamReader(path);
                var body = await streamReader.ReadToEndAsync(cancellationToken);
                var @params = ObjectHelper.GetDictionary(request.Info.Params);
                @params.ForEach(item =>
                {
                    body = body.Replace($"[{item.Key}]", item.Value, StringComparison.CurrentCultureIgnoreCase);
                });
                content = body;
            }
            else
            {
                content = request.Info.Content;
            }

            if (string.IsNullOrEmpty(content))
            {
                return methodResult;
            }

            request.PhoneNumbers.ForEach(x =>
            {
                var smsId = Guid.NewGuid().ToString();
                var smsRequest = new SMSRequestModel()
                {
                    BrandName = _appSetting.IRISConfig?.BrandName,
                    IsCheckDuplicate = request.Info.IsCheckDuplicate ? "1" : "0",
                    Priority = ((int)request.Info.Priority).ToString(CultureInfo.InvariantCulture),
                    SmsId = smsId,
                    PhoneNumber = x,
                    Content = request.Info.Content,
                    ContentType = _appSetting.IRISConfig?.ContentType,
                };
                requests.SendingList.Add(smsRequest);
                messageHistories.Add(new MessageHistory()
                {
                    To = x,
                    Type = EnumMessageHistoryType.SMS,
                    SMSId = smsId,
                    Status = EnumMessageHistoryStatus.False,
                    RequestBody = smsRequest.Serialize()
                });
            });

            var sendSMSResults = await _sMSService.SendSMSs(requests, token);
            var results = sendSMSResults.Content;

            messageHistories.ForEach(x =>
            {
                var response = results?.ResultList?.FirstOrDefault(p => p.SmsId == x.SMSId);
                x.Status = response != null && response.Code == "0" ? EnumMessageHistoryStatus.Success : EnumMessageHistoryStatus.False;
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
    }
}
