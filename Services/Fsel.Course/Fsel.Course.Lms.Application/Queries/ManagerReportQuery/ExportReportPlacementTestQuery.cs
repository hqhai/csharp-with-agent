// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManagerReportQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.PlacementTestResultQuery;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ExportReportPlacementTestQuery : BaseImportCommandModel, IRequest<MethodResult<Stream>>
    {
        public Guid SchoolId { get; set; }
    }

    public class ExportPlacementTestByStudentsQueryHandler : IRequestHandler<ExportPlacementTestByStudentsQuery, MethodResult<Stream>>
    {
        private readonly IUserService _userService;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly ISystemService _systemService;

        public ExportPlacementTestByStudentsQueryHandler(
            IUserService userService,
            ICourseResultRepository courseResultRepository,
            IPlacementTestResultRepository placementTestResultRepository,
            ISystemService systemService)
        {
            _userService = userService;
            _courseResultRepository = courseResultRepository;
            _placementTestResultRepository = placementTestResultRepository;
            _systemService = systemService;
        }

        public async Task<MethodResult<Stream>> Handle(ExportPlacementTestByStudentsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();
            if (request.FormFile == null)
            {
                return methodResult;
            }

            var placementTestResultExports = new List<PlacementTestResultExportModel>();
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

            var listEmail = result.Datas.Where(x => !string.IsNullOrEmpty(x.Email)).Select(x => x.Email!).Distinct().ToList();
            var studentResultToEmail = await _userService.GetStudentByEmailsAsync(listEmail);
            if (!studentResultToEmail.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResultToEmail.Error);
                return methodResult;
            }

            var students = studentResultToEmail.Content?.Result?.ToList();
            if (students == null || students.Count == 0)
            {
                return methodResult;
            }
            var schoolIds = students.Where(x => x.SchoolId.HasValue).Select(x => x.SchoolId.GetValueOrDefault()).ToList();
            var schoolResults = await _systemService.GetSchoolsAsync(schoolIds);
            if (!schoolResults.IsSuccessStatusCode)
            {
                methodResult.AddError(schoolResults.Error);
                return methodResult;
            }

            var schools = schoolResults.Content?.Result;
            var studentIds = students.Select(x => x.Id).ToList();

            var placementTestResults = await _placementTestResultRepository.Queryable.Where(x => studentIds.Contains(x.StudentId))
                                             .GroupBy(x => x.StudentId)
                                             .Select(g => new
                                             {
                                                 FirstResult = g.OrderBy(x => x.CreatedDate).FirstOrDefault(),
                                                 LastResult = g.OrderByDescending(x => x.UpdatedDate).ThenByDescending(x => x.CreatedDate).FirstOrDefault()
                                             })
                                             .ToListAsync(cancellationToken);
            var placementTestResultFirsts = placementTestResults.Select(x => x.FirstResult).ToList();
            var placementTestResultLasts = placementTestResults.Select(x => x.LastResult).ToList();

            var courseResults = await _courseResultRepository.Queryable.Include(x => x.Course)
                                     .Where(x => studentIds.Contains(x.StudentId) && x.WorkingStatus == EnumWorkingStatus.Active)
                                     .ToListAsync(cancellationToken);
            foreach (var item in students)
            {
                if (item == null)
                {
                    continue;
                }
                var courseResult = courseResults.FirstOrDefault(x => x.StudentId == item.Id);
                var placementTestResultExport = new PlacementTestResultExportModel
                {
                    Name = item.Human?.FullName,
                    Birthday = item.Human?.Birthday,
                    PhoneNumber = item.Human?.PhoneNumber,
                    SchoolClass = item.SchoolClass,
                    SchoolGrade = item.SchoolGrade,
                    Email = item.Human?.Email,
                    SchoolName = item.School ?? schools?.FirstOrDefault(x => x.Id == item.SchoolId)?.Name,
                    CourseName = courseResult?.Course?.Name,
                };

                var placementTestResultLast = placementTestResultLasts.FirstOrDefault(x => x.StudentId == item.Id);
                if (placementTestResultLast != null)
                {
                    var placementTestResultFirst = placementTestResultFirsts.FirstOrDefault(x => x.StudentId == item.Id);
                    int age = Shared.Helpers.DateTimeHelper.GetYearOld(item.Human?.Birthday);
                    var (levelCompleted, isLock) = placementTestResultLast.Level.GetLevelInScore(placementTestResultLast.Percent, IeltsScoreHelper.GetInitialAge(placementTestResultFirst?.Level, age));

                    placementTestResultExport.CourseLevel = placementTestResultLast.Level.GetCourseLevelByPlacementTestLevel();
                    placementTestResultExport.CurrentLevel = isLock ? item.CourseLevel : null;
                    placementTestResultExport.LevelCompleted = placementTestResultLast.Status == EnumResultStatus.Done ? levelCompleted : placementTestResultExport.CourseLevel;
                    placementTestResultExport.Percent = placementTestResultLast.Percent;
                    placementTestResultExport.IsPTdone = isLock;
                    placementTestResultExport.UpdatedDate = placementTestResultLast.UpdatedDate.HasValue ? placementTestResultLast.UpdatedDate.Value : null;
                }
                placementTestResultExports.Add(placementTestResultExport);
            }
            methodResult.Result = placementTestResultExports.OrderBy(x => x.UpdatedDate).ToList().ExportExcel();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
