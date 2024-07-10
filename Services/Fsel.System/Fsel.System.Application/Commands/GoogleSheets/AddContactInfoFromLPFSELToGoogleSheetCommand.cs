// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.GoogleSheets
{
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.Models.CommandModels.GoogleSheets;
    using MediatR;
    using Fsel.System.Infrastructure.ValueSettings;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Constants;
    using global::System.Globalization;
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Services;
    using Google.Apis.Sheets.v4.Data;
    using Google.Apis.Sheets.v4;

    public class AddContactInfoFromLPFSELToGoogleSheetCommand : AddContactInfoFromLPFSELToGoogleSheetCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class AddContactInfoFromLPFSELToGoogleSheetCommandHandler : IRequestHandler<AddContactInfoFromLPFSELToGoogleSheetCommand, MethodResult<bool>>
    {
        private readonly AppSetting _appSetting;

        public AddContactInfoFromLPFSELToGoogleSheetCommandHandler(AppSetting appSetting)
        {
            _appSetting = appSetting;
        }

        public async Task<MethodResult<bool>> Handle(AddContactInfoFromLPFSELToGoogleSheetCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var spreadSheetId = _appSetting.GoogleSheetConfig?.LandingPageFSELSpreadSheetId;
            var sheet = _appSetting.GoogleSheetConfig?.LandingPageFSELSheet;

            if (string.IsNullOrEmpty(spreadSheetId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var credentialsPath = ResourceSettings.I18NCredentialsFilePath;

            var birthday = request.Birthday.ToString("dd-MM-yyyy", CultureInfo.CurrentCulture);
            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd-MM-yyyy HH:mm", CultureInfo.CurrentCulture);

            var data = new List<IList<object>>()
        {
            new List<object>
            {
                request.FirstName ?? string.Empty,
                request.LastName?? string.Empty,
                request.Email ?? string.Empty,
                request.PhoneNumber ?? string.Empty,
                birthday,
                currentDate
            }
        };

            GoogleCredential credential;

            using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(new[] { SheetsService.Scope.Spreadsheets });
            }

            using (var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Google Sheets Integration",
            }))
            {
                var requestBody = service.Spreadsheets.Values.Get(spreadSheetId, $"{sheet}!A:A");
                var response = await requestBody.ExecuteAsync(cancellationToken);

                var valueRange = new ValueRange();
                valueRange.Values = data?.ToArray();

                var appendRequest = service.Spreadsheets.Values.Append(valueRange, spreadSheetId, $"{sheet}!A:A");
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                var appendResponse = await appendRequest.ExecuteAsync(cancellationToken);
            }

            return methodResult;
        }
    }
}
