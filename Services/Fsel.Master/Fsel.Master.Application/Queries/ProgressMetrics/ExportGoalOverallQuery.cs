// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Application.Queries.ProgressMetrics
{
    using System.Collections.Concurrent;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Master.Domain.Entities;
    using Fsel.Master.Domain.IRepositories;
    using Fsel.Master.Domain.Models.Enums;
    using Fsel.Master.Domain.Models.QueryModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using OfficeOpenXml;

    public class ExportGoalOverallQuery : BaseProgressMetricsQueryModel, IRequest<MethodResult<byte[]>>
    {
        public DateTime Date { get; set; }
    }

    public class ExportGoalOverallQueryHandler : IRequestHandler<ExportGoalOverallQuery, MethodResult<byte[]>>
    {
        private readonly IMasterBaseRepository<StudentProfileReport> _studentRepository;
        private readonly IMasterBaseRepository<StudentCompetitionEvent> _studentCompetitionEventRepository;
        private readonly IMasterBaseRepository<CourseResult> _courseResultRepository;
        private readonly IMasterBaseRepository<LessonResult> _lessonResultRepository;
        private readonly IMasterBaseRepository<Course> _courseRepository;
        private readonly IMasterBaseRepository<Level> _levelRepository;
        private readonly IMasterBaseRepository<Program> _programRepository;

        public ExportGoalOverallQueryHandler(IMasterBaseRepository<StudentProfileReport> studentRepository, IMasterBaseRepository<StudentCompetitionEvent> studentCompetitionEventRepository, IMasterBaseRepository<CourseResult> courseResultRepository, IMasterBaseRepository<LessonResult> lessonResultRepository, IMasterBaseRepository<Course> courseRepository, IMasterBaseRepository<Level> levelRepository, IMasterBaseRepository<Program> programRepository)
        {
            _studentRepository = studentRepository;
            _studentCompetitionEventRepository = studentCompetitionEventRepository;
            _courseResultRepository = courseResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _courseRepository = courseRepository;
            _levelRepository = levelRepository;
            _programRepository = programRepository;
        }

        public async Task<MethodResult<byte[]>> Handle(ExportGoalOverallQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<byte[]>();

            var studentsQuery = from s in _studentRepository.Queryable
                                where s.ProvinceId.HasValue && s.ProvinceId != default
                                   && s.DistrictId.HasValue && s.DistrictId != default
                                   && s.SchoolId.HasValue && s.SchoolId != default
                                select s;

            if (request.ProvinceIds?.Any() == true)
            {
                studentsQuery = studentsQuery.Where(s => s.ProvinceId.HasValue && request.ProvinceIds.Contains(s.ProvinceId.Value));
            }

            if (request.DistrictIds?.Any() == true)
            {
                studentsQuery = studentsQuery.Where(s => s.DistrictId.HasValue && request.DistrictIds.Contains(s.DistrictId.Value));
            }

            if (request.SchoolIds?.Any() == true)
            {
                studentsQuery = studentsQuery.Where(s => s.SchoolId.HasValue && request.SchoolIds.Contains(s.SchoolId.Value));
            }

            var studentCompetitionQuery = from s in studentsQuery
                                          join sce in _studentCompetitionEventRepository.Queryable
                                              on s.StudentId equals sce.StudentId
                                          select new
                                          {
                                              s.StudentId,
                                              s.FullName,
                                              s.Email,
                                              s.Phone,
                                              s.ProvinceId,
                                              s.ProvinceName,
                                              s.DistrictId,
                                              s.DistrictName,
                                              s.SchoolId,
                                              s.SchoolName,
                                              sce.CompetitionEventId
                                          };

            var courseQuery = from sc in studentCompetitionQuery
                              join cr in _courseResultRepository.Queryable on sc.StudentId equals cr.StudentId
                              join c in _courseRepository.Queryable on cr.CourseId equals c.CourseId
                              where cr.WorkingStatus == EnumWorkingStatus.Active && c.SubjectId == request.SubjectId
                              select new { sc.StudentId, sc.FullName, sc.Email, sc.ProvinceName, sc.DistrictName, sc.SchoolName, sc.Phone, cr.CourseResultId, c.LevelId, cr.Percent, c.ProgramId, cr.Status };

            if (request.ProgramIds != null && request.ProgramIds.Count > 0)
            {
                courseQuery = courseQuery.Where(p => request.ProgramIds.Contains(p.ProgramId));
            }

            if (request.LevelIds != null && request.LevelIds.Count > 0)
            {
                courseQuery = courseQuery.Where(p => request.LevelIds.Contains(p.LevelId));
            }

            var dataCourseResults = await courseQuery.ToListAsync(cancellationToken);

            var time = Shared.Helpers.DateTimeHelper.GetWeekBoundaries(request.Date);
            var current = Shared.Helpers.DateTimeHelper.GetWeekBoundaries(DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam));

            var isPastWeek = time.SundayEnd < current.MondayStart;
            var mondayStart = time.MondayStart;
            var sundayEnd = time.SundayEnd;

            var lessonGrouped = _lessonResultRepository.Queryable
                                .Where(l => l.Status == EnumResultStatus.Done
                                            && l.CompletionDate >= mondayStart
                                            && l.CompletionDate <= sundayEnd)
                                .GroupBy(l => l.CourseResultId)
                                .Select(g => new
                                {
                                    CourseResultId = g.Key,
                                    CompletedCount = g.Count()
                                });

            var lessonQuery = from cq in courseQuery
                              join lg in lessonGrouped
                                  on cq.CourseResultId equals lg.CourseResultId into lgJoin
                              from lg in lgJoin.DefaultIfEmpty()
                              let completedCount = (int?)lg.CompletedCount ?? 0
                              select new
                              {
                                  cq.StudentId,
                                  cq.LevelId,
                                  CompletedLessons =
                                      (isPastWeek || cq.Status != EnumResultStatus.Done)
                                      ? completedCount
                                      : 3
                              };

            var progressData = lessonQuery.Select(l => new
            {
                l.StudentId,
                l.LevelId,
                l.CompletedLessons,
                TargetLessons = 3
            });

            var dataLessonResults = await progressData.ToListAsync(cancellationToken);

            var levelIds = dataCourseResults.Select(l => l.LevelId).ToList();
            levelIds.AddRange(dataLessonResults.Select(p => p.LevelId));
            levelIds = levelIds.Distinct().ToList();

            var combinedData = from cr in dataCourseResults
                               join lr in dataLessonResults
                               on new { cr.StudentId, cr.LevelId } equals new { lr.StudentId, lr.LevelId }
                               select new
                               {
                                   cr.LevelId,
                                   cr.FullName,
                                   cr.Email,
                                   cr.Phone,
                                   cr.ProvinceName,
                                   cr.DistrictName,
                                   cr.SchoolName,
                                   cr.StudentId,
                                   lr.CompletedLessons,
                                   lr.TargetLessons,
                                   cr.Percent,
                                   IsStable = (cr.Percent >= 75 && lr.CompletedLessons >= lr.TargetLessons)
                               };

            var levels = await _levelRepository.Queryable.ToListAsync(cancellationToken);
            var programs = await _programRepository.Queryable.ToListAsync(cancellationToken);

            var levelStatistics = combinedData
                                  .Select(g =>
                                  {
                                      var level = levels.FirstOrDefault(p => p.LevelId == g.LevelId);
                                      var program = programs.FirstOrDefault(p => p.ProgramId == level?.ProgramId);
                                      return new
                                      {
                                          FullName = g.FullName,
                                          Email = g.Email,
                                          PhoneNumber = g.Phone,
                                          ProvinceName = g.ProvinceName,
                                          DistrictName = g.DistrictName,
                                          SchoolName = g.SchoolName,
                                          LevelName = level?.LevelCode ?? "Unknown",
                                          GoalProcess = GetGoalProcess(g.CompletedLessons, g.TargetLessons),
                                          GoalResult = GetGoalResult(g.Percent),
                                          IsStable = g.IsStable ? "Ổn" : "Bất ổn"
                                      };
                                  })
                                  .ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var memoryStream = new MemoryStream();          // ✅ bỏ using
            var excelPackage = new ExcelPackage();          // ✅ bỏ using

            var excelWorksheet = excelPackage.Workbook.Worksheets.Add("Students");

            int startRow = 3;

            excelWorksheet.Cells[1, 1].Value = "Tuần học chọn lựa:";
            excelWorksheet.Cells[1, 2].Value = $"{mondayStart.ToString("dd/MM/yyyy")} - {sundayEnd.ToString("dd/MM/yyyy")}";

            int col = 1;

            excelWorksheet.Cells[2, col++].Value = "Họ và tên";
            excelWorksheet.Cells[2, col++].Value = "Email";
            excelWorksheet.Cells[2, col++].Value = "Số điện thoại";
            excelWorksheet.Cells[2, col++].Value = "Tỉnh/ Thành phố";
            excelWorksheet.Cells[2, col++].Value = "Quận/ Huyện";
            excelWorksheet.Cells[2, col++].Value = "Trường";
            excelWorksheet.Cells[2, col++].Value = "Trình độ hiện tại";
            excelWorksheet.Cells[2, col++].Value = "Tiến độ mục tiêu";
            excelWorksheet.Cells[2, col++].Value = "Kết quả mục tiêu";
            excelWorksheet.Cells[2, col++].Value = "Trạng thái hiện tại";

            var results = new object[levelStatistics.Count][];

            Parallel.For(0, levelStatistics.Count, i =>
            {
                var student = levelStatistics[i];
                var rowValues = new object[10];

                rowValues[0] = student.FullName ?? string.Empty;
                rowValues[1] = student.Email ?? string.Empty;
                rowValues[2] = student.PhoneNumber ?? string.Empty;
                rowValues[3] = student.ProvinceName ?? string.Empty;
                rowValues[4] = student.DistrictName ?? string.Empty;
                rowValues[5] = student.SchoolName ?? string.Empty;
                rowValues[6] = student.LevelName ?? string.Empty;
                rowValues[7] = student.GoalProcess ?? string.Empty;
                rowValues[8] = student.GoalResult ?? string.Empty;
                rowValues[9] = student.IsStable ?? string.Empty;

                results[i] = rowValues;
            });

            for (int i = 0; i < results.Length; i++)
            {
                var row = startRow + i;
                var values = results[i];

                for (int c = 0; c < values.Length; c++)
                {
                    excelWorksheet.Cells[row, c + 1].Value = values[c];
                }
            }

            excelWorksheet.Cells.AutoFitColumns();

            excelPackage.SaveAs(memoryStream);

            var bytes = memoryStream.ToArray();

            methodResult.Result = bytes;
            return methodResult;
        }

        public string GetGoalProcess(int completedLessons, int targetLessons)
        {
            if (targetLessons == 0 || completedLessons * 1.0 / targetLessons < 0.5)
            {
                return "Xa mục tiêu";
            }
            else if (targetLessons > 0 && completedLessons * 1.0 / targetLessons >= 0.5 && completedLessons * 1.0 / targetLessons < 1)
            {
                return "Gần mục tiêu";
            }
            else if (targetLessons > 0 && completedLessons == targetLessons)
            {
                return "Đạt mục tiêu";
            }
            else
            {
                return "Vượt mục tiêu";
            }
        }

        public string GetGoalResult(decimal? percent)
        {
            if (!percent.HasValue || percent < 60)
            {
                return "Xa mục tiêu";
            }
            else if (percent.HasValue && percent >= 60 && percent <= 74)
            {
                return "Gần mục tiêu";
            }
            else if (percent.HasValue && percent >= 75 && percent <= 89)
            {
                return "Đạt mục tiêu";
            }
            else
            {
                return "Vượt mục tiêu";
            }
        }
    }
}
