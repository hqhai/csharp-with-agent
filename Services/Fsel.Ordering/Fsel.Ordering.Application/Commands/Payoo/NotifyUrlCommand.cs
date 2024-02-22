// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.Payoo
{
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Ordering.Application.Services.PayooService.Models;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class NotifyUrlCommand : NotifyUrlCommandModel, IRequest<MethodResult<NotifyUrlModel>>
    {
    }

    public class NotifyUrlCommandHandler : IRequestHandler<NotifyUrlCommand, MethodResult<NotifyUrlModel>>
    {
        private readonly AppSetting _appSetting;
        private readonly ILogger<NotifyUrlCommand> _logger;
        private readonly IUrBoxTransactionRepository _urBoxTransactionRepository;

        public NotifyUrlCommandHandler(AppSetting appSetting, ILogger<NotifyUrlCommand> logger, IUrBoxTransactionRepository urBoxTransactionRepository)
        {
            _appSetting = appSetting;
            _logger = logger;
            _urBoxTransactionRepository = urBoxTransactionRepository;
        }

        public async Task<MethodResult<NotifyUrlModel>> Handle(NotifyUrlCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<NotifyUrlModel>();

            var secureHash = ValidateSecureHash(_appSetting.PayooConfig?.Key ?? string.Empty, request.ResponseData ?? string.Empty, _appSetting.PayooConfig?.PayooIP ?? string.Empty);
            var response = await _urBoxTransactionRepository.GetByIdAsync(new Guid("ed24380c-407c-4790-a450-093edd5c1f57"));
            if (response == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            response.ResponseBody = request;

            if (secureHash != request.SecureHash)
            {
                _logger.LogError($"ReturnCode: 1");
                _urBoxTransactionRepository.Update(response);
                await _urBoxTransactionRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.Result = new NotifyUrlModel { ReturnCode = 1, Description = string.Empty };
                return methodResult;
            }
            _urBoxTransactionRepository.Update(response);
            await _urBoxTransactionRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogError($"ReturnCode: 0");
            methodResult.Result = new NotifyUrlModel { ReturnCode = 0, Description = string.Empty };
            return methodResult;
        }

        public string ValidateSecureHash(string checksumKey, string responseData, string payooIP)
        {
            try
            {
                string stringToHash = checksumKey + responseData + payooIP;
                byte[] dataBytes = Encoding.UTF8.GetBytes(stringToHash);
                byte[] hashValue = SHA512.HashData(dataBytes);
                StringBuilder builder = new StringBuilder();
                foreach (byte b in hashValue)
                {
                    builder.Append(b.ToString("x2", CultureInfo.InvariantCulture));
                }

                return builder.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
