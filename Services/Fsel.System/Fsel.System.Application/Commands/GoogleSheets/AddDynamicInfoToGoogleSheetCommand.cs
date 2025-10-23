// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.GoogleSheets
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.System.Application.Services.GoogleSheetServices;
    using global::System;
    using global::System.Globalization;
    using global::System.Threading.Tasks;
    using MediatR;

    public class AddDynamicInfoToGoogleSheetFileCommand : IRequest<MethodResult<bool>>
    {
        public IList<Dictionary<string, object>>? Model { get; set; }
        public string? OverrideSpreadSheetId { get; set; }
        public string? OverrideSheet { get; set; }
        public IList<string>? ColumnOrder { get; set; }
    }

    public class AddDynamicInfoToGoogleSheetFileCommandHandler : IRequestHandler<AddDynamicInfoToGoogleSheetFileCommand, MethodResult<bool>>
    {
        private readonly IGoogleSheetService _googleSheetService;

        public AddDynamicInfoToGoogleSheetFileCommandHandler(IGoogleSheetService googleSheetService)
        {
            _googleSheetService = googleSheetService;
        }

        public async Task<MethodResult<bool>> Handle(AddDynamicInfoToGoogleSheetFileCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (request.ColumnOrder == null || !request.ColumnOrder.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumValidateInputDataErrorCode.DataIsRequired));
                return methodResult;
            }

            if (request.Model == null || !request.Model.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumValidateInputDataErrorCode.DataIsRequired));
                return methodResult;
            }

            var spreadSheetId = request.OverrideSpreadSheetId;
            var sheet = request.OverrideSheet;

            if (string.IsNullOrEmpty(spreadSheetId) || string.IsNullOrEmpty(sheet))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            // Build data rows
            var data = request.Model.Select(item => BuildRowFromItem(item, request.ColumnOrder)).ToList();

            // Append to Google Sheet
            await _googleSheetService.AppendRowsAsync(spreadSheetId, sheet, data, cancellationToken);

            methodResult.Result = true;
            return methodResult;
        }

        private static IList<object> BuildRowFromItem(Dictionary<string, object> item, IList<string> columnOrder)
        {
            var row = new List<object>();
            foreach (var col in columnOrder)
            {
                if (string.Equals(col, "Time", StringComparison.OrdinalIgnoreCase))
                {
                    row.Add(DateTimeHelper.ConvertTimeFromUtc(DateTime.UtcNow, EnumCountryKey.Vietnam)
                        .ToString("dd-MM-yyyy HH:mm", CultureInfo.CurrentCulture));
                }
                else
                {
                    row.Add(item.TryGetValue(col, out var value) ? value?.ToString() ?? "" : "");
                }
            }
            return row;
        }
    }
}
