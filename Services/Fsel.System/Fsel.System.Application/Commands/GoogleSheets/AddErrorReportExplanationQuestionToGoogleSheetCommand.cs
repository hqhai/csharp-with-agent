// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.GoogleSheets
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Infrastructure.ValueSettings;
    using global::System.Globalization;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    // Đổi thành file share point
    public class AddErrorReportExplanationQuestionToGoogleSheetCommand : AddErrorReportExplanationQuestionModel, IRequest<MethodResult<bool>>
    {
    }

    public class AddErrorReportExplanationQuestionToGoogleSheetCommandHandler : IRequestHandler<AddErrorReportExplanationQuestionToGoogleSheetCommand, MethodResult<bool>>
    {
        private readonly AppSetting _appSetting;
        private readonly ISharePointService _sharePointService;

        public AddErrorReportExplanationQuestionToGoogleSheetCommandHandler(AppSetting appSetting, ISharePointService sharePointService)
        {
            _appSetting = appSetting;
            _sharePointService = sharePointService;
        }

        public async Task<MethodResult<bool>> Handle(AddErrorReportExplanationQuestionToGoogleSheetCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var siteId = _appSetting.SharePointConfig?.FSELContentTeamSiteId;
            var fileId = _appSetting.SharePointConfig?.FSELContentTeamFileId;
            var sheetName = _appSetting.SharePointConfig?.FSELContentTeamSheetName;

            if (string.IsNullOrEmpty(siteId) || string.IsNullOrEmpty(fileId) || string.IsNullOrEmpty(sheetName))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            var data = new List<List<string>>() {
                new List<string>()
                {
                    request.Id.ToString(),
                    request.ExplanationType.ToString(),
                    request.CourseLevel.ToString(),
                    request.DisplayTime.ToString(CultureInfo.InvariantCulture),
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

            var result = await _sharePointService.AddDataToExcelFile(siteId, fileId, sheetName, data);

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = result;
            return methodResult;
        }
    }
}
