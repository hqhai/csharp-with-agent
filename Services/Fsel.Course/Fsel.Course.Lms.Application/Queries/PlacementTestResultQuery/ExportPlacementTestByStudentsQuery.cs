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
            var placementTestResultExports = new List<PlacementTestResultExportModel>();

            var result = request.FormFile.ImportAndValidateExcel(async (ImportStudentEmailModel x, IList<ImportStudentEmailModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                if (string.IsNullOrEmpty(x.Email) || !x.Email.IsValidEmail())
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = "Email is null or malformed" });
                }
                return await Task.FromResult(errors.Count == 0);
            });
            var listEmail = result.Datas.Where(x => !string.IsNullOrEmpty(x.Email)).Select(x => x.Email!).ToList();
            var studentResultToEmail = await _userService.GetStudentByEmailsAsync(listEmail);
            if (!studentResultToEmail.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                return methodResult;
            }

            var students = studentResultToEmail.Content?.Result?.ToList();
            var studentIds = students?.Select(x => x.Id).ToList() ?? new List<Guid>();
            var placementTestResults = await _placementTestResultRepository.Queryable
                                            .Where(x => studentIds.Contains(x.StudentId))
                                            .GroupBy(x => x.StudentId)
                                            .Select(x => x.OrderByDescending(x => x.UpdatedDate).ThenByDescending(x => x.CreatedDate).FirstOrDefault())
                                            .ToListAsync(cancellationToken);

            var courseResults = await _courseResultRepository.Queryable.Include(x => x.Course).Where(x => studentIds.Contains(x.StudentId) && x.WorkingStatus == EnumWorkingStatus.Active).ToListAsync(cancellationToken);
            foreach (var item in placementTestResults)
            {
                if (item != null)
                {
                    var student = students?.FirstOrDefault(x => x.Id == item.StudentId);
                    if (student != null)
                    {
                        var placementTestResult = await _placementTestResultRepository.Queryable.Where(x => x.StudentId == item.StudentId).OrderBy(x => x.CreatedDate).FirstOrDefaultAsync(cancellationToken);
                        int age = Shared.Helpers.DateTimeHelper.GetYearOld(student.Human?.Birthday);
                        var (levelCompleted, isLock) = item.Level.GetLevelInScore(item.Percent, IeltsScoreHelper.GetInitialAge(placementTestResult?.Level, age));
                        var courseResult = courseResults.FirstOrDefault(x => x.StudentId == item.StudentId);
                        placementTestResultExports.Add(new PlacementTestResultExportModel
                        {
                            Name = student.Human?.FullName,
                            Birthday = student.Human?.Birthday,
                            Email = student.Human?.Email,
                            CourseLevel = item.Level.GetCourseLevelByPlacementTestLevel(),
                            CurrentLevel = isLock ? student.CourseLevel : null,
                            LevelCompleted = item.Status == EnumResultStatus.Done ? levelCompleted : item.Level.GetCourseLevelByPlacementTestLevel(),
                            Percent = item.Percent,
                            IsPTdone = isLock,
                            CourseName = courseResult?.Course?.Name,
                            UpdatedDate = item.UpdatedDate.HasValue ? item.UpdatedDate.Value : null,
                        });
                    }
                }
            }
            methodResult.Result = placementTestResultExports.OrderBy(x => x.UpdatedDate).ToList().ExportExcel();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
