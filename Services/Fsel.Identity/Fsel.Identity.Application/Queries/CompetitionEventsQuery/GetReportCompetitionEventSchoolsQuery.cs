// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CompetitionEventsQuery
{
    using System.Linq;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.SystemService.QueryModels;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Infrastructure.Repositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetReportCompetitionEventSchoolsQuery : IRequest<MethodResult<IList<ReportCompetitionEventModel>>>
    {
        public string? EventCodeStr { get; set; }
        public string? DistrictName { get; set; }

        public IList<string>? EventCodes
        {
            get
            {
                return EventCodeStr.ToList<string>();
            }
        }

        public EnumEducationLevel EducationLevel { get; set; }
    }

    public class GetReportCompetitionEventSchoolsQueryHandler : IRequestHandler<GetReportCompetitionEventSchoolsQuery, MethodResult<IList<ReportCompetitionEventModel>>>
    {
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ISystemService _systemService;
        private readonly IEventRegistrationRepository _eventRegistrationRepository;
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;
        private readonly UserManager<User> _userManager;
        private readonly IHumanRepository _humanRepository;

        public GetReportCompetitionEventSchoolsQueryHandler(ICompetitionEventsRepository competitionEventsRepository,
            IStudentRepository studentRepository,
            ISystemService systemService,
            IEventRegistrationRepository eventRegistrationRepository,
            IStudentCompetitionEventsRepository studentCompetitionEventsRepository,
            UserManager<User> userManager,
            IHumanRepository humanRepository)
        {
            _competitionEventsRepository = competitionEventsRepository;
            _studentRepository = studentRepository;
            _systemService = systemService;
            _eventRegistrationRepository = eventRegistrationRepository;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
            _userManager = userManager;
            _humanRepository = humanRepository;
        }

        public async Task<MethodResult<IList<ReportCompetitionEventModel>>> Handle(GetReportCompetitionEventSchoolsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<ReportCompetitionEventModel>>();
            var competitions = await _competitionEventsRepository.Queryable.Include(x => x.CompetitionEvents).Where(x => request.EventCodes != null && request.EventCodes.Any(y => y == x.EventCode)).ToListAsync(cancellationToken);
            if (competitions == null || !competitions.Any())
            {
                return methodResult;
            }
            var competitionEvents = competitions.SelectMany(x => x.CompetitionEvents).ToList();

            var districtIds = competitionEvents.Where(x => x.LocationId.HasValue).Select(x => x.LocationId.GetValueOrDefault()).ToList();
            var locationResults = await _systemService.GetLocationByIdsAsync(new GetLocationsByIdsQueryModel { IdsStr = string.Join(",", districtIds) });
            var locationDistricts = locationResults.Content?.Result;

            if (!string.IsNullOrEmpty(request.DistrictName))
            {
                request.DistrictName = request.DistrictName.ToLower(System.Globalization.CultureInfo.CurrentCulture).Trim();
                var district = locationDistricts?.FirstOrDefault(x => x.Name.ToLower().Trim() == request.DistrictName);
                competitionEvents = competitionEvents.Where(x => district != null && x.LocationId == district.Id).ToList();
            }

            var schoolIds = competitionEvents.SelectMany(x => x.SchoolIds ?? new List<Guid>()).ToList();
            var schoolResults = await _systemService.GetSchoolsAsync(new GetListSchoolQueryModel { Ids = schoolIds, EducationLevel = request.EducationLevel });
            var schools = schoolResults.Content?.Result;

            var studentSchoolIds = await (from baseQ in _studentRepository.Queryable
                                          join er in _eventRegistrationRepository.Queryable on baseQ.Id equals er.StudentId
                                          join sce in _studentCompetitionEventsRepository.Queryable on baseQ.Id equals sce.StudentId
                                          join human in _humanRepository.Queryable on baseQ.HumanId equals human.Id
                                          join user in _userManager.Users on human.UserId equals user.Id
                                          where schools != null && baseQ.SchoolId.HasValue && schools.Select(x => x.Id).Contains(baseQ.SchoolId.Value)
                                          && competitions.Select(x => x.Id).Contains(er.CompetitionEventId)
                                          group new { baseQ, human, user }
                                          by baseQ.SchoolId into g
                                          select new
                                          {
                                              SchoolId = g.Key.GetValueOrDefault(),
                                              StudentIds = g.Select(x => x.baseQ.Id).Distinct().ToList(),
                                              UserIds = g.Where(x => x.human.UserId.HasValue).Select(x => x.human.UserId.GetValueOrDefault()).Distinct().ToList(),
                                              NumberStudentVerifiedSchool = g.Select(x => x.user).Where(x => x.Status == EnumUserStatus.Active).Distinct().Count(),
                                          }).ToListAsync(cancellationToken);

            var reportCompetitionEvents = new List<ReportCompetitionEventModel>();
            foreach (var item in competitionEvents)
            {
                var studentDistricts = studentSchoolIds.Where(x => item.SchoolIds != null && item.SchoolIds.Contains(x.SchoolId)).ToList();
                var studentIds = studentDistricts.SelectMany(x => x.StudentIds).Distinct().ToList();
                var schoolDistricts = schools?.Where(x => item.SchoolIds != null && item.SchoolIds.Contains(x.Id));

                var reportCompetition = new ReportCompetitionEventModel
                {
                    DistrictName = locationDistricts?.FirstOrDefault(x => x.Id == item.LocationId)?.Name,
                    NumberRegisteredSchool = schoolDistricts?.Count() ?? default,
                    NumberActualParticipatingSchool = studentDistricts.Select(x => x.SchoolId).Distinct().Count(),
                    NumberValidStudentAccount = studentIds.Count,
                    NumberStudentVerifiedDistrict = studentDistricts.Sum(x => x.NumberStudentVerifiedSchool),
                    StudentIds = studentIds,
                    UserIds = studentDistricts.SelectMany(x => x.UserIds).Distinct().ToList(),
                    ReportCompetitionEventSchools = schoolDistricts?.Select(school =>
                    {
                        var schoolDistrict = studentDistricts.FirstOrDefault(x => x.SchoolId == school.Id);
                        return new ReportCompetitionEventSchoolModel
                        {
                            SchoolName = schools?.FirstOrDefault(x => x.Id == school.Id)?.Name,
                            NumberValidStudentAccount = schoolDistrict?.StudentIds.Count ?? default,
                            NumberStudentVerifiedSchool = schoolDistrict?.NumberStudentVerifiedSchool ?? default,
                            StudentIds = schoolDistrict?.StudentIds ?? new List<Guid>(),
                            UserIds = schoolDistrict?.UserIds ?? new List<Guid>(),
                        };
                    }).ToList() ?? new List<ReportCompetitionEventSchoolModel>()
                };

                reportCompetitionEvents.Add(reportCompetition);
            }

            methodResult.Result = reportCompetitionEvents;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
