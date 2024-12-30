// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CompetitionEventsQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.SystemService.QueryModels;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetReportCompetitionEventsQuery : IRequest<MethodResult<IList<ReportCompetitionEventModel>>>
    {
        public string? EventCodeStr { get; set; }

        public IList<string>? EventCodes
        {
            get
            {
                return EventCodeStr.ToList<string>();
            }
        }

        public EnumEducationLevel EducationLevel { get; set; }
    }

    public class GetReportCompetitionEventsQueryHandler : IRequestHandler<GetReportCompetitionEventsQuery, MethodResult<IList<ReportCompetitionEventModel>>>
    {
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ISystemService _systemService;
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;

        public GetReportCompetitionEventsQueryHandler(ICompetitionEventsRepository competitionEventsRepository,
            IStudentRepository studentRepository,
            ISystemService systemService,
            IStudentCompetitionEventsRepository studentCompetitionEventsRepository)
        {
            _competitionEventsRepository = competitionEventsRepository;
            _studentRepository = studentRepository;
            _systemService = systemService;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
        }

        public async Task<MethodResult<IList<ReportCompetitionEventModel>>> Handle(GetReportCompetitionEventsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<ReportCompetitionEventModel>>();
            var competitions = await _competitionEventsRepository.Queryable.Include(x => x.CompetitionEvents).Where(x => request.EventCodes != null && request.EventCodes.Any(y => y == x.EventCode)).ToListAsync(cancellationToken);
            if (competitions == null || !competitions.Any())
            {
                return methodResult;
            }
            var districtIds = competitions.SelectMany(x => x.CompetitionEvents).Where(x => x.LocationId.HasValue).Select(x => x.LocationId.GetValueOrDefault()).ToList();
            var locationResults = await _systemService.GetLocationByIdsAsync(new GetLocationsByIdsQueryModel { IdsStr = string.Join(",", districtIds) });
            var locationDistricts = locationResults.Content?.Result;

            var schoolIds = competitions.SelectMany(x => x.CompetitionEvents).SelectMany(x => x.SchoolIds ?? new List<Guid>()).ToList();
            var schoolResults = await _systemService.GetSchoolsAsync(new GetListSchoolQueryModel { Ids = schoolIds, EducationLevel = request.EducationLevel });
            var schools = schoolResults.Content?.Result;

            var studentSchoolIds = await (from baseQ in _studentRepository.Queryable
                                          join sce in _studentCompetitionEventsRepository.Queryable on baseQ.Id equals sce.StudentId
                                          where baseQ.SchoolId.HasValue && schools != null && schools.Select(x => x.Id).Contains(baseQ.SchoolId.Value)
                                          && competitions.Select(x => x.Id).Contains(sce.CompetitionEventId)
                                          select new
                                          {
                                              SchoolId = baseQ.SchoolId.GetValueOrDefault(),
                                              StudentId = baseQ.Id
                                          }).ToListAsync(cancellationToken);

            var reportCompetitionEvents = new List<ReportCompetitionEventModel>();
            foreach (var item in competitions.SelectMany(x => x.CompetitionEvents))
            {
                var studentDistricts = studentSchoolIds.Where(x => item.SchoolIds != null && item.SchoolIds.Contains(x.SchoolId)).ToList();
                var studentIds = studentDistricts.Select(x => x.StudentId).Distinct().ToList();
                var reportCompetition = new ReportCompetitionEventModel
                {
                    DistrictName = locationDistricts?.FirstOrDefault(x => x.Id == item.LocationId)?.Name,
                    NumberRegisteredSchool = schools?.Where(x => item.SchoolIds != null && item.SchoolIds.Contains(x.Id)).Count() ?? default,
                    NumberActualParticipatingSchool = studentDistricts.Select(x => x.SchoolId).Distinct().Count(),
                    NumberValidStudentAccount = studentIds.Count,
                    StudentIds = studentIds,
                };

                reportCompetitionEvents.Add(reportCompetition);
            }

            methodResult.Result = reportCompetitionEvents;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
