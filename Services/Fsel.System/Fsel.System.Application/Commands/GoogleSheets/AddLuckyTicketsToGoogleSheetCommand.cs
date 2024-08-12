// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.GoogleSheets
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Constants;
    using Fsel.System.Application.Queries.LuckyTickets;
    using Fsel.System.Infrastructure.ValueSettings;
    using global::System.Globalization;
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Services;
    using Google.Apis.Sheets.v4;
    using Google.Apis.Sheets.v4.Data;
    using MediatR;

    public class AddLuckyTicketsToGoogleSheetCommand : IRequest<MethodResult<VoidMethodResult>>
    {
        public string? EventCode { get; set; }
        public string? Sheet { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class AddLuckyTicketsToGoogleSheetCommandHandler : IRequestHandler<AddLuckyTicketsToGoogleSheetCommand, MethodResult<VoidMethodResult>>
    {
        private readonly AppSetting _appSetting;
        private readonly IMediator _mediator;

        public AddLuckyTicketsToGoogleSheetCommandHandler(AppSetting appSetting, IMediator mediator)
        {
            _appSetting = appSetting;
            _mediator = mediator;
        }

        public async Task<MethodResult<VoidMethodResult>> Handle(AddLuckyTicketsToGoogleSheetCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<VoidMethodResult>();

            var spreadSheetId = _appSetting.GoogleSheetConfig?.LuckyTicketSpreadSheetId;
            var sheet = request.Sheet;

            if (string.IsNullOrEmpty(spreadSheetId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var credentialsPath = ResourceSettings.I18NCredentialsFilePath;

            var queryLuckyTickets = new GetAllLuckyTicketQuery()
            {
                SchoolCode = request.EventCode,
                EndDate = request.EndDate,
                StartDate = request.StartDate,
            };

            queryLuckyTickets.SetIsQueryAll(true);

            var luckyTicketResults = await _mediator.Send(queryLuckyTickets, cancellationToken);

            var luckyTickets = luckyTicketResults.Result?.Items;

            var data = luckyTickets?.Select(item => new List<object> { item.Email ?? string.Empty, item.StudentName ?? string.Empty, DateTimeHelper.ConvertTimeFromUtc(item.CreatedDate, EnumCountryKey.Vietnam).ToString("dd-MM-yyyy HH:mm", CultureInfo.CurrentCulture), item.SchoolName ?? string.Empty, item.Ticket ?? string.Empty }).ToList();

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
