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
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums.ErrorCodes;

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

            var placementTestResultExports = new List<PlacementTestResultExportModel>();
            var placementTestResults = await _placementTestResultRepository.Queryable
                                            .GroupBy(x => x.StudentId)
                                            .Select(x => x.OrderByDescending(x => x.UpdatedDate).ThenByDescending(x => x.CreatedDate).FirstOrDefault())
                                            .ToListAsync(cancellationToken);

            placementTestResults = placementTestResults.Where(x => x != null && x.UpdatedDate.HasValue && x.UpdatedDate.Value.Date >= request.StartDate.Date && x.UpdatedDate.Value.Date <= request.EndDate.Date).ToList();
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

            var courseResults = await _courseResultRepository.Queryable.Include(x => x.Course).Where(x => studentIds.Contains(x.StudentId) && x.WorkingStatus == EnumWorkingStatus.Active).ToListAsync(cancellationToken);
            foreach (var item in placementTestResults)
            {
                if (item != null)
                {
                    var student = students?.FirstOrDefault(x => x.Id == item.StudentId);
                    if (student != null)
                    {
                        var placementTestResult = await _placementTestResultRepository.Queryable.Where(x => x.StudentId == item.StudentId).OrderBy(x => x.CreatedDate).FirstOrDefaultAsync(cancellationToken);
                        int age = Shared.Helpers.DateTimeHelper.GetYearOld(student.User?.Birthday);
                        var (levelCompleted, isLock) = item.Level.GetLevelInScore(item.Percent, IeltsScoreHelper.GetInitialAge(placementTestResult?.Level, age));
                        var courseResult = courseResults.FirstOrDefault(x => x.StudentId == item.StudentId);
                        placementTestResultExports.Add(new PlacementTestResultExportModel
                        {
                            Name = student.User?.FullName,
                            Birthday = student.User?.Birthday,
                            Email = student.User?.Email,
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
