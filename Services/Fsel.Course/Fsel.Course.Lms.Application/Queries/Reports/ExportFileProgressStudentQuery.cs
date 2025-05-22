// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.Reports
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using static Fsel.Shared.Constants.ValueSettings;

    public class ExportFileProgressStudentQuery : BaseImportCommandModel, IRequest<MethodResult<Stream>>
    {
    }

    public class ExportFileProgressStudentQueryHandler : IRequestHandler<ExportFileProgressStudentQuery, MethodResult<Stream>>
    {
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ManagerProgressHelper _managerProgressHelper;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;

        public ExportFileProgressStudentQueryHandler(ICourseResultRepository courseResultRepository,
            IUserService userService,
            ISystemService systemService,
            IUnitResultRepository unitResultRepository,
            ILessonResultRepository lessonResultRepository,
            ManagerProgressHelper managerProgressHelper,
            ICourseUnitMockTestRepository courseUnitMockTestRepository)
        {
            _courseResultRepository = courseResultRepository;
            _userService = userService;
            _systemService = systemService;
            _unitResultRepository = unitResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _managerProgressHelper = managerProgressHelper;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
        }

        public async Task<MethodResult<Stream>> Handle(ExportFileProgressStudentQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<Stream>();
            ArgumentNullException.ThrowIfNull(request);
            var reportStudents = new List<StudentProgressExportModel>();
            if (request.FormFile == null)
            {
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

            var emails = result.Datas.Where(x => !string.IsNullOrEmpty(x.Email)).Select(x => x.Email!.ToLower(System.Globalization.CultureInfo.CurrentCulture).Trim()).Distinct().ToList();
            var studentResultToEmail = await _userService.GetStudentByEmailsAsync(emails);
            if (!studentResultToEmail.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResultToEmail.Error);
                return methodResult;
            }

            var students = studentResultToEmail.Content?.Result?.ToList();
            var studentIds = students?.Select(x => x.Id).ToList() ?? new List<Guid>();
            if (students != null && students.Any())
            {
                var courseResults = await _courseResultRepository.Queryable.Include(x => x.Course).Where(x => studentIds.Contains(x.StudentId) && x.WorkingStatus == EnumWorkingStatus.Active).OrderBy(x => x.CreatedDate).ToListAsync(cancellationToken);
                foreach (var student in students)
                {
                    var userId = student?.UserId ?? default;
                    var courseResult = courseResults?.FirstOrDefault(x => x.StudentId == student.Id);
                    var reportProgress = new StudentProgressExportModel
                    {
                        FullName = student?.User?.FullName,
                        Email = student?.User?.Email,
                        CourseName = courseResult?.Course?.Name,
                        ExpiredDate = student.ExpiredDate,
                        Status = courseResult == null ? ValueStatusUser.NotStarted : ValueStatusUser.InProgress,
                        ProgressDate = courseResult?.ProcessDate,
                    };
                    await SetProgressStudentAsync(reportProgress, courseResult, cancellationToken);
                    if (courseResult != null)
                    {
                        var courseResultModel = new CourseResultModel
                        {
                            CourseType = courseResult.Course?.CourseType,
                            CourseId = courseResult.CourseId,
                            StudentId = courseResult.StudentId
                        };
                        var (currentProgress, progress) = await _managerProgressHelper.GetCompleteCourseAsync(courseResultModel);
                        reportProgress.ProgressPercent = NumberHelper.GetPercent(currentProgress, progress);

                        var featureAccessTimeResult = await _systemService.GetFeatureAccessTimeAsync(new FeatureAccessTimeQueryModel { UserId = student?.UserId ?? default, CourseId = courseResult.CourseId });
                        if (featureAccessTimeResult.IsSuccessStatusCode)
                        {
                            reportProgress.TotalTime = featureAccessTimeResult.Content?.Result?.AccessTime ?? default;
                        }
                    }
                    reportStudents.Add(reportProgress);
                }
            }

            methodResult.Result = reportStudents.OrderBy(x => emails.IndexOf((x.FullName ?? string.Empty).ToLower(System.Globalization.CultureInfo.CurrentCulture).Trim())).ToList().ExportExcel();
            return methodResult;
        }

        private async Task<UnitResult?> GetUnitResultProgressAsync(CourseResult courseResult, CancellationToken cancellationToken)
        {
            var unitResults = await _unitResultRepository.Queryable.Include(x => x.Unit)
                                                        .Where(x => x.StudentId == courseResult.StudentId && x.CourseId == courseResult.CourseId)
                                                        .Where(x => x.Status != EnumResultStatus.Unfinished)
                                                        .OrderByDescending(x => x.CreatedDate)
                                                        .ToListAsync(cancellationToken);
            var unitResult = unitResults.Where(x => x.Status != EnumResultStatus.Done).OrderByDescending(x => x.CreatedDate).FirstOrDefault();
            if (unitResult == null)
            {
                var courseUnitMockTest = await _courseUnitMockTestRepository.Queryable.Where(x => x.CourseId == courseResult.CourseId && x.UnitId.HasValue)
                    .OrderByDescending(x => x.DisplayOrder)
                    .FirstOrDefaultAsync(cancellationToken);
                if (courseUnitMockTest != null)
                {
                    unitResult = unitResults.FirstOrDefault(x => x.CourseId == courseResult.CourseId && x.UnitId == courseUnitMockTest.UnitId);
                }
            }
            return unitResult;
        }

        private async Task SetProgressStudentAsync(StudentProgressExportModel reportProgress, CourseResult? courseResult, CancellationToken cancellationToken)
        {
            if (courseResult == null)
            {
                return;
            }
            var unitResult = await GetUnitResultProgressAsync(courseResult, cancellationToken);
            if (unitResult == null)
            {
                return;
            }
            reportProgress.LocationName = unitResult.Unit?.Name;
            var lessonResult = await _lessonResultRepository.Queryable.Include(x => x.Lesson)
                                              .Where(x => x.StudentId == courseResult.StudentId && x.Status != EnumResultStatus.Unfinished)
                                              .Where(x => x.CourseId == courseResult.CourseId && x.UnitId == unitResult.UnitId)
                                              .OrderByDescending(x => x.CreatedDate)
                                              .ThenByDescending(x => x.UpdatedDate)
                                              .FirstOrDefaultAsync(cancellationToken);
            if (lessonResult != null)
            {
                reportProgress.LocationName = string.Concat(lessonResult.Lesson?.Name, '_', reportProgress.LocationName);
            }
        }
    }
}
