// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.PlacementTestResultQuery
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using static Fsel.Shared.Constants.ValueSettings;

    public class ExportPlacementTestByStudentsQuery : BaseImportCommandModel, IRequest<MethodResult<Stream>>
    {
    }

    public class ExportPlacementTestByStudentsQueryHandler : IRequestHandler<ExportPlacementTestByStudentsQuery, MethodResult<Stream>>
    {
        private readonly IUserService _userService;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;

        public ExportPlacementTestByStudentsQueryHandler(
            IUserService userService,
            ICourseResultRepository courseResultRepository,
            IPlacementTestResultRepository placementTestResultRepository)
        {
            _userService = userService;
            _courseResultRepository = courseResultRepository;
            _placementTestResultRepository = placementTestResultRepository;
        }

        public async Task<MethodResult<Stream>> Handle(ExportPlacementTestByStudentsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();
            if (request.FormFile == null)
            {
                return methodResult;
            }
            var placementTestResultExports = new List<PlacementTestReportExportModel>();

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

            var listEmail = result.Datas.Where(x => !string.IsNullOrEmpty(x.Email)).Select(x => x.Email!.ToLower().Trim()).ToList();
            var studentResultToEmail = await _userService.GetStudentByEmailsAsync(listEmail);
            if (!studentResultToEmail.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                return methodResult;
            }

            var students = studentResultToEmail.Content?.Result?.ToList();
            var studentIds = students?.Select(x => x.Id).ToList() ?? new List<Guid>();
            var placementTestResults = await _placementTestResultRepository.Queryable
                                            .Where(x => x.Status == EnumResultStatus.Done)
                                            .WhereBulkContains(studentIds, x => (x.StudentId))
                                            .ToListAsync(cancellationToken);

            var placementTestResultGroups = placementTestResults.GroupBy(x => x.StudentId)
                                            .Select(x => new
                                            {
                                                StudentId = x.Key,
                                                PlacementTestStart = x.Select(x => x).OrderBy(x => x.CreatedDate).FirstOrDefault(),
                                                PlacementTestEnd = x.Select(x => x).OrderByDescending(x => x.CreatedDate).FirstOrDefault(),
                                            })
                                            .ToList();

            var courseResults = await _courseResultRepository.Queryable
                                                             .Include(x => x.Course)
                                                             .WhereBulkContains(studentIds, x => x.StudentId)
                                                             .Where(x => x.WorkingStatus == EnumWorkingStatus.Active)
                                                             .ToListAsync(cancellationToken);
            foreach (var item in placementTestResultGroups)
            {
                var student = students?.FirstOrDefault(x => x.Id == item.StudentId);
                var placementTestResultEnd = item.PlacementTestEnd;
                var placementTestResultStart = item.PlacementTestStart;
                if (student == null)
                {
                    continue;
                }

                if (item == null || placementTestResultEnd == null)
                {
                    placementTestResultExports.Add(new PlacementTestReportExportModel
                    {
                        Name = student.User?.FullName,
                        Birthday = student.User?.Birthday,
                        Email = student.User?.Email,
                    });
                    continue;
                }

                int age = Shared.Helpers.DateTimeHelper.GetYearOld(student.User?.Birthday);
                var (levelCompleted, isLock) = placementTestResultEnd.Level.GetLevelInScore(placementTestResultEnd.Percent, IeltsScoreHelper.GetInitialAge(placementTestResultStart?.Level, age));
                var courseResult = courseResults.FirstOrDefault(x => x.StudentId == item.StudentId);
                var currentLevel = SendMailHelper.GetPreviousEnumValue(levelCompleted ?? default);
                placementTestResultExports.Add(new PlacementTestReportExportModel
                {
                    Name = student.User?.FullName,
                    Birthday = student.User?.Birthday,
                    Email = student.User?.Email,
                    CompletionLevel = isLock ? EnumCourseLevelHelper.GetCodeByEnumCourseLevel(placementTestResultEnd.Level.GetCourseLevelByPlacementTestLevel()) : null,
                    ChooseLevel = isLock ? student.CourseLevel.GetCodeByEnumCourseLevel() : null,
                    SuggetLevel = isLock ? placementTestResultEnd.Status == EnumResultStatus.Done ? EnumCourseLevelHelper.GetCodeByEnumCourseLevel(levelCompleted) : EnumCourseLevelHelper.GetCodeByEnumCourseLevel(placementTestResultEnd.Level.GetCourseLevelByPlacementTestLevel()) : null,
                    CurrentLevel = isLock ? currentLevel == EnumCourseLevel.A1 && placementTestResultEnd.Percent < MinCompletePercent ? ValueCourseLevel.PreA1 : EnumCourseLevelHelper.GetCodeByEnumCourseLevel(currentLevel) : null,
                    Percent = isLock ? placementTestResultEnd.Percent : null,
                    IsPTdone = isLock,
                    CourseName = EnumCourseLevelHelper.GetCodeByEnumCourseLevel(courseResult?.Course?.CourseLevel),
                    FinishDate = isLock && placementTestResultEnd.UpdatedDate.HasValue ? placementTestResultEnd.UpdatedDate.Value : null,
                });
            }
            methodResult.Result = placementTestResultExports.OrderBy(x => listEmail.IndexOf(x.Email?.ToLower(System.Globalization.CultureInfo.CurrentCulture) ?? string.Empty)).ToList().ExportExcel();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
