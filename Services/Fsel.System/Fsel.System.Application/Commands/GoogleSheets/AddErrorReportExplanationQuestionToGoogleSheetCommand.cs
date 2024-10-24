// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.GoogleSheets
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Infrastructure.ValueSettings;
    using global::System.Globalization;
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Services;
    using Google.Apis.Sheets.v4;
    using Google.Apis.Sheets.v4.Data;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class AddErrorReportExplanationQuestionToGoogleSheetCommand : AddErrorReportExplanationQuestionModel, IRequest<MethodResult<bool>>
    {
    }

    public class AddErrorReportExplanationQuestionToGoogleSheetCommandHandler : IRequestHandler<AddErrorReportExplanationQuestionToGoogleSheetCommand, MethodResult<bool>>
    {
        private readonly AppSetting _appSetting;

        public AddErrorReportExplanationQuestionToGoogleSheetCommandHandler(AppSetting appSetting)
        {
            _appSetting = appSetting;
        }

        public async Task<MethodResult<bool>> Handle(AddErrorReportExplanationQuestionToGoogleSheetCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var spreadSheetId = _appSetting.GoogleSheetConfig?.ErrorReportExplanationQuestionId;
            var sheet = _appSetting.GoogleSheetConfig?.ErrorReportExplanationQuestion;

            if (string.IsNullOrEmpty(spreadSheetId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var credentialsPath = ResourceSettings.I18NCredentialsFilePath;

            var data = new List<List<object>>() {
                new List<object>()
                {
                    request.VideoId.ToString() ,
                    request.CourseLevel.ToString(),
                    request.DisplayTime,
                    request.QuestionId.ToString(),
                    request.QuestionType.ToString(),
                    request.Config?.ToString() ?? string.Empty,
                    request.Explanation ?? string.Empty,
                    request.PromptRequest ?? string.Empty,
                    request.PromptResponse ?? string.Empty,
                    DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd-MM-yyyy HH:mm:ff", CultureInfo.CurrentCulture),
                    request.Feedback ?? string.Empty,
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
                valueRange.Values = data.ToArray();

                var appendRequest = service.Spreadsheets.Values.Append(valueRange, spreadSheetId, $"{sheet}!A:A");
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                await appendRequest.ExecuteAsync(cancellationToken);
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }
    }
}
