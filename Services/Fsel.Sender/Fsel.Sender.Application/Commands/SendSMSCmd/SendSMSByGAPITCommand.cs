// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Application.Commands.SendSMSCmd
{
    using System.Globalization;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Sender.Application.Services.SMSServices.GAPIT;
    using Fsel.Sender.Application.Services.SMSServices.GAPIT.Models;
    using Fsel.Sender.Domain.Entities;
    using Fsel.Sender.Domain.IRepositories;
    using Fsel.Sender.Domain.Models.Commands;
    using Fsel.Sender.Domain.ValueSettings;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class SendSMSByGAPITCommand : SendSMSCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class SendSMSByGAPITCommandHandler : IRequestHandler<SendSMSByGAPITCommand, MethodResult<bool>>
    {
        private readonly AppSetting _appSetting;
        private readonly IMessageHistoryRepository _messageHistoryRepository;
        private readonly IGAPITService _gapitService;

        public SendSMSByGAPITCommandHandler(AppSetting appSetting, IMessageHistoryRepository messageHistoryRepository, IGAPITService gapitService)
        {
            _appSetting = appSetting;
            _messageHistoryRepository = messageHistoryRepository;
            _gapitService = gapitService;
        }

        public async Task<MethodResult<bool>> Handle(SendSMSByGAPITCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var username = _appSetting.SMSConfig?.GAPITConfig?.UserName;
            var password = _appSetting.SMSConfig?.GAPITConfig?.Password;
            var cpId = _appSetting.SMSConfig?.GAPITConfig?.CPId;
            var serviceId = _appSetting.SMSConfig?.GAPITConfig?.ServiceId;
            var contentType = _appSetting.SMSConfig?.GAPITConfig?.ContentType;
            var brandName = _appSetting.SMSConfig?.GAPITConfig?.BrandName;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(cpId) || request.PhoneNumbers == null || request.PhoneNumbers.Count == 0 || string.IsNullOrEmpty(serviceId) || string.IsNullOrEmpty(contentType) || string.IsNullOrEmpty(brandName))
            {
                return methodResult;
            }

            string credentials = $"{username}:{password}";

            string encodeStr = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));

            string authorizationHeader = $"Basic {encodeStr}";

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

            var messageHistories = new List<MessageHistory>();
            var requests = new GAPITSendSMSRequestModels()
            {
                BrandName = brandName,
                CPId = cpId,
            };
            request.PhoneNumbers.ForEach(x =>
            {
                var smsId = Guid.NewGuid().ToString();
                var smsRequest = new GAPITSendSMSRequestModel()
                {
                    PhoneNumber = x,
                    Content = content,
                    ContentType = contentType,
                    ServiceId = serviceId,
                    MTId = smsId,
                };
                requests.SendingList.Add(smsRequest);
                messageHistories.Add(new MessageHistory()
                {
                    To = x,
                    Type = EnumMessageHistoryType.GAPITSMS,
                    SMSId = smsId,
                    Status = EnumMessageHistoryStatus.False,
                    RequestBody = smsRequest.Serialize()
                });
            });

            var sendSMSResults = await _gapitService.SendSMSs(requests, authorizationHeader);
            var results = sendSMSResults.Content;

            messageHistories.ForEach(x =>
            {
                var response = results?.Result?.FirstOrDefault(p => p.MTId == x.SMSId);
                x.Status = response != null && response.Status == 200 ? EnumMessageHistoryStatus.Success : EnumMessageHistoryStatus.False;
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
