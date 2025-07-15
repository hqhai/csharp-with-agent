// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CompetitionEventsQuery
{
    using System.Linq;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.SystemService.Model;
    using Fsel.Identity.Application.Services.SystemService.QueryModels;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
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

        public EnumEducationLevel? EducationLevel { get; set; }
    }

    public class GetReportCompetitionEventSchoolsQueryHandler : IRequestHandler<GetReportCompetitionEventSchoolsQuery, MethodResult<IList<ReportCompetitionEventModel>>>
    {
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ISystemService _systemService;
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;
        private readonly IHumanRepository _humanRepository;
        private readonly UserManager<User> _userManager;

        public GetReportCompetitionEventSchoolsQueryHandler(ICompetitionEventsRepository competitionEventsRepository,
            IStudentRepository studentRepository,
            ISystemService systemService,
            IStudentCompetitionEventsRepository studentCompetitionEventsRepository,
            IHumanRepository humanRepository,
            UserManager<User> userManager)
        {
            _competitionEventsRepository = competitionEventsRepository;
            _studentRepository = studentRepository;
            _systemService = systemService;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
            _humanRepository = humanRepository;
            _userManager = userManager;
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
            var competitionEvents = await GetCompetitionEventsAsync(competitions);
            competitionEvents = competitionEvents.GroupBy(x => x.Id).Select(x => x.First()).ToList();

            var districtIds = competitionEvents.Where(x => x.LocationId.HasValue).Select(x => x.LocationId.GetValueOrDefault()).ToList();
            var locationResults = await _systemService.GetLocationByIdsAsync(new GetLocationsByIdsQueryModel { IdsStr = string.Join(",", districtIds) });
            var locationDistricts = locationResults.Content?.Result;

            if (!string.IsNullOrEmpty(request.DistrictName))
            {
                request.DistrictName = request.DistrictName.ToLower(System.Globalization.CultureInfo.CurrentCulture).Trim();
                var district = locationDistricts?.FirstOrDefault(x => x.Name != null && x.Name.ToLower(System.Globalization.CultureInfo.CurrentCulture).Trim() == request.DistrictName);
                competitionEvents = competitionEvents.Where(x => district != null && x.LocationId == district.Id).ToList();
            }
            var competitionEventIds = competitionEvents.Select(x => x.Id).ToList();
            competitionEventIds.AddRange(competitions.Select(x => x.Id));

            var schoolIds = competitionEvents.SelectMany(x => x.SchoolIds ?? new List<Guid>()).ToList();
            var schoolResults = await _systemService.GetSchoolsAsync(new GetListSchoolQueryModel { Ids = schoolIds, EducationLevel = request.EducationLevel });
            var schools = schoolResults.Content?.Result ?? new List<SchoolModel>();

            var studentSchoolIds = (await (from baseQ in _studentRepository.Queryable.Where(x => x.SchoolId != null)
                                           join sce in _studentCompetitionEventsRepository.Queryable on baseQ.Id equals sce.StudentId
                                           join human in _humanRepository.Queryable on baseQ.HumanId equals human.Id
                                           join user in _userManager.Users on human.UserId equals user.Id
                                           where competitionEventIds.Contains(sce.CompetitionEventId) && !baseQ.IsDeleted && !user.IsDeleted
                                           select new
                                           {
                                               baseQ.SchoolId,
                                               StudentId = baseQ.Id,
                                               UserId = user.Id,
                                               user.EmailConfirmed,
                                               user.PhoneNumberConfirmed
                                           }).ToListAsync(cancellationToken)).GroupBy(x => x.SchoolId.Value)
                                            .Select(g => new
                                            {
                                                SchoolId = g.Key,
                                                StudentIds = g.Select(x => x.StudentId).Distinct().ToList(),
                                                UserIds = g.Select(x => x.UserId).Distinct().ToList(),
                                                CountCompleteVerify = g.Where(x => x.EmailConfirmed || x.PhoneNumberConfirmed).Select(x => x.UserId).Distinct().Count()
                                            }).ToList();

            var reportCompetitionEvents = new List<ReportCompetitionEventModel>();
            foreach (var item in competitionEvents)
            {
                var studentDistricts = studentSchoolIds.Where(x => item.SchoolIds != null && item.SchoolIds.Contains(x.SchoolId)).ToList();
                var studentIds = studentDistricts.SelectMany(x => x.StudentIds).Distinct().ToList();
                var schoolDistricts = schools.Where(x => item.SchoolIds != null && item.SchoolIds.Contains(x.Id));

                var reportCompetition = new ReportCompetitionEventModel
                {
                    LocationId = item.Id,
                    DistrictName = locationDistricts?.FirstOrDefault(x => x.Id == item.LocationId)?.Name,
                    NumberRegisteredSchool = schoolDistricts?.Count() ?? default,
                    NumberActualParticipatingSchool = studentDistricts.Select(x => x.SchoolId).Distinct().Count(),
                    NumberValidStudentAccount = studentIds.Count,
                    NumberStudentCompleteVerify = studentDistricts.Sum(x => x.CountCompleteVerify),
                    StudentIds = studentIds,
                    UserIds = studentDistricts.SelectMany(x => x.UserIds).Distinct().ToList(),
                    ReportCompetitionEventSchools = schoolDistricts?.Select(school =>
                    {
                        var schoolDistrict = studentDistricts.FirstOrDefault(x => x.SchoolId == school.Id);
                        return new ReportCompetitionEventSchoolModel
                        {
                            SchoolId = school.Id,
                            SchoolName = schools?.FirstOrDefault(x => x.Id == school.Id)?.Name,
                            NumberValidStudentAccount = schoolDistrict?.StudentIds.Count ?? default,
                            NumberStudentCompleteVerify = schoolDistrict?.CountCompleteVerify ?? default,
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

        private async Task<List<CompetitionEvent>> GetCompetitionEventsAsync(List<CompetitionEvent> competitionEvents)
        {
            // Lấy tất cả các sự kiện con của competitionEvents
            var subEvents = await _competitionEventsRepository.Queryable
                .Where(x => x.ParentEventId.HasValue && competitionEvents.Select(e => e.Id).Contains(x.ParentEventId.Value))
                .ToListAsync();

            // Lọc ra các sự kiện từ competitionEvents mà không có con (nút lá)
            var leafNodes = competitionEvents.Where(e => !subEvents.Any(se => se.ParentEventId == e.Id)).ToList();

            // Nếu không có con thì trả về các nút lá (điểm cuối)
            if (!subEvents.Any())
            {
                return leafNodes;
            }

            // Gọi đệ quy để lấy tiếp các điểm cuối từ danh sách con
            var childLeafNodes = await GetCompetitionEventsAsync(subEvents);

            return leafNodes.Concat(childLeafNodes).ToList();
        }
    }
}
