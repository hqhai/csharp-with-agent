// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.GoogleSheets
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Services.GoogleSheetServices;
    using Fsel.System.Application.Services.GoogleSheetServices.Models;
    using Fsel.System.Infrastructure.ValueSettings;
    using MediatR;

    public class GetListStudentSchoolQuery : IRequest<MethodResult<IList<SchoolStudent>>>
    {
        public string? SchoolCode { get; set; }
    }

    public class GetListStudentSchoolQueryHandler : IRequestHandler<GetListStudentSchoolQuery, MethodResult<IList<SchoolStudent>>>
    {
        private readonly IGoogleSheetService _googleSheetService;
        private readonly AppSetting _appSetting;

        public GetListStudentSchoolQueryHandler(AppSetting appSetting)
        {
            _googleSheetService = new GoogleSheetService(ResourceSettings.I18NCredentialsFilePath);
            _appSetting = appSetting;
        }

        public async Task<MethodResult<IList<SchoolStudent>>> Handle(GetListStudentSchoolQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<SchoolStudent>>();

            var googleSheetId = _appSetting.GoogleSheetConfig?.SchoolStudentSheetId;

            if (string.IsNullOrEmpty(googleSheetId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            if (string.IsNullOrEmpty(request.SchoolCode))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            var students = new List<SchoolStudent>();
            try
            {
                IList<IList<object>> dataStudent = _googleSheetService.ReadDataFromSheet(googleSheetId, request.SchoolCode);

                if (dataStudent == null || dataStudent.Count < 2)
                {
                    return methodResult;
                }

                // Lấy headers từ dòng đầu tiên
                var headers = dataStudent[0].Select(h => h.ToString()).ToList();


                for (int i = 1; i < dataStudent.Count; i++)
                {
                    var row = dataStudent[i];
                    if (row.Count != headers.Count)
                    {
                        continue; // Bỏ qua dòng nếu số cột không khớp
                    }

                    var student = new SchoolStudent
                    {
                        FullName = row[headers.IndexOf("FullName")].ToString(),
                        Email = row[headers.IndexOf("Email")].ToString(),
                        Grade = row[headers.IndexOf("Grade")].ToString(),
                        ParentEmail = row[headers.IndexOf("ParentEmail")].ToString()
                    };

                    students.Add(student);
                }

            }
            catch (Exception ex)
            {
                methodResult.AddErrorBadRequest(ex.Message);
                return methodResult;
            }

            methodResult.Result = students;
            return methodResult;
        }
    }
}
