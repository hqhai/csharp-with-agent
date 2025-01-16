// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Application.Commands.SendSMSCmd
{
    using System.Globalization;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Caching;
    using Fsel.Sender.Application.Services.SMSServices.IRIS;
    using Fsel.Sender.Application.Services.SMSServices.IRIS.Models;
    using Fsel.Sender.Domain.Entities;
    using Fsel.Sender.Domain.IRepositories;
    using Fsel.Sender.Domain.Models.Commands;
    using Fsel.Sender.Domain.ValueSettings;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public class SendSMSByIRISCommand : SendSMSCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class SendSMSByIRISCommandHandler : IRequestHandler<SendSMSByIRISCommand, MethodResult<bool>>
    {
        private readonly IIRISServiceDC _iRISServiceDC;
        private readonly IIRISServiceDR _iRISServiceDR;
        private readonly AppSetting _appSetting;
        private readonly IMessageHistoryRepository _messageHistoryRepository;
        private readonly ILogger<SendSMSByIRISCommand> _logger;
        private readonly ICacheService<IRISSMSTokenResponseModel> _cache;

        public SendSMSByIRISCommandHandler(IIRISServiceDC iRISServiceDC, AppSetting appSetting, IMessageHistoryRepository messageHistoryRepository, IIRISServiceDR iRISServiceDR, ILogger<SendSMSByIRISCommand> logger, ICacheService<IRISSMSTokenResponseModel> cache)
        {
            _iRISServiceDC = iRISServiceDC;
            _appSetting = appSetting;
            _messageHistoryRepository = messageHistoryRepository;
            _iRISServiceDR = iRISServiceDR;
            _logger = logger;
            _cache = cache;
        }

        public async Task<MethodResult<bool>> Handle(SendSMSByIRISCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var username = _appSetting.SMSConfig?.IRISConfig?.UserName;
            var password = _appSetting.SMSConfig?.IRISConfig?.Password;
            var grantType = _appSetting.SMSConfig?.IRISConfig?.GrantType;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(grantType) || request.PhoneNumbers == null || request.PhoneNumbers.Count == 0 || request == null)
            {
                return methodResult;
            }

            string? token = await GetTokenAsync(username, password, grantType);

            if (string.IsNullOrEmpty(token))
            {
                return methodResult;
            }

            var requests = new IRISSendSMSRequestModels();
            var messageHistories = new List<MessageHistory>();

            string? content = string.Empty;
            if (request.Template.HasValue)
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format(CultureInfo.InvariantCulture, SenderSettings.SMSTemplateFileName, request.Template.Value.ToString()));
                using StreamReader streamReader = new StreamReader(path);
                var body = await streamReader.ReadToEndAsync(cancellationToken);
                var @params = ObjectHelper.GetDictionary(request.Params);
                @params.ForEach(item =>
                {
                    body = body.Replace($"[{item.Key}]", item.Value, StringComparison.CurrentCultureIgnoreCase);
                });
                content = body;
            }
            else
            {
                content = request.Content;
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
                    BrandName = _appSetting.SMSConfig?.IRISConfig?.BrandName,
                    IsCheckDuplicate = request.IsCheckDuplicate ? "1" : "0",
                    Priority = ((int)request.Priority).ToString(CultureInfo.InvariantCulture),
                    SmsId = smsId,
                    PhoneNumber = x,
                    Content = content,
                    ContentType = _appSetting.SMSConfig?.IRISConfig?.ContentType,
                };
                requests.SendingList.Add(smsRequest);
                messageHistories.Add(new MessageHistory()
                {
                    To = x,
                    Type = EnumMessageHistoryType.IRISSMS,
                    SMSId = smsId,
                    Status = EnumMessageHistoryStatus.False,
                    RequestBody = smsRequest.Serialize()
                });
            });

            IRISSendSMSResponseModels? results = null;

            try
            {
                var sendSMSResults = await _iRISServiceDC.SendSMSs(requests, token);
                if (sendSMSResults.StatusCode == HttpStatusCode.Unauthorized)
                {
                    token = await GetTokenAsync(username, password, grantType);
                    sendSMSResults = await _iRISServiceDC.SendSMSs(requests, token);
                }
                results = sendSMSResults.Content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            if (results == null || results.ResultList == null || results.ResultList.Count == 0)
            {
                try
                {
                    var sendSMSResults = await _iRISServiceDR.SendSMSs(requests, token);
                    if (sendSMSResults.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        token = await GetTokenAsync(username, password, grantType);
                        sendSMSResults = await _iRISServiceDR.SendSMSs(requests, token);
                    }
                    results = sendSMSResults.Content;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }

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

        private async Task<string> GetTokenAsync(string username, string password, string grantType)
        {
            const string CacheKey = "IRIS_Token";
            string credentials = $"{username}:{password}";
            string encodeStr = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
            string authorizationHeader = $"Basic {encodeStr}";

            var tokenModel = await _cache.GetAsync(CacheKey, TimeSpan.FromSeconds(1800), async () =>
            {
                return await GetToken(grantType, authorizationHeader);
            },
            _logger);

            if (tokenModel == null || tokenModel.ExpiresAt <= DateTime.UtcNow)
            {
                tokenModel = await GetToken(grantType, authorizationHeader);
            }
            return $"{tokenModel.TokenType} {tokenModel.AccessToken}";
        }

        private async Task<IRISSMSTokenResponseModel> GetToken(string grantType, string authorizationHeader)
        {
            var tokenResult = await _iRISServiceDC.GetToken(new IRISSMSTokenRequestModel() { GrantType = grantType }, authorizationHeader);

            if (tokenResult.IsSuccessStatusCode)
            {
                return tokenResult.Content ?? new IRISSMSTokenResponseModel();
            }
            else
            {
                _logger.LogError($"Failed to retrieve token: {tokenResult.ReasonPhrase}");
                return new IRISSMSTokenResponseModel();
            }
        }
    }
}
