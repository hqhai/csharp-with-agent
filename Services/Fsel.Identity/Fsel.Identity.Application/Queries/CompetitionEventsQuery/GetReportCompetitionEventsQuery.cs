// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CompetitionEventsQuery
{
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

        public EnumEducationLevel? EducationLevel { get; set; }
    }

    public class GetReportCompetitionEventsQueryHandler : IRequestHandler<GetReportCompetitionEventsQuery, MethodResult<IList<ReportCompetitionEventModel>>>
    {
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ISystemService _systemService;
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;
        private readonly UserManager<User> _userManager;

        public GetReportCompetitionEventsQueryHandler(ICompetitionEventsRepository competitionEventsRepository,
            IStudentRepository studentRepository,
            ISystemService systemService,
            IStudentCompetitionEventsRepository studentCompetitionEventsRepository,
            UserManager<User> userManager)
        {
            _competitionEventsRepository = competitionEventsRepository;
            _studentRepository = studentRepository;
            _systemService = systemService;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
            _userManager = userManager;
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
            var competitionEvents = await GetCompetitionEventsAsync(competitions);
            competitionEvents = competitionEvents.GroupBy(x => x.Id).Select(x => x.First()).ToList();

            var districtIds = competitionEvents.Where(x => x.LocationId.HasValue).Select(x => x.LocationId.GetValueOrDefault()).ToList();
            var locationResults = await _systemService.GetLocationByIdsAsync(new GetLocationsByIdsQueryModel { IdsStr = string.Join(",", districtIds) });
            var locationDistricts = locationResults.Content?.Result;

            var competitionEventIds = competitionEvents.Select(x => x.Id).ToList();
            competitionEventIds.AddRange(competitions.Select(x => x.Id));

            var schoolIds = competitionEvents.SelectMany(x => x.SchoolIds ?? new List<Guid>()).ToList();
            var schoolResults = await _systemService.GetSchoolsAsync(new GetListSchoolQueryModel { Ids = schoolIds, EducationLevel = request.EducationLevel });
            var schools = schoolResults.Content?.Result ?? new List<SchoolModel>();

            var studentSchoolIds = await (from baseQ in _studentRepository.Queryable.Where(x => x.SchoolId != null).WhereBulkContains(schools.Select(x => x.Id), x => x.SchoolId)
                                          join sce in _studentCompetitionEventsRepository.Queryable on baseQ.Id equals sce.StudentId
                                          join user in _userManager.Users on baseQ.UserId equals user.Id
                                          where competitionEventIds.Contains(sce.CompetitionEventId) && !baseQ.IsDeleted && !user.IsDeleted
                                          select new
                                          {
                                              baseQ.SchoolId,
                                              StudentId = baseQ.Id,
                                              UserId = user.Id,
                                              IsConfirmed = user.EmailConfirmed || user.PhoneNumberConfirmed,
                                          }).ToListAsync(cancellationToken);

            var reportCompetitionEvents = new List<ReportCompetitionEventModel>();
            var studentSchoolLookup = studentSchoolIds.ToLookup(x => x.SchoolId ?? default);
            var locationDistrictLookup = locationDistricts?.ToDictionary(x => x.Id, x => x.Name);
            var schoolSet = schools.Select(x => x.Id).ToHashSet();
            foreach (var item in competitionEvents)
            {
                item.SchoolIds = item.SchoolIds ?? new List<Guid>();
                var studentDistricts = item.SchoolIds.SelectMany(id => studentSchoolLookup[id]).ToList();

                // Dùng HashSet để lọc trùng nhanh
                var studentIds = new HashSet<Guid>(studentDistricts.Select(x => x.StudentId));
                var userIds = new HashSet<Guid>(studentDistricts.Select(x => x.UserId));
                var confirmedUserIds = new HashSet<Guid>(studentDistricts.Where(x => x.IsConfirmed).Select(x => x.UserId));

                reportCompetitionEvents.Add(new ReportCompetitionEventModel
                {
                    LocationId = item.LocationId.GetValueOrDefault(),
                    DistrictName = locationDistrictLookup != null && locationDistrictLookup.ContainsKey(item.LocationId.GetValueOrDefault())
                        ? locationDistrictLookup[item.LocationId.GetValueOrDefault()]
                        : null,
                    NumberRegisteredSchool = item.SchoolIds.Count(schoolSet.Contains),
                    NumberActualParticipatingSchool = studentDistricts.Select(x => x.SchoolId).Distinct().Count(),
                    NumberValidStudentAccount = studentIds.Count,
                    NumberStudentCompleteVerify = confirmedUserIds.Count,
                    StudentIds = studentIds.ToList(),
                    UserIds = userIds.ToList(),
                });
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
