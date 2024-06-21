// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.GoogleSheets
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Constants;
    using Fsel.System.Application.Services.GoogleSheetServices;
    using Fsel.System.Infrastructure.ValueSettings;
    using MediatR;

    public class CCEmailModel
    {
        public string? StudentEmail { get; set; }
        public string? OCEmail { get; set; }
        public string? OMEmail { get; set; }
    }

    public class GetCCEmailsAccordingToStudentsQuery : IRequest<MethodResult<IList<CCEmailModel>>>
    {
    }

    public class GetCCEmailsAccordingToStudentsQueryHandler : IRequestHandler<GetCCEmailsAccordingToStudentsQuery, MethodResult<IList<CCEmailModel>>>
    {
        private readonly IGoogleSheetService _googleSheetService;
        private readonly AppSetting _appSetting;

        public GetCCEmailsAccordingToStudentsQueryHandler(AppSetting appSetting)
        {
            _googleSheetService = new GoogleSheetService(ResourceSettings.I18NCredentialsFilePath);
            _appSetting = appSetting;
        }

        public async Task<MethodResult<IList<CCEmailModel>>> Handle(GetCCEmailsAccordingToStudentsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<CCEmailModel>>();

            var ccEmailSpreadSheetId = _appSetting.GoogleSheetConfig?.CCEmailSpreadSheetId;

            if (string.IsNullOrEmpty(ccEmailSpreadSheetId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var ccEmails = new List<CCEmailModel>();
            try
            {
                IList<IList<object>> data = _googleSheetService.ReadDataFromSheet(ccEmailSpreadSheetId, _appSetting.GoogleSheetConfig?.CCEmailSheet ?? string.Empty);

                foreach (var dataItem in data)
                {
                    var student = dataItem[0].ToString();
                    var oc = dataItem[1].ToString();
                    var om = dataItem[2].ToString();
                    if (!string.IsNullOrEmpty(student) && !string.IsNullOrEmpty(oc))
                    {
                        ccEmails.Add(new CCEmailModel
                        {
                            StudentEmail = student,
                            OCEmail = oc,
                            OMEmail = om
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                methodResult.AddErrorBadRequest(ex.Message);
                return methodResult;
            }

            ccEmails.RemoveAt(0);
            methodResult.Result = ccEmails;
            return methodResult;
        }
    }
}
