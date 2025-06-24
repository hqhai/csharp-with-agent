// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CompetitionEventsQuery
{
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.SystemService.Model;
    using Fsel.Identity.Application.Services.SystemService.QueryModels;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class GetCompetitionEventToEventParentQuery : IRequest<MethodResult<IList<ReportCompetitionEventModel>>>
    {
        public string? EventCode { get; set; }
        public string? DistrictName { get; set; }
    }

    public class GetCompetitionEventToEventParentQueryHandler : IRequestHandler<GetCompetitionEventToEventParentQuery, MethodResult<IList<ReportCompetitionEventModel>>>
    {
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ISystemService _systemService;
        private readonly IEventRegistrationRepository _eventRegistrationRepository;
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<GetCompetitionEventToEventParentQueryHandler> _logger;

        public GetCompetitionEventToEventParentQueryHandler(ICompetitionEventsRepository competitionEventsRepository,
            IStudentRepository studentRepository,
            ISystemService systemService,
            IEventRegistrationRepository eventRegistrationRepository,
            IStudentCompetitionEventsRepository studentCompetitionEventsRepository,
            UserManager<User> userManager,
            ILogger<GetCompetitionEventToEventParentQueryHandler> logger)
        {
            _competitionEventsRepository = competitionEventsRepository;
            _studentRepository = studentRepository;
            _systemService = systemService;
            _eventRegistrationRepository = eventRegistrationRepository;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<MethodResult<IList<ReportCompetitionEventModel>>> Handle(GetCompetitionEventToEventParentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<ReportCompetitionEventModel>>();
            var competition = await _competitionEventsRepository.Queryable
                                                    .FirstOrDefaultAsync(x => x.EventCode == request.EventCode, cancellationToken);
            if (competition == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(competition), request.EventCode);
                return methodResult;
            }

            var competitionEvents = await GetCompetitionEventsAsync(competition);
            _logger.LoggerRequest($"GetCompetitionEventToEventParentQueryHandler : {competitionEvents.Select(x => x.EventCode).Serialize()}");
            var districtIds = competitionEvents.Where(x => x.LocationId.HasValue).Select(x => x.LocationId.GetValueOrDefault()).ToList();
            var locationResults = await _systemService.GetLocationByIdsAsync(new GetLocationsByIdsQueryModel { IdsStr = string.Join(",", districtIds) });
            var locationDistricts = locationResults.Content?.Result;

            if (!string.IsNullOrEmpty(request.DistrictName))
            {
                request.DistrictName = request.DistrictName.ToLower(System.Globalization.CultureInfo.CurrentCulture).Trim();
                var district = locationDistricts?.FirstOrDefault(x => x.Name != null && x.Name.ToLower(System.Globalization.CultureInfo.CurrentCulture).Trim() == request.DistrictName);
                competitionEvents = competitionEvents.Where(x => district != null && x.Id == district.Id).ToList();
            }

            var competitionEventIds = competitionEvents.Select(x => x.Id).ToList();
            competitionEventIds.Add(competition.Id);

            var schoolIds = competitionEvents.SelectMany(x => x.SchoolIds ?? new List<Guid>()).ToList();
            var schoolResults = await _systemService.GetSchoolsAsync(new GetListSchoolQueryModel { Ids = schoolIds });
            var schools = schoolResults.Content?.Result ?? new List<SchoolModel>();

            var studentToEvents = await (from baseQ in _studentRepository.Queryable
                                         join sce in _studentCompetitionEventsRepository.Queryable on baseQ.Id equals sce.StudentId
                                         join er in _eventRegistrationRepository.Queryable on baseQ.Id equals er.StudentId into erGroup
                                         join user in _userManager.Users on baseQ.UserId equals user.Id
                                         where baseQ.SchoolId.HasValue && !erGroup.Any() && !baseQ.IsDeleted
                                         && competitionEventIds.Contains(sce.CompetitionEventId)
                                         group new { baseQ, user }
                                         by baseQ.SchoolId into g
                                         select new
                                         {
                                             SchoolId = g.Key.GetValueOrDefault(),
                                             StudentIds = g.Select(x => x.baseQ.Id).Distinct().ToList(),
                                             CountCompleteVerify = g.Where(x => x.user.EmailConfirmed || x.user.PhoneNumberConfirmed).Select(x => x.user.Id).Distinct().Count(),
                                         }).ToListAsync(cancellationToken);

            var studentToRegisters = await (from baseQ in _studentRepository.Queryable
                                            join er in _eventRegistrationRepository.Queryable on baseQ.Id equals er.StudentId
                                            join user in _userManager.Users on baseQ.UserId equals user.Id
                                            where baseQ.SchoolId.HasValue && !baseQ.IsDeleted
                                            && competitionEventIds.Contains(er.CompetitionEventId)
                                            group new { baseQ, user }
                                            by baseQ.SchoolId into g
                                            select new
                                            {
                                                SchoolId = g.Key.GetValueOrDefault(),
                                                StudentIds = g.Select(x => x.baseQ.Id).Distinct().ToList(),
                                                CountCompleteVerify = g.Where(x => x.user.EmailConfirmed || x.user.PhoneNumberConfirmed).Select(x => x.user.Id).Distinct().Count(),
                                            }).ToListAsync(cancellationToken);

            var reportCompetitionEvents = new List<ReportCompetitionEventModel>();
            foreach (var item in competitionEvents)
            {
                var studentDistrictEvents = studentToEvents.Where(x => item.SchoolIds != null && item.SchoolIds.Contains(x.SchoolId)).ToList();
                var studentDistrictRegisters = studentToRegisters.Where(x => item.SchoolIds != null && item.SchoolIds.Contains(x.SchoolId)).ToList();
                var studentEvents = studentDistrictEvents.Concat(studentDistrictRegisters).GroupBy(x => x.SchoolId)
                                                         .Select(g => new
                                                         {
                                                             SchoolId = g.Key,
                                                             StudentIds = g.SelectMany(x => x.StudentIds).Distinct().ToList(),
                                                             CountCompleteVerify = g.Sum(x => x.CountCompleteVerify),
                                                         }).ToList();

                var studentIdEvents = studentDistrictEvents.SelectMany(x => x.StudentIds).Distinct().ToList();
                var studentIdRegisters = studentDistrictRegisters.SelectMany(x => x.StudentIds).Distinct().ToList();

                var schoolDistricts = schools.Where(x => item.SchoolIds != null && item.SchoolIds.Contains(x.Id));
                var reportCompetition = new ReportCompetitionEventModel
                {
                    DistrictName = locationDistricts?.FirstOrDefault(x => x.Id == item.LocationId)?.Name,
                    NumberRegisteredSchool = schoolDistricts?.Count() ?? default,
                    NumberActualParticipatingSchool = studentDistrictEvents.Select(x => x.SchoolId).Distinct().Count(),
                    NumberValidStudentAccount = studentIdEvents.Count,
                    NumberStudentAccountRegister = studentIdRegisters.Count,
                    StudentIds = studentEvents.SelectMany(x => x.StudentIds).ToList(),
                    ReportCompetitionEventSchools = schoolDistricts?.Select(school =>
                    {
                        var studentEventSchool = studentEvents.FirstOrDefault(x => x.SchoolId == school.Id);
                        var studentDistrictEvent = studentDistrictEvents.FirstOrDefault(x => x.SchoolId == school.Id);
                        var studentDistrictRegister = studentDistrictRegisters.FirstOrDefault(x => x.SchoolId == school.Id);

                        return new ReportCompetitionEventSchoolModel
                        {
                            SchoolName = schools?.FirstOrDefault(x => x.Id == school.Id)?.Name,
                            NumberStudentCompleteVerify = studentEventSchool?.CountCompleteVerify ?? default,
                            NumberStudentAccountRegister = studentDistrictRegister?.StudentIds.Count ?? default,
                            NumberValidStudentAccount = studentDistrictEvent?.StudentIds.Count ?? default,
                            StudentIds = studentEventSchool?.StudentIds ?? new List<Guid>()
                        };
                    }).ToList() ?? new List<ReportCompetitionEventSchoolModel>()
                };

                reportCompetitionEvents.Add(reportCompetition);
            }

            methodResult.Result = reportCompetitionEvents;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<List<CompetitionEvent>> GetCompetitionEventsAsync(CompetitionEvent competitionEvent)
        {
            var subEvents = await _competitionEventsRepository.Queryable.Where(x => x.ParentEventId == competitionEvent.Id).ToListAsync();
            if (subEvents == null || !subEvents.Any())
            {
                return new List<CompetitionEvent> { competitionEvent };
            }
            var leafEvents = new List<CompetitionEvent>();
            foreach (var subEvent in subEvents)
            {
                var childLeafEvents = await GetCompetitionEventsAsync(subEvent);
                leafEvents.AddRange(childLeafEvents);
            }
            return leafEvents;
        }
    }
}
