// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.PlacementTestResultQuery
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using static Fsel.Shared.Constants.ValueSettings;

    public class ExportPlacementTestQuery : IRequest<MethodResult<Stream>>
    {
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }

    public class ExportPlacementTestQueryHandler : IRequestHandler<ExportPlacementTestQuery, MethodResult<Stream>>
    {
        private readonly IUserService _userService;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;

        public ExportPlacementTestQueryHandler(
            IUserService userService,
            ICourseResultRepository courseResultRepository,
            IPlacementTestResultRepository placementTestResultRepository)
        {
            _userService = userService;
            _courseResultRepository = courseResultRepository;
            _placementTestResultRepository = placementTestResultRepository;
        }

        public async Task<MethodResult<Stream>> Handle(ExportPlacementTestQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            var students = new List<StudentModel>();
            var studentIds = new List<Guid>();

            var placementTestResultExports = new List<PlacementTestReportExportModel>();
            var placementTestResults = await _placementTestResultRepository.Queryable
                                            .Where(x => x != null && x.UpdatedDate.HasValue && x.UpdatedDate.Value.Date >= request.StartDate.Date && x.UpdatedDate.Value.Date <= request.EndDate.Date)
                                            .GroupBy(x => x.StudentId)
                                            .Select(x => x.OrderByDescending(x => x.UpdatedDate).ThenByDescending(x => x.CreatedDate).FirstOrDefault())
                                            .ToListAsync(cancellationToken);
            if (placementTestResults.Any())
            {
                studentIds = placementTestResults.Select(x => x.StudentId).ToList();
                var studentResults = await _userService.GetStudentsByStudentIdsAsync(studentIds);
                if (!studentResults.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                    return methodResult;
                }
                students = studentResults.Content?.Result?.ToList();
            }

            var placementTestResultGroupResults = await _placementTestResultRepository.Queryable
                                                                                      .WhereBulkContains(studentIds, x => x.StudentId)
                                                                                      .Where(x => x.Status == EnumResultStatus.Done)
                                                                                      .ToListAsync(cancellationToken);

            var placementTestResultGroups = placementTestResultGroupResults.GroupBy(x => x.StudentId)
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
                    SuggetLevel = isLock ? placementTestResultEnd.Status == EnumResultStatus.Done ? levelCompleted.GetCodeByEnumCourseLevel() : EnumCourseLevelHelper.GetCodeByEnumCourseLevel(placementTestResultEnd.Level.GetCourseLevelByPlacementTestLevel()) : null,
                    CurrentLevel = isLock ? currentLevel == EnumCourseLevel.A1 && placementTestResultEnd.Percent < MinCompletePercent ? ValueCourseLevel.PreA1 : EnumCourseLevelHelper.GetCodeByEnumCourseLevel(currentLevel) : null,
                    Percent = isLock ? placementTestResultEnd.Percent : null,
                    IsPTdone = isLock,
                    CourseName = EnumCourseLevelHelper.GetCodeByEnumCourseLevel(courseResult?.Course?.CourseLevel),
                    FinishDate = isLock && placementTestResultEnd.UpdatedDate.HasValue ? placementTestResultEnd.UpdatedDate.Value : null,
                });
            }
            methodResult.Result = placementTestResultExports.OrderBy(x => x.FinishDate).ToList().ExportExcel();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
