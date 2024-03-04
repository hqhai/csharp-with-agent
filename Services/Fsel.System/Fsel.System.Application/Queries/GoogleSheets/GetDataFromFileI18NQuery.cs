// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.GoogleSheets
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Constants;
    using Fsel.System.Application.Services.GoogleSheetServices;
    using Fsel.System.Application.Services.GoogleSheetServices.Models;
    using Fsel.System.Infrastructure.ValueSettings;
    using MediatR;

    public class GetDataFromFileI18NQuery : IRequest<MethodResult<I18NModel>>
    {
    }

    public class GetDataFromFileI18NQueryHandler : IRequestHandler<GetDataFromFileI18NQuery, MethodResult<I18NModel>>
    {
        private readonly IGoogleSheetService _googleSheetService;
        private readonly AppSetting _appSetting;

        public GetDataFromFileI18NQueryHandler(AppSetting appSetting)
        {
            _googleSheetService = new GoogleSheetService(ResourceSettings.I18NCredentialsFilePath);
            _appSetting = appSetting;
        }

        public async Task<MethodResult<I18NModel>> Handle(GetDataFromFileI18NQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<I18NModel>();

            var i18nSpreadSheetId = _appSetting.GoogleSheetConfig?.I18NSpreadSheetId;

            if (string.IsNullOrEmpty(i18nSpreadSheetId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            var i18n = new I18NModel();
            try
            {
                IList<IList<object>> dataVN = _googleSheetService.ReadDataFromSheet(i18nSpreadSheetId, _appSetting.GoogleSheetConfig?.I18NSheetVN ?? string.Empty);

                foreach (var dataItem in dataVN)
                {
                    var key = dataItem.FirstOrDefault()?.ToString();
                    var value = dataItem.LastOrDefault()?.ToString();
                    if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                    {
                        i18n.AddLanguage("vn", key, value);
                    }
                }
            }
            catch (Exception ex)
            {
                methodResult.AddErrorBadRequest(ex.Message);
                return methodResult;
            }

            try
            {
                IList<IList<object>> dataEN = _googleSheetService.ReadDataFromSheet(i18nSpreadSheetId, _appSetting.GoogleSheetConfig?.I18NSheetEN ?? string.Empty);

                foreach (var dataItem in dataEN)
                {
                    var firstValue = dataItem.FirstOrDefault()?.ToString();
                    var lastValue = dataItem.LastOrDefault()?.ToString();
                    if (!string.IsNullOrEmpty(firstValue) && !string.IsNullOrEmpty(lastValue))
                    {
                        i18n.AddLanguage("en", firstValue, lastValue);
                    }
                }
            }
            catch (Exception ex)
            {
                methodResult.AddErrorBadRequest(ex.Message);
                return methodResult;
            }

            methodResult.Result = i18n;
            return methodResult;
        }
    }
}
