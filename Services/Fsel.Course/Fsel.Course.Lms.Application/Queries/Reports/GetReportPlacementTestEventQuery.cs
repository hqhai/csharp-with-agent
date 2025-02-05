// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.Reports
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.QueryModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    public class GetReportPlacementTestEventQuery : IRequest<MethodResult<IList<ReportPlacementTestEventModel>>>
    {
        public string? EventCodeStr { get; set; }
        public EnumEducationLevel EducationLevel { get; set; }
    }

    public class GetReportPlacementTestEventQueryHandler : IRequestHandler<GetReportPlacementTestEventQuery, MethodResult<IList<ReportPlacementTestEventModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IServiceProvider _serviceProvider;

        public GetReportPlacementTestEventQueryHandler(
            IMapper mapper,
            IUserService userService,
            IServiceProvider serviceProvider)
        {
            _mapper = mapper;
            _userService = userService;
            _serviceProvider = serviceProvider;
        }

        public async Task<MethodResult<IList<ReportPlacementTestEventModel>>> Handle(GetReportPlacementTestEventQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<ReportPlacementTestEventModel>>();
            var reportCompetitionEventResults = await _userService.GetReportCompetitionEventAsync(new GetReportCompetitionEventQueryModel
            {
                EducationLevel = request.EducationLevel,
                EventCodeStr = request.EventCodeStr,
            });

            var reportCompetitionEvents = reportCompetitionEventResults?.Content?.Result;
            if (reportCompetitionEvents == null)
            {
                return methodResult;
            }
            var reportPlacementTestEvents = new ConcurrentBag<ReportPlacementTestEventModel>();
            var placementTestResultGroups = new ConcurrentStack<PlacementTestResultReportGroupModel>();

            var studentIds = reportCompetitionEvents.Where(x => x.StudentIds != null && x.StudentIds.Any()).SelectMany(x => x.StudentIds ?? new List<Guid>()).ToList();

            // Chia danh sách thành từng nhóm
            var batches = studentIds
                .Select((id, index) => new { id, index })
                .GroupBy(x => x.index / ValueSettings.BatchSize)
                .Select(g => g.Select(x => x.id).ToList())
                .ToList();

            // Thực hiện truy vấn từng nhóm
            await Parallel.ForEachAsync(batches, async (batche, cancellationToken) =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var placementTestGroupResultRepository = scope.ServiceProvider.GetRequiredService<IPlacementTestGroupResultRepository>();
                    var placementTestGroups = await placementTestGroupResultRepository.Queryable
                                                    .Where(x => batche.Contains(x.StudentId))
                                                    .Select(x => new PlacementTestResultReportGroupModel
                                                    {
                                                        CourseLevel = x.SuggetLevel,
                                                        StudentId = x.StudentId,
                                                        IsDonePT = x.Status == EnumResultStatus.Done
                                                    })
                                                    .ToListAsync(cancellationToken);
                    placementTestResultGroups.PushRange(placementTestGroups.ToArray());
                }
            });

            Parallel.ForEach(reportCompetitionEvents, reportCompetitionEvent =>
            {
                var placementTestResultReports = placementTestResultGroups.Where(x => reportCompetitionEvent.StudentIds != null && reportCompetitionEvent.StudentIds.Contains(x.StudentId)).ToList();
                var reportPlacementTestEvent = new ReportPlacementTestEventModel
                {
                    LocationName = reportCompetitionEvent.DistrictName,
                    NumberRegisteredSchool = reportCompetitionEvent.NumberRegisteredSchool,
                    NumberActualParticipatingSchool = reportCompetitionEvent.NumberActualParticipatingSchool,
                    NumberValidStudentAccount = reportCompetitionEvent.NumberValidStudentAccount,
                    NumberStudentsCompletedPT = placementTestResultReports?.Where(x => x.IsDonePT).Count() ?? default,
                    ReportCourseLevels = EnumCourseLevelHelper.GetEnumCourseLevels(EnumCourseType.Academic).Select(courseLevel =>
                    {
                        var numberStudentOfLevel = placementTestResultReports?.Where(x => x.IsDonePT && x.CourseLevel == courseLevel).Count() ?? default;
                        return new ReportCourseLevelModel
                        {
                            CourseLevel = courseLevel,
                            TotalStudent = numberStudentOfLevel,
                            Percent = NumberHelper.GetPercent(numberStudentOfLevel, placementTestResultReports?.Where(x => x.IsDonePT).Count() ?? default)
                        };
                    }).ToList()
                };
                reportPlacementTestEvents.Add(reportPlacementTestEvent);
            });
            methodResult.Result = reportPlacementTestEvents.ToList();
            return methodResult;
        }
    }
}
