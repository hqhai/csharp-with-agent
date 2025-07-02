// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery
{
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.OrderServices;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ExportFullNameByReportProgressQuery : BaseImportCommandModel, IRequest<MethodResult<Stream>>
    {
    }

    public class ExportFullNameByReportProgressQueryHandler : IRequestHandler<ExportFullNameByReportProgressQuery, MethodResult<Stream>>
    {
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;
        private readonly ICourseRepository _courseRepository;
        private readonly ManagerProgressHelper _managerProgressHelper;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly ISystemService _systemService;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;

        public ExportFullNameByReportProgressQueryHandler(
            IUserService userService,
            IOrderService orderService,
            ICourseRepository courseRepository,
            ManagerProgressHelper managerProgressHelper,
            ILessonResultRepository lessonResultRepository,
            IUnitResultRepository unitResultRepository,
            IPlacementTestResultRepository placementTestResultRepository,
            ISystemService systemService,
            ICourseResultRepository courseResultRepository,
            ICourseUnitMockTestRepository courseUnitMockTestRepository)
        {
            _userService = userService;
            _orderService = orderService;
            _courseRepository = courseRepository;
            _managerProgressHelper = managerProgressHelper;
            _lessonResultRepository = lessonResultRepository;
            _unitResultRepository = unitResultRepository;
            _placementTestResultRepository = placementTestResultRepository;
            _systemService = systemService;
            _courseResultRepository = courseResultRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
        }

        public async Task<MethodResult<Stream>> Handle(ExportFullNameByReportProgressQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();
            var reportStudents = new List<ReportStudentInfoExportModel>();

            if (request.FormFile == null)
            {
                return methodResult;
            }
            var listEmailData = new List<ImportStudentEmailModel>();
            var result = request.FormFile.ImportAndValidateExcel(async (ImportStudentEmailModel x, IList<ImportStudentEmailModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                if (string.IsNullOrEmpty(x.Email))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = "Email is null or malformed" });
                }
                return await Task.FromResult(errors.Count == 0);
            });
            listEmailData = result.Datas.ToList();
            var fullNames = listEmailData.Where(x => !string.IsNullOrEmpty(x.Email)).Select(x => x.Email!.ToLower(System.Globalization.CultureInfo.CurrentCulture).Trim()).ToList();
            var studentResultToEmail = await _userService.GetStudentByFullNamesAsync(fullNames);
            if (!studentResultToEmail.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                return methodResult;
            }
            var students = studentResultToEmail.Content?.Result?.ToList();
            var studentIds = students?.Select(x => x.Id).ToList() ?? new List<Guid>();

            if (students != null && students.Any())
            {
                var courseResults = await _courseResultRepository.Queryable
                                                                 .Include(x => x.Course)
                                                                 .WhereBulkContains(studentIds, x => x.StudentId)
                                                                 .Where(x => x.WorkingStatus == EnumWorkingStatus.Active).OrderBy(x => x.CreatedDate)
                                                                 .ToListAsync(cancellationToken);
                foreach (var student in students)
                {
                    var userId = student?.UserId ?? default;
                    var courseResult = courseResults?.FirstOrDefault(x => x.StudentId == student.Id);
                    var reportProgress = new ReportStudentInfoExportModel
                    {
                        FullName = student?.User?.FullName,
                        Birthday = student?.User?.Birthday,
                        Email = student?.User?.Email,
                        CourseName = courseResult?.Course?.Name,
                        PhoneNumber = student?.User?.PhoneNumber,
                        CreatedDate = student?.CreatedDate,
                        CourseLevel = EnumCourseLevelHelper.GetCodeByEnumCourseLevel(courseResult?.Course?.CourseLevel),
                        SchoolName = student?.School
                    };
                    (reportProgress.PTStatus, reportProgress.PTLevel) = await GetStatusPTAsync(student, courseResult != null, cancellationToken);
                    var orderResult = await _orderService.GetOrderTrialAsync(userId);
                    if (orderResult.IsSuccessStatusCode)
                    {
                        var order = orderResult.Content?.Result;
                        if (order != null && order.IsTrial && order.ExpireDate.HasValue)
                        {
                            reportProgress.StartTrial = order.UpdatedDate ?? order.CreatedDate;
                            reportProgress.EndTrial = order.ExpireDate;
                        }
                    }
                    var featureAccessTimeResult = await _systemService.GetFeatureAccessTimeAsync(new FeatureAccessTimeQueryModel { UserId = student?.UserId ?? default });
                    if (featureAccessTimeResult.IsSuccessStatusCode)
                    {
                        reportProgress.LastEntry = featureAccessTimeResult.Content?.Result?.LastVisited;
                    }
                    await SetProgressStudentAsync(reportProgress, courseResult, cancellationToken);
                    if (courseResult != null)
                    {
                        var featureAccessTimeResults = await _systemService.GetFeatureAccessTimesAsync(new FeatureAccessTimesQueryModel
                        {
                            FeatureAccessTimes = new List<FeatureAccessTimeQueryModel>
                            {
                                new FeatureAccessTimeQueryModel
                                {
                                    CourseId = courseResult.CourseId,
                                    UserId =userId
                                },
                            },
                            UserId = userId
                        });
                        if (featureAccessTimeResults.IsSuccessStatusCode)
                        {
                            var featureAccessTimes = featureAccessTimeResults.Content?.Result;
                            var featureAccessTime = featureAccessTimes?.FirstOrDefault();
                            if (featureAccessTime != null)
                            {
                                reportProgress.TotalAccess = featureAccessTime.Visit;
                                reportProgress.TotalTime = featureAccessTime.AccessTime;
                                reportProgress.FinalStudyPeriod = featureAccessTime.LastVisited;
                            }
                            var courseResultModel = new CourseResultModel
                            {
                                CourseType = courseResult.Course?.CourseType,
                                CourseId = courseResult.CourseId,
                                StudentId = courseResult.StudentId
                            };
                            var (currentProgress, progress) = await _managerProgressHelper.GetCompleteCourseAsync(courseResultModel);
                            reportProgress.LessonCompleted = string.Format("{0} / {1}", currentProgress, progress);
                        }
                    }

                    reportStudents.Add(reportProgress);
                }
            }

            methodResult.Result = reportStudents.OrderBy(x => fullNames.IndexOf(x.FullName?.Trim().ToLower(System.Globalization.CultureInfo.CurrentCulture) ?? string.Empty)).ToList().ExportExcel();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task SetProgressStudentAsync(ReportStudentInfoExportModel reportProgress, CourseResult? courseResult, CancellationToken cancellationToken)
        {
            if (courseResult == null)
            {
                return;
            }
            var unitResult = await GetUnitResultProgressAsync(courseResult, cancellationToken);

            var unitResultDones = await _unitResultRepository.Queryable.Include(x => x.Unit).Where(x => x.StudentId == courseResult.StudentId && x.Status == EnumResultStatus.Done)
                                                       .OrderByDescending(x => x.CreatedDate).ToListAsync(cancellationToken);
            if (unitResult == null)
            {
                return;
            }
            reportProgress.UnitName = unitResult.Unit?.Name;
            reportProgress.UnitAverage = unitResultDones.Any() ? unitResultDones.Average(x => x.Percent) : unitResult.Percent;
            var lessonResult = await _lessonResultRepository.Queryable.Include(x => x.Lesson)
                                              .Where(x => x.StudentId == courseResult.StudentId && x.Status != EnumResultStatus.Unfinished)
                                              .Where(x => x.CourseId == courseResult.CourseId && x.UnitId == unitResult.UnitId)
                                              .OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync(cancellationToken);
            if (lessonResult == null)
            {
                return;
            }
            reportProgress.LessonName = lessonResult.Lesson?.Name;
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

        private async Task<(EnumResultStatus?, EnumCourseLevel?)> GetStatusPTAsync(StudentModel? student, bool isLearn, CancellationToken cancellationToken)
        {
            EnumResultStatus? status = default;
            EnumCourseLevel? level = default;
            if (student == null)
            {
                return (status, level);
            }
            int age = Shared.Helpers.DateTimeHelper.GetYearOld(student.User?.Birthday);
            var placementTestResultDone = await _placementTestResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && x.StudentId == student.Id)
                                                                          .OrderByDescending(x => x.CreatedDate)
                                                                          .FirstOrDefaultAsync(cancellationToken);

            var placementTestResultInitial = await _placementTestResultRepository.Queryable.Where(x => x.StudentId == student.Id).OrderBy(x => x.CreatedDate).FirstOrDefaultAsync(cancellationToken);
            if (placementTestResultInitial != null)
            {
                status = EnumResultStatus.Process;
            }
            if (placementTestResultDone != null)
            {
                (level, var isLock) = placementTestResultDone.Level.GetLevelInScore(placementTestResultDone.Percent, IeltsScoreHelper.GetInitialAge(placementTestResultInitial?.Level, age));
                if (isLock)
                {
                    status = EnumResultStatus.Done;
                }
            }
            if (isLearn)
            {
                return (default, level);
            }
            return (status, level);
        }
    }
}
