// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.OtherFeatureQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ExportFileProgressStudentsToEmailsQuery : BaseImportCommandModel, IRequest<MethodResult<Stream>>
    {
    }

    public class ExportFileProgressStudentsToEmailsQueryHandler : IRequestHandler<ExportFileProgressStudentsToEmailsQuery, MethodResult<Stream>>
    {
        private readonly IUserService _userService;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ISystemService _systemService;

        public ExportFileProgressStudentsToEmailsQueryHandler(IUserService userService, ICourseResultRepository courseResultRepository, ISystemService systemService)
        {
            _userService = userService;
            _courseResultRepository = courseResultRepository;
            _systemService = systemService;
        }

        public async Task<MethodResult<Stream>> Handle(ExportFileProgressStudentsToEmailsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            if (request.FormFile == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.FormFile));
                return methodResult;
            }
            var studentProgressReports = new List<StudentProgressReportModel>();

            var result = request.FormFile.ImportAndValidateExcel(async (ImportStudentEmailModel x, IList<ImportStudentEmailModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                if (string.IsNullOrEmpty(x.Email) || !x.Email.IsValidEmail())
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = "Email is null or malformed" });
                }
                return await Task.FromResult(errors.Count == 0);
            });
            var duplicateEmails = result.Datas.GroupBy(user => user.Email).Where(group => group.Count() > 1).Select(group => group.Key);
            if (duplicateEmails.Any())
            {
                methodResult.AddErrorBadRequest("Duplicate Emails");
                return methodResult;
            }

            if (result.Stream != null)
            {
                methodResult.Result = result.Stream;
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var listEmail = result.Datas.Where(x => !string.IsNullOrEmpty(x.Email)).Select(x => x.Email!).ToList();
            var studentResultToEmail = await _userService.GetStudentByEmailsAsync(listEmail);
            if (!studentResultToEmail.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResultToEmail.Error);
                return methodResult;
            }
            var students = studentResultToEmail.Content?.Result?.ToList();
            if (students == null || !students.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(students));
                return methodResult;
            }
            var schoolIds = students.Where(x => x.SchoolId.HasValue).Select(x => x.SchoolId.GetValueOrDefault()).ToList();
            var studentIds = students.Select(x => x.Id).ToList() ?? new List<Guid>();

            var schoolResults = await _systemService.GetSchoolsAsync(schoolIds);
            if (!schoolResults.IsSuccessStatusCode)
            {
                methodResult.AddError(schoolResults.Error);
                return methodResult;
            }
            var schools = schoolResults.Content?.Result;

            var courseResults = await _courseResultRepository.Queryable.Include(x => x.Course)
                .Where(x => studentIds.Contains(x.StudentId) && x.WorkingStatus == EnumWorkingStatus.Active)
                .ToListAsync(cancellationToken);
            foreach (var student in students)
            {
                var courseResult = courseResults.FirstOrDefault(x => x.StudentId == student.Id);
                var studentProgressReport = new StudentProgressReportModel
                {
                    FullName = student.Human?.FullName,
                    Email = student.Human?.Email,
                    School = student.School ?? schools?.FirstOrDefault(x => x.Id == student.Id)?.Name,
                    CourseName = courseResult?.Course?.Name
                };
                studentProgressReports.Add(studentProgressReport);
            }

            methodResult.Result = studentProgressReports.ExportExcel();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
