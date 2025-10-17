// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Application.Commands.SendSMSCmd
{
    using System.Globalization;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Common.Caching;
    using Fsel.Sender.Application.Services.SMSServices.IRIS;
    using Fsel.Sender.Application.Services.SMSServices.IRIS.Models;
    using Fsel.Sender.Domain.Entities;
    using Fsel.Sender.Domain.IRepositories;
    using Fsel.Sender.Domain.ValueSettings;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Polly;
    using Refit;

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
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            string? token = await GetTokenAsync(username, password, grantType);

            if (string.IsNullOrEmpty(token))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
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
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
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

            var sendSMSResults = await SendSMS(requests, token);

            messageHistories.ForEach(x =>
            {
                var response = sendSMSResults?.ResultList?.FirstOrDefault(p => p.SmsId == x.SMSId);
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
            string encodeStr = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(credentials));
            string authorizationHeader = $"Basic {encodeStr}";

            var tokenModel = await _cache.GetAsync(CacheKey, TimeSpan.FromSeconds(1200), async () =>
            {
                return await GetToken(grantType, authorizationHeader) ?? new IRISSMSTokenResponseModel();
            },
            _logger);

            if (tokenModel == null || string.IsNullOrEmpty(tokenModel.AccessToken) || tokenModel.ExpiresAt <= DateTime.UtcNow)
            {
                tokenModel = await GetToken(grantType, authorizationHeader);
                if (tokenModel != null)
                {
                    await _cache.SetAsync(CacheKey, tokenModel, TimeSpan.FromSeconds(1800));
                }
            }
            _logger.LogInformation($"Token model: {tokenModel.Serialize()}");
            return $"{tokenModel?.TokenType} {tokenModel?.AccessToken}";
        }

        private async Task<IRISSMSTokenResponseModel?> GetToken(string grantType, string authorizationHeader)
        {
            var retryPolicyDC = Policy
                .HandleResult<IApiResponse<IRISSMSTokenResponseModel>>(tokenResult => tokenResult.Content == null || !tokenResult.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt), async (result, timeSpan, retryCount, context) =>
                {
                    _logger.LogError($"Retry {retryCount} cho API DC thất bại: {result.Result.Error?.Message}");
                });

            try
            {
                var tokenResult = await retryPolicyDC.ExecuteAsync(async () =>
                {
                    var tokenResult = await _iRISServiceDC.GetToken(new IRISSMSTokenRequestModel() { GrantType = grantType }, authorizationHeader);
                    if (!tokenResult.IsSuccessStatusCode)
                    {
                        var request = new
                        {
                            Authorization = authorizationHeader,
                            Body = new IRISSMSTokenRequestModel() { GrantType = grantType }
                        }.Serialize();
                        var response = tokenResult?.Content.Serialize();
                        _logger.LogError($"Lấy Token của API DC thất bại: Api url {_appSetting.Services.IRISApiUrlDC}, Status Code {(int)tokenResult.StatusCode} - {tokenResult.StatusCode.ToString()}, Request {request}, Response {response}");
                    }
                    return tokenResult;
                });
                return tokenResult?.Content;
            }
            catch (Exception ex)
            {
                _logger.LogError($"API chính (DC) thất bại sau retry: {ex.Message}");
            }

            // Nếu API chính thất bại, chuyển sang API dự phòng (_iRISServiceDR)
            var retryPolicyDR = Policy
                .HandleResult<IApiResponse<IRISSMSTokenResponseModel>>(tokenResult => tokenResult.Content == null || !tokenResult.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt), (result, timeSpan, retryCount, context) =>
                {
                    _logger.LogError($"Retry {retryCount} cho API DR thất bại: {result.Result.Error?.Message}");
                });

            try
            {
                var tokenResult = await retryPolicyDR.ExecuteAsync(async () =>
                {
                    var tokenResult = await _iRISServiceDR.GetToken(new IRISSMSTokenRequestModel() { GrantType = grantType }, authorizationHeader);
                    if (!tokenResult.IsSuccessStatusCode)
                    {
                        var request = new
                        {
                            Authorization = authorizationHeader,
                            Body = new IRISSMSTokenRequestModel() { GrantType = grantType }
                        }.Serialize();
                        var response = tokenResult?.Content.Serialize();
                        _logger.LogError($"Lấy Token của API DR thất bại: Api url {_appSetting.Services.IRISApiUrlDR}, Status Code {(int)tokenResult.StatusCode} - {tokenResult.StatusCode.ToString()}, Request {request}, Response {response}");
                    }
                    return tokenResult;
                });
                return tokenResult?.Content;
            }
            catch (Exception ex)
            {
                _logger.LogError($"API dự phòng (DR) thất bại sau retry: {ex.Message}");
            }

            return null;
        }

        private async Task<IRISSendSMSResponseModels?> SendSMS(IRISSendSMSRequestModels requestModels, string authorizationHeader)
        {
            var retryPolicyDC = Policy
                .HandleResult<IApiResponse<IRISSendSMSResponseModels>>(tokenResult => tokenResult.Content == null || !tokenResult.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt), async (result, timeSpan, retryCount, context) =>
                {
                    _logger.LogError($"Retry {retryCount} cho API DC thất bại: {result.Result.Error?.Message}");
                });

            try
            {
                var tokenResult = await retryPolicyDC.ExecuteAsync(async () =>
                {
                    var tokenResult = await _iRISServiceDC.SendSMSs(requestModels, authorizationHeader);
                    if (!tokenResult.IsSuccessStatusCode)
                    {
                        var request = new
                        {
                            Authorization = authorizationHeader,
                            Body = requestModels
                        }.Serialize();
                        var response = tokenResult?.Content.Serialize();
                        _logger.LogError($"Send message cho API DC thất bại: Api url {_appSetting.Services.IRISApiUrlDC}, Status Code {(int)tokenResult.StatusCode} - {tokenResult.StatusCode.ToString()}, Request {request}, Response {response}");
                    }
                    return tokenResult;
                });
                return tokenResult?.Content;
            }
            catch (Exception ex)
            {
                _logger.LogError($"API chính (DC) thất bại sau retry: {ex.Message}");
            }

            // Nếu API chính thất bại, chuyển sang API dự phòng (_iRISServiceDR)
            var retryPolicyDR = Policy
                .HandleResult<IApiResponse<IRISSendSMSResponseModels>>(tokenResult => tokenResult.Content == null || !tokenResult.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt), (result, timeSpan, retryCount, context) =>
                {
                    _logger.LogError($"Retry {retryCount} cho API DR thất bại: {result.Result.Error?.Message}");
                });

            try
            {
                var tokenResult = await retryPolicyDR.ExecuteAsync(async () =>
                {
                    var tokenResult = await _iRISServiceDR.SendSMSs(requestModels, authorizationHeader);
                    if (!tokenResult.IsSuccessStatusCode)
                    {
                        var request = new
                        {
                            Authorization = authorizationHeader,
                            Body = requestModels
                        }.Serialize();
                        var response = tokenResult?.Content.Serialize();
                        _logger.LogError($"Send message cho API DR thất bại: Api url {_appSetting.Services.IRISApiUrlDR}, Status Code {(int)tokenResult.StatusCode} - {tokenResult.StatusCode.ToString()}, Request {request}, Response {response}");
                    }
                    return tokenResult;
                });
                return tokenResult?.Content;
            }
            catch (Exception ex)
            {
                _logger.LogError($"API dự phòng (DR) thất bại sau retry: {ex.Message}");
            }

            return null;
        }
    }
}
