// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.OtherFeatureQuery
{
    using System.Linq;
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using static Fsel.Shared.Constants.ValueSettings;

    public class ExportFileProgressStudentIELTSToEmailsQuery : BaseImportCommandModel, IRequest<MethodResult<Stream>>
    {
    }

    public class ExportFileProgressStudentIELTSToEmailsQueryHandler : IRequestHandler<ExportFileProgressStudentIELTSToEmailsQuery, MethodResult<Stream>>
    {
        private readonly IUserService _userService;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ISystemService _systemService;
        private readonly ManagerProgressHelper _managerProgressHelper;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IMapper _mapper;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;

        public ExportFileProgressStudentIELTSToEmailsQueryHandler(IUserService userService,
            ICourseResultRepository courseResultRepository,
            ISystemService systemService,
            ManagerProgressHelper managerProgressHelper,
            IUnitResultRepository unitResultRepository,
            ILessonResultRepository lessonResultRepository,
            IPlacementTestResultRepository placementTestResultRepository,
            ILessonRepository lessonRepository,
            IMockTestResultRepository mockTestResultRepository,
            IMapper mapper,
            ICourseUnitMockTestRepository courseUnitMockTestRepository)
        {
            _userService = userService;
            _courseResultRepository = courseResultRepository;
            _systemService = systemService;
            _managerProgressHelper = managerProgressHelper;
            _unitResultRepository = unitResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _placementTestResultRepository = placementTestResultRepository;
            _lessonRepository = lessonRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _mapper = mapper;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
        }

        public async Task<MethodResult<Stream>> Handle(ExportFileProgressStudentIELTSToEmailsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            if (request.FormFile == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.FormFile));
                return methodResult;
            }
            var studentProgressReports = new List<StudentProgressReportIELTSModel>();

            var result = request.FormFile.ImportAndValidateExcel(async (ImportStudentEmailModel x, IList<ImportStudentEmailModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                if (string.IsNullOrEmpty(x.Email) || !x.Email.IsValidEmail())
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = "Email is null or malformed" });
                }
                return await Task.FromResult(errors.Count == 0);
            });
            result.Datas = result.Datas.Where(x => !string.IsNullOrEmpty(x.Email) && x.Email.IsValidEmail()).Distinct().ToList();
            if (result.Stream != null)
            {
                methodResult.Result = result.Stream;
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var listEmail = result.Datas.Where(x => !string.IsNullOrEmpty(x.Email)).Select(x => x.Email!.ToLower(System.Globalization.CultureInfo.CurrentCulture)).ToList();
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
                .WhereBulkContains(studentIds, x => x.StudentId)
                .Where(x => x.WorkingStatus == EnumWorkingStatus.Active)
                .ToListAsync(cancellationToken);

            var placemenTestResultGroups = await _placementTestResultRepository.Queryable
                .WhereBulkContains(studentIds, x => x.StudentId)
                .Where(x => x.Status == EnumResultStatus.Done)
                .GroupBy(x => x.StudentId)
                .Select(x => new
                {
                    StudentId = x.Key,
                    PlacementTestCurrent = x.Select(x => x).OrderBy(x => x.CreatedDate).FirstOrDefault(),
                    PlacementTestEnd = x.Select(x => x).OrderByDescending(x => x.CreatedDate).FirstOrDefault(),
                }).ToListAsync(cancellationToken);
            var unitResultGroups = await (from baseQ in _courseResultRepository.Queryable.WhereBulkContains(studentIds, x => x.StudentId)
                                          join cum in _courseUnitMockTestRepository.Queryable on baseQ.CourseId equals cum.CourseId
                                          join ur in _unitResultRepository.Queryable on new { cum.CourseId, baseQ.StudentId, UnitId = cum.UnitId } equals new { ur.CourseId, ur.StudentId, UnitId = (Guid?)ur.UnitId }
                                          where baseQ.WorkingStatus == EnumWorkingStatus.Active && ur.Status == EnumResultStatus.Done
                                          group ur by ur.StudentId into g
                                          select new
                                          {
                                              StudentId = g.Key,
                                              UnitResults = g.Select(x => x).OrderBy(x => x.CreatedDate).ToList()
                                          }).ToListAsync(cancellationToken);
            var mockTestResultGroups = await (from baseQ in _courseResultRepository.Queryable.WhereBulkContains(studentIds, x => x.StudentId)
                                              join cum in _courseUnitMockTestRepository.Queryable on baseQ.CourseId equals cum.CourseId
                                              join mtr in _mockTestResultRepository.Queryable on new { cum.CourseId, baseQ.StudentId, MockTestId = cum.MockTestId } equals new { mtr.CourseId, mtr.StudentId, MockTestId = (Guid?)mtr.MockTestId }
                                              where baseQ.WorkingStatus == EnumWorkingStatus.Active && mtr.Status == EnumResultStatus.Done
                                              group mtr by mtr.StudentId into g
                                              select new
                                              {
                                                  StudentId = g.Key,
                                                  MockTestResults = g.Select(x => x).OrderBy(x => x.CreatedDate).ToList()
                                              }).ToListAsync(cancellationToken);

            var skillMockTestResultGroups = await (from baseQ in _courseResultRepository.Queryable
                                                   join cum in _courseUnitMockTestRepository.Queryable on baseQ.CourseId equals cum.CourseId
                                                   join ur in _unitResultRepository.Queryable on new { cum.CourseId, baseQ.StudentId, UnitId = cum.UnitId } equals new { ur.CourseId, ur.StudentId, UnitId = (Guid?)ur.UnitId }
                                                   join mtr in _mockTestResultRepository.Queryable on new { ur.CourseId, ur.StudentId, UnitId = (Guid?)ur.UnitId } equals new { mtr.CourseId, mtr.StudentId, UnitId = mtr.UnitId }
                                                   where baseQ.WorkingStatus == EnumWorkingStatus.Active && mtr.Status == EnumResultStatus.Done
                                                   group mtr by mtr.StudentId into g
                                                   select new
                                                   {
                                                       StudentId = g.Key,
                                                       MockTestResults = g.Select(x => x).OrderBy(x => x.CreatedDate).ToList()
                                                   }).ToListAsync(cancellationToken);

            var courseCompletes = await _managerProgressHelper.GetProgressCompleteModuleExportAsync(courseResults.Select(x => new CourseResultModel { CourseId = x.CourseId, StudentId = x.StudentId }).ToList());
            var lessonResultIds = courseCompletes.Where(x => x.LessonResult != null).Select(x => x.LessonResult).Select(x => x.Id).ToList() ?? new List<Guid>();

            var lessonResults = await _lessonResultRepository.Queryable
               .WhereBulkContains(lessonResultIds, x => x.Id)
               .Select(x => new
               {
                   StudentId = x.StudentId,
                   LessonName = x.Lesson != null ? x.Lesson.Name : string.Empty,
               })
               .ToListAsync(cancellationToken);

            var queryFeatureAccessTime = new GetFeatureAccessTimeToExportQueryModel
            {
                FeatureAccessTimes = students.SelectMany(student =>
                {
                    return new List<GetFeatureAccessTimeExportQueryModel>
                    {
                        new GetFeatureAccessTimeExportQueryModel
                        {
                            UserId = student.UserId,
                            EnumFeature = EnumFeature.VideoLesson,
                            CourseId = student.CourseId,
                        },
                        new GetFeatureAccessTimeExportQueryModel
                        {
                            UserId = student.UserId,
                            EnumFeature = EnumFeature.ClassForum,
                            CourseId =student.CourseId,
                        },
                        new GetFeatureAccessTimeExportQueryModel
                        {
                            UserId = student.UserId,
                            EnumFeature = EnumFeature.HomeWork,
                            CourseId = student.CourseId,
                        },
                        new GetFeatureAccessTimeExportQueryModel
                        {
                            UserId = student.UserId,
                            CourseId = student.CourseId,
                        },
                        new GetFeatureAccessTimeExportQueryModel
                        {
                            UserId = student.UserId,
                        }
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
            foreach (var student in students)
            {
                var courseResult = courseResults.FirstOrDefault(x => x.StudentId == student.Id);
                var courseComplete = courseCompletes.FirstOrDefault(x => x.StudentId == student.Id);
                var placementTestStudent = placemenTestResultGroups.FirstOrDefault(x => x.StudentId == student.Id);
                var unitResultGroup = unitResultGroups.FirstOrDefault(x => x.StudentId == student.Id);
                var mockTestResultGroup = mockTestResultGroups.FirstOrDefault(x => x.StudentId == student.Id);
                var skillMockTestResultGroup = skillMockTestResultGroups.FirstOrDefault(x => x.StudentId == student.Id);
                var lessonResult = lessonResults.FirstOrDefault(x => x.StudentId == student.Id);
                var featureAccessTimeStudent = featureAccessTimes?.Where(x => x.CreatedUserId == student.UserId).ToList();

                var featureAccessTime = featureAccessTimeStudent?.FirstOrDefault(x => !x.EnumFeature.HasValue && !x.CourseId.HasValue);

                var studentProgressReport = new StudentProgressReportIELTSModel
                {
                    FullName = student.User?.FullName,
                    Email = student.User?.Email,
                    School = student.School ?? schools?.FirstOrDefault(x => x.Id == student.SchoolId)?.Name,
                    ProcessDate = courseResult?.ProcessDate,
                    ExpiredDate = student.ExpiredDate,
                    LastVisited = featureAccessTime?.LastVisited,
                    CourseName = student.ExpiredDate.HasValue ? EnumCourseLevelHelper.GetCodeByEnumCourseLevel(courseResult?.Course?.CourseLevel) : string.Empty,
                };

                if (placementTestStudent != null && placementTestStudent.PlacementTestEnd != null)
                {
                    int age = Shared.Helpers.DateTimeHelper.GetYearOld(student.User?.Birthday);
                    var (levelCompleted, isLock) = placementTestStudent.PlacementTestEnd.Level.GetLevelInScore(placementTestStudent.PlacementTestEnd.Percent, IeltsScoreHelper.GetInitialAge(placementTestStudent.PlacementTestCurrent?.Level, age));
                    studentProgressReport.StatusUser = isLock ? ValueStatusUser.CompletedPlacementTest : ValueStatusUser.NotCompletedPlacementTest;
                }
                if (unitResultGroup != null)
                {
                    SetOverallPercentUnitAsync(studentProgressReport, unitResultGroup.UnitResults);
                    studentProgressReport.CourseOverall = unitResultGroup.UnitResults.Any() ? GetPercentFormat(NumberHelper.ConvertRound(unitResultGroup.UnitResults.Average(x => x.Percent))) : null;
                }
                if (courseComplete != null)
                {
                    studentProgressReport.TotalLessonDone = courseComplete.TotalLessonDone;
                    studentProgressReport.ProgressPercent = GetPercentFormat(NumberHelper.GetPercent(courseComplete.CountComplete, courseComplete.TotalComplete));
                    studentProgressReport.CompletedProgress = $"{courseComplete.CountComplete} / {courseComplete.TotalComplete}";
                    studentProgressReport.CurrentPositionUnit = $"{nameof(Domain.Entities.Unit)}{courseComplete.UnitDisplayOrder}";
                    studentProgressReport.CurrentPositionLesson = lessonResult?.LessonName;
                }
                if (mockTestResultGroup != null)
                {
                    SetOverallPercentFullMockTestAsync(studentProgressReport, mockTestResultGroup.MockTestResults);
                }
                if (skillMockTestResultGroup != null)
                {
                    SetOverallPercentSkillMockTestAsync(studentProgressReport, skillMockTestResultGroup.MockTestResults);
                }
                if (courseResult != null)
                {
                    if (courseResult.Status == EnumResultStatus.Done)
                    {
                        studentProgressReport.CourseOverall = GetPercentFormat(courseResult.Percent);
                    }
                    studentProgressReport.StatusUser = ValueStatusUser.InProgress;
                    SetFeatureAccessTimeAsync(studentProgressReport, featureAccessTimeStudent, courseResult);
                }
                studentProgressReports.Add(studentProgressReport);
            }

            methodResult.Result = studentProgressReports.OrderBy(x => listEmail.IndexOf(x.Email!.ToLower(System.Globalization.CultureInfo.CurrentCulture))).ToList().ExportExcel();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static string GetBandScoreFormat(double bandScore)
        {
            return string.Format("{0:0.0}", bandScore);
        }

        private static string GetPercentFormat(double? percent)
        {
            return $"{percent}%";
        }

        private static void SetFeatureAccessTimeAsync(StudentProgressReportIELTSModel studentProgressReport, IList<FeatureAccessTimeModel>? featureAccessTimes, CourseResult courseResult)
        {
            var featureAccessTimeCourse = featureAccessTimes?.FirstOrDefault(x => !x.EnumFeature.HasValue && x.CourseId == courseResult.CourseId);

            if (featureAccessTimeCourse != null)
            {
                studentProgressReport.TotalVisit = featureAccessTimeCourse.Visit;
                studentProgressReport.TotalTime = featureAccessTimeCourse.AccessTime;
            }

            studentProgressReport.TimeVideoLesson = featureAccessTimes?.FirstOrDefault(x => x.EnumFeature == EnumFeature.VideoLesson)?.AccessTime ?? default;
            studentProgressReport.TimeClassForum = featureAccessTimes?.FirstOrDefault(x => x.EnumFeature == EnumFeature.ClassForum)?.AccessTime ?? default;
            studentProgressReport.TimeHomeWork = featureAccessTimes?.FirstOrDefault(x => x.EnumFeature == EnumFeature.HomeWork)?.AccessTime ?? default;
        }

        private static void SetOverallPercentUnitAsync(StudentProgressReportIELTSModel studentProgressReport, IList<UnitResult>? unitResults)
        {
            if (unitResults == null || !unitResults.Any())
            {
                return;
            }
            foreach (var unitResult in unitResults)
            {
                var index = unitResults.IndexOf(unitResult);
                var unitField = typeof(StudentProgressReportIELTSModel).GetProperty($"OverallUnit{index + 1}");
                if (unitField == null)
                {
                    continue;
                }
                unitField.SetValue(studentProgressReport, GetPercentFormat(unitResult.Percent));
            }
        }

        private void SetOverallPercentSkillMockTestAsync(StudentProgressReportIELTSModel studentProgressReport, IList<MockTestResult>? mockTestResults)
        {
            if (mockTestResults == null || !mockTestResults.Any())
            {
                return;
            }

            foreach (var mockTestResult in mockTestResults)
            {
                var index = mockTestResults.IndexOf(mockTestResult);
                var skillMockTestField = typeof(StudentProgressReportIELTSModel).GetProperty($"BandSkillMockTest{index + 1}");
                if (skillMockTestField == null)
                {
                    continue;
                }
                var mockTestResultModel = _mapper.Map<MockTestResultModel>(mockTestResult);
                skillMockTestField.SetValue(studentProgressReport, GetBandScoreFormat(mockTestResultModel.Scores));
            }
        }

        private void SetOverallPercentFullMockTestAsync(StudentProgressReportIELTSModel studentProgressReport, IList<MockTestResult>? mockTestResults)
        {
            if (mockTestResults == null || !mockTestResults.Any())
            {
                return;
            }
            foreach (var mockTestResult in mockTestResults)
            {
                var index = mockTestResults.IndexOf(mockTestResult) + 1;
                var mockTestField = typeof(StudentProgressReportIELTSModel).GetProperty($"BandFullMockTest{index}");
                if (mockTestField == null)
                {
                    continue;
                }
                var mockTestResultModel = _mapper.Map<MockTestResultModel>(mockTestResult);
                mockTestField.SetValue(studentProgressReport, GetBandScoreFormat(mockTestResultModel.Scores));
                SetSkillFullMockTest(studentProgressReport, mockTestResult.SkillScores, index);
            }
        }

        private static void SetSkillFullMockTest(StudentProgressReportIELTSModel studentProgressReport, IList<SkillScores>? skillScores, int index)
        {
            if (skillScores == null)
            {
                return;
            }

            foreach (var item in skillScores)
            {
                if (item == null)
                {
                    continue;
                }
                var mockTestField = typeof(StudentProgressReportIELTSModel).GetProperty($"BandFullMockTest{item.Skill}{index}");
                if (mockTestField == null)
                {
                    continue;
                }
                mockTestField.SetValue(studentProgressReport, GetBandScoreFormat(item.Scores));
            }
        }
    }
}
