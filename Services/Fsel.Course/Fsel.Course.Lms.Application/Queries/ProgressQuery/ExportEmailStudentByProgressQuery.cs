// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery
{
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ExportEmailStudentByProgressQuery : BaseImportCommandModel, IRequest<MethodResult<Stream>>
    {
    }

    public class ExportEmailStudentByProgressQueryHandler : IRequestHandler<ExportEmailStudentByProgressQuery, MethodResult<Stream>>
    {
        private readonly IUserService _userService;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly ISystemService _systemService;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly ManagerProgressHelper _managerProgressHelper;
        private const int NumberModuleLesson = 3;
        private const int ModuleDefault = 1;

        public ExportEmailStudentByProgressQueryHandler(
            IUserService userService,
            IUnitResultRepository unitResultRepository,
            ILessonResultRepository lessonResultRepository,
            IVideoResultRepository videoResultRepository,
            ISystemService systemService,
            IPlacementTestResultRepository placementTestResultRepository,
            IClassForumResultRepository classForumResultRepository,
            ICourseResultRepository courseResultRepository,
            IHomeWorkResultRepository homeWorkResultRepository,
            ICourseUnitMockTestRepository courseUnitMockTestRepository,
            IMockTestResultRepository mockTestResultRepository,
            IFinalTestResultRepository finalTestResultRepository,
            ManagerProgressHelper managerProgressHelper)
        {
            _userService = userService;
            _unitResultRepository = unitResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _videoResultRepository = videoResultRepository;
            _systemService = systemService;
            _placementTestResultRepository = placementTestResultRepository;
            _classForumResultRepository = classForumResultRepository;
            _courseResultRepository = courseResultRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _managerProgressHelper = managerProgressHelper;
        }

        public async Task<MethodResult<Stream>> Handle(ExportEmailStudentByProgressQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();
            var reportStudents = new List<ReportProgressStudentExportModel>();

            if (request.FormFile == null)
            {
                return methodResult;
            }
            var listEmailData = new List<ImportStudentEmailModel>();
            var result = request.FormFile.ImportAndValidateExcel(async (ImportStudentEmailModel x, IList<ImportStudentEmailModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                if (string.IsNullOrEmpty(x.Email) || !x.Email.IsValidEmail())
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = "Email is null or malformed" });
                }
                return await Task.FromResult(errors.Count == 0);
            });
            listEmailData = result.Datas.ToList();
            var emails = listEmailData.Where(x => !string.IsNullOrEmpty(x.Email)).Select(x => x.Email!.ToLower(System.Globalization.CultureInfo.CurrentCulture).Trim()).ToList();

            var studentResultToEmail = await _userService.GetStudentByEmailsAsync(emails);
            if (!studentResultToEmail.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                return methodResult;
            }
            var students = studentResultToEmail.Content?.Result?.ToList();
            var studentIds = students?.Select(x => x.Id).ToList() ?? new List<Guid>();
            var queryFeatureAccessTime = new GetFeatureAccessTimeToExportQueryModel
            {
                FeatureAccessTimes = students?.Select(student =>
                {
                    return new GetFeatureAccessTimeExportQueryModel
                    {
                        UserId = student?.UserId ?? default,
                    };
                }).ToList()
            };

            var featureAccessTimeResults = await _systemService.GetFeatureAccessTimesAsync(queryFeatureAccessTime);
            if (!featureAccessTimeResults.IsSuccessStatusCode)
            {
                methodResult.AddError(featureAccessTimeResults.Error);
                return methodResult;
            }
            var featureAccessTimes = featureAccessTimeResults.Content?.Result;
            if (students == null || !students.Any())
            {
                methodResult.Result = reportStudents.ExportExcel();
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var courseProgressCompletes = await _managerProgressHelper.GetProgressCompleteModuleExportAsync(students.Select(x => new CourseResultModel { StudentId = x.Id, CourseId = x.CourseId.GetValueOrDefault() }).ToList());
            var lessonResultIds = courseProgressCompletes.Where(x => x.LessonResult != null).Select(x => x.LessonResult).Select(x => x.Id).ToList();

            var courseResults = await _courseResultRepository.Queryable
                                                             .Include(x => x.Course)
                                                             .WhereBulkContains(studentIds, x => x.StudentId)
                                                             .Where(x => x.WorkingStatus == EnumWorkingStatus.Active)
                                                             .ToListAsync(cancellationToken);

            var lessonResults = await _lessonResultRepository.Queryable.WhereBulkContains(lessonResultIds, x => x.Id)
                .Select(x => new OverallLessonResultModel
                {
                    StudentId = x.StudentId,
                    LessonName = x.Lesson != null ? x.Lesson.Name : string.Empty,
                    Status = x.Status,
                    VideoStatus = x.VideoResult != null ? x.VideoResult.Status : EnumResultStatus.New,
                    ClassForumStatus = x.ClassForumResults.Select(x => x.Status).FirstOrDefault(),
                    HomeWorkStatus = !x.HomeWorkResults.Any() ? null : x.Status == EnumResultStatus.Done ? EnumResultStatus.Done :
                                      x.HomeWorkResults.All(x => x.Status == EnumResultStatus.Unfinished) ? null :
                                      x.HomeWorkResults.All(x => x.Status == EnumResultStatus.New) ? EnumResultStatus.New : EnumResultStatus.Process
                })
                .ToListAsync(cancellationToken);

            foreach (var student in students)
            {
                var userId = student.UserId;
                var featureAccessTimeResult = featureAccessTimes?.FirstOrDefault(x => x.CreatedUserId == userId);
                var courseProgressComplete = courseProgressCompletes.FirstOrDefault(x => x.StudentId == student.Id);
                var courseResult = courseResults.FirstOrDefault(x => x.StudentId == student.Id);
                var reportProgress = new ReportProgressStudentExportModel
                {
                    FullName = student.User?.FullName,
                    Birthday = student.User?.Birthday,
                    Email = student.User?.Email,
                    CourseName = student.ExpiredDate.HasValue ? EnumCourseLevelHelper.GetCodeByEnumCourseLevel(courseResult?.Course?.CourseLevel) : string.Empty,
                    LastEntry = featureAccessTimeResult?.LastVisited,
                    FinalStudyPeriod = featureAccessTimeResult?.LearnLastVisited,
                    UnitStatus = courseProgressComplete?.UnitResult?.Status,
                };

                if (courseResult != null && courseProgressComplete != null)
                {
                    reportProgress.UnitName = $"{nameof(Domain.Entities.Unit)} {courseProgressComplete.UnitDisplayOrder}";
                    reportProgress.TotalLessonDone = courseProgressComplete.TotalLessonDone;
                    reportProgress.ContentProgress = $"{courseProgressComplete.CountComplete} / {courseProgressComplete.TotalComplete}";
                    reportProgress.PercentProgress = $"{NumberHelper.GetPercent(courseProgressComplete.CountComplete, courseProgressComplete.TotalComplete)}%";
                }
                var lessonResult = lessonResults.FirstOrDefault(x => x.StudentId == student.Id);
                if (lessonResult != null)
                {
                    reportProgress.LessonName = lessonResult.LessonName;
                    reportProgress.ClassForumStatus = lessonResult.ClassForumStatus;
                    reportProgress.VideoStatus = lessonResult.VideoStatus;
                    reportProgress.HomeWorkStatus = lessonResult.HomeWorkStatus;
                    reportProgress.LessonStatus = lessonResult.Status;
                }
                reportStudents.Add(reportProgress);
            }
            methodResult.Result = reportStudents.OrderBy(x => emails.IndexOf(x.Email?.ToLower(System.Globalization.CultureInfo.CurrentCulture) ?? string.Empty)).ToList().ExportExcel();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private class OverallLessonResultModel
        {
            public Guid StudentId { get; set; }
            public EnumResultStatus Status { get; set; }
            public string? LessonName { get; set; }
            public EnumResultStatus? VideoStatus { get; set; }
            public EnumClassForumResultStatus? ClassForumStatus { get; set; }
            public EnumResultStatus? HomeWorkStatus { get; set; }
        }

        private class OverallModuleLearnModel
        {
            public Guid CourseId { get; set; }
            public int Count { get; set; }
        }

        private async Task<List<OverallModuleLearnModel>> GetCompleteCourseTotalsAsync(IList<CourseResultModel>? courseResults)
        {
            if (courseResults == null || !courseResults.Any())
            {
                return new List<OverallModuleLearnModel>();
            }
            var courseIds = courseResults.Select(x => x.CourseId).Distinct().ToList();
            var courses = await _courseUnitMockTestRepository.Queryable.Where(x => courseIds.Contains(x.CourseId))
                                                                      .GroupBy(x => x.CourseId)
                                                                      .Select(x => new
                                                                      {
                                                                          CourseId = x.Key,
                                                                          CourseLevel = x.Select(x => x.Course).Select(x => x.CourseLevel).FirstOrDefault(),
                                                                          CourseName = x.Select(x => x.Course).Select(x => x.Name).FirstOrDefault(),
                                                                          CountLesson = x.Where(x => x.UnitId.HasValue).Select(x => x.Unit).SelectMany(x => x.UnitLessons).Count(),
                                                                          CountSkillMockTest = x.Where(x => x.UnitId.HasValue).Select(x => x.Unit).SelectMany(x => x.UnitSkillMockTests).Count(),
                                                                          CountMockTest = x.Where(x => x.MockTestId.HasValue).Count(),
                                                                          CountFinalTest = x.Where(x => x.FinalTestId.HasValue).Count(),
                                                                      }).ToListAsync();
            return courseResults.Join(courses,
                                      courseResult => courseResult.CourseId,
                                      course => course.CourseId,
                                      (courseResult, course) => course).Select(x => new OverallModuleLearnModel
                                      {
                                          CourseId = x.CourseId,
                                          Count = x.CountLesson * NumberModuleLesson + x.CountFinalTest + x.CountSkillMockTest + x.CountFinalTest
                                      }).ToList();
        }
    }
}
