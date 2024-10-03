// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.Reports
{
    using System.Globalization;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ExportTimeReportStudentQuery : BaseImportCommandModel, IRequest<MethodResult<Stream>>
    {
    }

    public class ExportTimeReportStudentQueryHandler : IRequestHandler<ExportTimeReportStudentQuery, MethodResult<Stream>>
    {
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;

        public ExportTimeReportStudentQueryHandler(IUserService userService, ISystemService systemService)
        {
            _userService = userService;
            _systemService = systemService;
        }

        public async Task<MethodResult<Stream>> Handle(ExportTimeReportStudentQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<Stream>();
            ArgumentNullException.ThrowIfNull(request);
            var reportStudents = new List<ReportTimeStudentModel>();
            if (request.FormFile == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.FormFile));
                return methodResult;
            }
            var result = request.FormFile.ImportAndValidateExcel(async (ImportStudentEmailModel x, IList<ImportStudentEmailModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                if (string.IsNullOrEmpty(x.Email) || !x.Email.IsValidEmail())
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = "Email is null or malformed" });
                }
                return await Task.FromResult(errors.Count == 0);
            });

            if (result.Stream != null)
            {
                methodResult.Result = result.Stream;
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var emails = result.Datas.Where(x => !string.IsNullOrEmpty(x.Email)).Select(x => x.Email!).Distinct().ToList();
            var studentResultToEmail = await _userService.GetStudentByEmailsAsync(emails);
            if (!studentResultToEmail.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResultToEmail.Error);
                return methodResult;
            }

            var students = studentResultToEmail.Content?.Result?.ToList();
            if (students == null || students.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(students));
                return methodResult;
            }

            var studentIds = students.Select(x => x.Id).ToList() ?? new List<Guid>();
            foreach (var student in students)
            {
                var reportProgress = new ReportTimeStudentModel
                {
                    FullName = student.Human?.FullName,
                    Email = student.Human?.Email,
                };
                var featureAccessTimeResult = await _systemService.GetFeatureAccessTimesAsync(new FeatureAccessTimesQueryModel
                {
                    UserId = student.Human?.UserId ?? default,
                    FeatureAccessTimes = new List<FeatureAccessTimeQueryModel> {
                        new FeatureAccessTimeQueryModel
                        {
                            EnumFeature = EnumFeature.ClassForum,
                        },
                        new FeatureAccessTimeQueryModel
                        {
                            EnumFeature = EnumFeature.HomeWork,
                        },
                        new FeatureAccessTimeQueryModel
                        {
                            EnumFeature = EnumFeature.VideoLesson,
                        }
                    }
                });
                if (featureAccessTimeResult.IsSuccessStatusCode)
                {
                    var featureAccessTimes = featureAccessTimeResult.Content?.Result;
                    reportProgress.CurrentTimeVideoLesson = GetLastVisiteDate(featureAccessTimes, EnumFeature.VideoLesson);
                    reportProgress.CurrentTimeClassForum = GetLastVisiteDate(featureAccessTimes, EnumFeature.ClassForum);
                    reportProgress.CurrentTimeHomework = GetLastVisiteDate(featureAccessTimes, EnumFeature.HomeWork);
                }
                reportStudents.Add(reportProgress);
            }
            methodResult.Result = reportStudents.OrderBy(x => x.FullName).ToList().ExportExcel();
            return methodResult;
        }

        private static DateTime? GetLastVisiteDate(IList<FeatureAccessTimeModel>? featureAccessTimes, EnumFeature feature)
        {
            var data = featureAccessTimes?.FirstOrDefault(x => x.EnumFeature == feature);
            if (data == null || !data.LastVisited.HasValue)
            {
                return null;
            }
            return data.LastVisited.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam);
        }
    }
}
