// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.PlacementTestResultQuery
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

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
        private readonly IPlacementTestResultRepository _placementTestResultRepository;

        public ExportPlacementTestQueryHandler(
            IUserService userService,
            IPlacementTestResultRepository placementTestResultRepository)
        {
            _userService = userService;
            _placementTestResultRepository = placementTestResultRepository;
        }

        public async Task<MethodResult<Stream>> Handle(ExportPlacementTestQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();
            var placementTestResultExports = new List<PlacementTestResultExportModel>();
            var placementTestResults = await _placementTestResultRepository.Queryable
                .Where(x => x.CreatedDate.Date >= request.StartDate.Date && x.CreatedDate.Date <= request.EndDate.Date)
                .GroupBy(x => x.StudentId)
                .Select(x => x.OrderBy(x => x.CreatedDate).FirstOrDefault())
                .ToListAsync(cancellationToken);

            foreach (var item in placementTestResults)
            {
                if (item != null)
                {
                    var studentResult = await _userService.GetStudentByUserIdAsync(item.CreatedUserId);
                    var student = studentResult.Content?.Result;
                    if (student != null)
                    {
                        int age = Shared.Helpers.DateTimeHelper.GetYearOld(student.Human?.Birthday);
                        var (levelNext, isLock) = item.Level.GetLevelInScore(item.Percent, age);
                        placementTestResultExports.Add(new PlacementTestResultExportModel
                        {
                            Level = student.CourseLevel,
                            LevelDone = levelNext ?? default,
                            Name = student.Human?.FullName
                        });
                    }
                }
            }
            methodResult.Result = placementTestResultExports.OrderBy(x => x.Name).ToList().ExportExcel();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
