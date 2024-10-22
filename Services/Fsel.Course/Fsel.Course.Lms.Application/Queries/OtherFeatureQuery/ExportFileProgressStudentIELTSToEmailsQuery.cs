// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.OtherFeatureQuery
{
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
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

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
                var studentProgressReport = new StudentProgressReportIELTSModel
                {
                    FullName = student.Human?.FullName,
                    Email = student.Human?.Email,
                    School = student.School ?? schools?.FirstOrDefault(x => x.Id == student.SchoolId)?.Name,
                    ProcessDate = courseResult?.ProcessDate,
                    ExpiredDate = student.ExpiredDate
                };
                await SetProgressPlacementTestAsync(studentProgressReport, student, cancellationToken);
                if (courseResult != null)
                {
                    await SetProgressCourseAsync(studentProgressReport, courseResult);
                    await SetProgressModuleAsync(studentProgressReport, courseResult);
                    await SetOverallPercentFullMockTestAsync(studentProgressReport, courseResult);
                    await SetOverallPercentSkillMockTestAsync(studentProgressReport, courseResult);
                    await SetOverallPercentUnitAsync(studentProgressReport, courseResult);
                    await SetFeatureAccessTimeAsync(studentProgressReport, student, courseResult);
                }

                studentProgressReports.Add(studentProgressReport);
            }

            methodResult.Result = studentProgressReports.ExportExcel();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task SetProgressPlacementTestAsync(StudentProgressReportIELTSModel studentProgressReport, StudentModel student, CancellationToken cancellationToken)
        {
            var placementTestResultCurrent = await _placementTestResultRepository.Queryable.Where(x => x.StudentId == student.Id)
                                                                                              .OrderByDescending(x => x.UpdatedDate)
                                                                                              .ThenByDescending(x => x.CreatedDate)
                                                                                              .FirstOrDefaultAsync(cancellationToken);
            if (placementTestResultCurrent == null)
            {
                return;
            }
            var placementTestResult = await _placementTestResultRepository.Queryable.Where(x => x.StudentId == student.Id)
                                                          .OrderBy(x => x.CreatedDate).FirstOrDefaultAsync(cancellationToken);
            int age = Shared.Helpers.DateTimeHelper.GetYearOld(student.Human?.Birthday);
            var (levelCompleted, isLock) = placementTestResultCurrent.Level.GetLevelInScore(placementTestResultCurrent.Percent, IeltsScoreHelper.GetInitialAge(placementTestResult?.Level, age));
            studentProgressReport.StatusUser = isLock ? "Hoàn Thành PT" : "Chưa Hoàn Thành PT";
        }

        private static string GetBandScoreFormat(double bandScore)
        {
            return string.Format("{0:0.0}", bandScore);
        }

        private static string GetPercentFormat(double? percent)
        {
            return $"{percent}%";
        }

        private async Task SetProgressCourseAsync(StudentProgressReportIELTSModel studentProgressReport, CourseResult courseResult)
        {
            var courseResultModel = new CourseResultModel
            {
                CourseType = courseResult.Course?.CourseType,
                CourseId = courseResult.CourseId,
                StudentId = courseResult.StudentId,
            };
            var (currentProgress, progress) = await _managerProgressHelper.GetCompleteCourseAsync(courseResultModel);
            studentProgressReport.CourseName = courseResult.Course?.Name;
            studentProgressReport.CompletedProgress = string.Format("{0} / {1}", currentProgress, progress);
            studentProgressReport.ProgressPercent = GetPercentFormat(NumberHelper.GetPercent(currentProgress, progress));
            studentProgressReport.StatusUser = "Đang Học";
            if (courseResult.Status == EnumResultStatus.Done)
            {
                studentProgressReport.CourseOverall = GetPercentFormat(courseResult.Percent);
            }
            else
            {
                var unitResults = await _unitResultRepository.Queryable.Where(x => x.CourseId == courseResult.CourseId && x.StudentId == courseResult.StudentId && x.Status == EnumResultStatus.Done).ToListAsync();
                if (unitResults.Any())
                {
                    studentProgressReport.CourseOverall = GetPercentFormat(NumberHelper.ConvertRound(unitResults.Average(x => x.Percent)));
                }
                else
                {
                    var placementTestScore = await _placementTestResultRepository.Queryable.Where(x => x.StudentId == courseResult.StudentId && x.Status == EnumResultStatus.Done)
                                                                                       .OrderByDescending(x => x.CreatedDate)
                                                                                       .FirstOrDefaultAsync();
                    studentProgressReport.CourseOverall = GetPercentFormat(placementTestScore?.Percent);
                }
            }
        }

        private async Task SetFeatureAccessTimeAsync(StudentProgressReportIELTSModel studentProgressReport, StudentModel student, CourseResult courseResult)
        {
            var featureAccessTimeResults = await _systemService.GetFeatureAccessTimeToModulesAsync(new FeatureAccessTimesQueryModel
            {
                UserId = student.Human?.UserId ?? default,
                FeatureAccessTimes = new List<FeatureAccessTimeQueryModel>
                {
                   new FeatureAccessTimeQueryModel
                   {
                       EnumFeature = EnumFeature.VideoLesson,
                       CourseId = courseResult.CourseId
                   },
                   new FeatureAccessTimeQueryModel
                   {
                       EnumFeature = EnumFeature.ClassForum,
                       CourseId = courseResult.CourseId
                   },
                   new FeatureAccessTimeQueryModel
                   {
                       EnumFeature = EnumFeature.HomeWork,
                       CourseId = courseResult.CourseId
                   },
                   new FeatureAccessTimeQueryModel
                   {
                       CourseId = courseResult.CourseId
                   }
                }
            });

            if (!featureAccessTimeResults.IsSuccessStatusCode)
            {
                return;
            }
            var featureAccessTimes = featureAccessTimeResults.Content?.Result;

            var featureAccessTimeCourse = featureAccessTimes?.FirstOrDefault(x => !x.EnumFeature.HasValue && x.CourseId == courseResult.CourseId);
            if (featureAccessTimeCourse != null)
            {
                studentProgressReport.TotalVisit = featureAccessTimeCourse.Visit;
                studentProgressReport.TotalTime = featureAccessTimeCourse.AccessTime;
                studentProgressReport.LastVisited = featureAccessTimeCourse.LastVisited;
            }
            studentProgressReport.TimeVideoLesson = featureAccessTimes?.FirstOrDefault(x => x.EnumFeature == EnumFeature.VideoLesson)?.AccessTime ?? default;
            studentProgressReport.TimeClassForum = featureAccessTimes?.FirstOrDefault(x => x.EnumFeature == EnumFeature.ClassForum)?.AccessTime ?? default;
            studentProgressReport.TimeHomeWork = featureAccessTimes?.FirstOrDefault(x => x.EnumFeature == EnumFeature.HomeWork)?.AccessTime ?? default;
        }

        private async Task<IList<UnitResult>> GetUnitResultsAsync(CourseResult courseResult, CancellationToken cancellationToken)
        {
            var unitIds = await _courseUnitMockTestRepository.Queryable.Where(x => x.CourseId == courseResult.CourseId && x.UnitId.HasValue)
                                                                                   .OrderBy(x => x.DisplayOrder)
                                                                                   .Select(x => x.UnitId!)
                                                                                   .ToListAsync(cancellationToken);
            var unitResults = await _unitResultRepository.Queryable.Include(x => x.Unit)
                                                        .Where(x => x.StudentId == courseResult.StudentId && x.CourseId == courseResult.CourseId && unitIds.Contains(x.UnitId))
                                                        .Where(x => x.Status == EnumResultStatus.Done)
                                                        .OrderByDescending(x => x.CreatedDate)
                                                        .ToListAsync(cancellationToken);
            return unitResults.OrderBy(x => unitIds.IndexOf(x.UnitId)).ToList();
        }

        private async Task SetOverallPercentUnitAsync(StudentProgressReportIELTSModel studentProgressReport, CourseResult courseResult)
        {
            var unitResults = await GetUnitResultsAsync(courseResult, CancellationToken.None);
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

        private async Task SetOverallPercentSkillMockTestAsync(StudentProgressReportIELTSModel studentProgressReport, CourseResult courseResult)
        {
            var mockTestResults = await _mockTestResultRepository.Queryable.Where(x => x.StudentId == courseResult.StudentId && x.CourseId == courseResult.CourseId && x.Status == EnumResultStatus.Done)
                                                         .Where(x => x.UnitId.HasValue)
                                                         .OrderBy(x => x.CreatedDate)
                                                         .ToListAsync();
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

        private async Task SetOverallPercentFullMockTestAsync(StudentProgressReportIELTSModel studentProgressReport, CourseResult courseResult)
        {
            var mockTestResults = await _mockTestResultRepository.Queryable.Where(x => x.StudentId == courseResult.StudentId && x.CourseId == courseResult.CourseId && x.Status == EnumResultStatus.Done)
                                                         .Where(x => !x.UnitId.HasValue)
                                                         .OrderBy(x => x.CreatedDate)
                                                         .ToListAsync();
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

        private async Task SetProgressModuleAsync(StudentProgressReportIELTSModel studentProgressReport, CourseResult courseResult)
        {
            var unitResult = await GetUnitResultProgressAsync(courseResult, CancellationToken.None);
            if (unitResult == null)
            {
                return;
            }
            var lessonResult = await _lessonResultRepository.Queryable.Include(x => x.Lesson).Where(x => x.StudentId == courseResult.StudentId && x.UnitId == unitResult.UnitId)
                                                                    .Where(x => x.Status != EnumResultStatus.Unfinished && x.CourseId == courseResult.CourseId)
                                                                    .OrderByDescending(x => x.CreatedDate)
                                                                    .ThenByDescending(x => x.UpdatedDate)
                                                                    .FirstOrDefaultAsync();
            var lesson = lessonResult?.Lesson;
            if (lesson == null)
            {
                lesson = await _lessonRepository.Queryable.Include(x => x.UnitLessons.Where(y => y.UnitId == unitResult.UnitId)).Where(x => x.UnitLessons.Any(y => y.UnitId == unitResult.UnitId))
                                                                                      .OrderBy(x => x.UnitLessons.Max(x => x.DisplayOrder))
                                                                                      .FirstOrDefaultAsync();
            }
            studentProgressReport.CurrentPositionLesson = string.Concat(lesson?.Name);
            studentProgressReport.CurrentPositionUnit = unitResult.Unit?.Name;
        }
    }
}
