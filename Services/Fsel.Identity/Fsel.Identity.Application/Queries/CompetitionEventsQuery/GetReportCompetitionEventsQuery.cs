// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CompetitionEventsQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.SystemService.QueryModels;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetReportCompetitionEventsQuery : IRequest<MethodResult<IList<ReportCompetitionEventModel>>>
    {
        public string? EventCode { get; set; }
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
            var competition = await _competitionEventsRepository.Queryable.Include(x => x.CompetitionEvents).ThenInclude(x => x.CompetitionEvents).FirstOrDefaultAsync(x => x.EventCode == request.EventCode, cancellationToken);
            if (competition == null)
            {
                return methodResult;
            }
            var districtIds = competition.CompetitionEvents.SelectMany(x => x.CompetitionEvents).Where(x => x.LocationId.HasValue).Select(x => x.LocationId.GetValueOrDefault()).ToList();
            var locationResults = await _systemService.GetLocationByIdsAsync(new GetLocationsByIdsQueryModel { IdsStr = string.Join(",", districtIds) });
            var locationDistricts = locationResults.Content?.Result;

            var schoolIds = competition.CompetitionEvents.SelectMany(x => x.CompetitionEvents).SelectMany(x => x.SchoolIds ?? new List<Guid>()).Distinct().ToList();
            var schoolResults = await _systemService.GetSchoolsAsync(new GetListSchoolQueryModel { SchoolIds = schoolIds, EducationLevel = request.EducationLevel });
            var schools = schoolResults.Content?.Result;

            //var studentSchoolIds = await _studentRepository.Queryable
            //                                               .Where(x => x.SchoolId.HasValue && schools != null && schools.Select(x => x.Id).Contains(x.SchoolId.Value))
            //                                               .Select(x => new
            //                                               {
            //                                                   SchoolId = x.SchoolId.GetValueOrDefault(),
            //                                                   StudentId = x.Id
            //                                               }).ToListAsync(cancellationToken);

            var studentSchoolIds = await (from baseQ in _studentRepository.Queryable
                                          join sce in _studentCompetitionEventsRepository.Queryable on baseQ.Id equals sce.StudentId
                                          where baseQ.SchoolId.HasValue && schools != null && schools.Select(x => x.Id).Contains(baseQ.SchoolId.Value)
                                          && sce.CompetitionEventId == competition.Id
                                          select new
                                          {
                                              SchoolId = baseQ.SchoolId.GetValueOrDefault(),
                                              StudentId = baseQ.Id
                                          }).ToListAsync(cancellationToken);

            var reportCompetitionEvents = new List<ReportCompetitionEventModel>();
            foreach (var item in competition.CompetitionEvents.SelectMany(x => x.CompetitionEvents))
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
