// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CompetitionEventsQuery
{
    using System.Diagnostics;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.SystemService.Model;
    using Fsel.Identity.Application.Services.SystemService.QueryModels;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentEventRegistrationsQuery : IRequest<MethodResult<IList<EventRegistrationModel>>>
    {
        public string? EventCodeStr { get; set; }
        public string? DistrictName { get; set; }
        public Guid? StudentId { get; set; }
        public EnumCourseType CourseType { get; set; }
        public string? UserNameStr { get; set; }

        public IList<string>? UserNames
        {
            get
            {
                return UserNameStr.ToList<string>();
            }
        }

        public IList<string>? EventCodes
        {
            get
            {
                return EventCodeStr.ToList<string>();
            }
        }
    }

    public class GetStudentEventRegistrationsQueryHandler : IRequestHandler<GetStudentEventRegistrationsQuery, MethodResult<IList<EventRegistrationModel>>>
    {
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ISystemService _systemService;
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;
        private readonly IHumanRepository _humanRepository;
        private readonly UserManager<User> _userManager;

        public GetStudentEventRegistrationsQueryHandler(ICompetitionEventsRepository competitionEventsRepository,
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

        public async Task<MethodResult<IList<EventRegistrationModel>>> Handle(GetStudentEventRegistrationsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<EventRegistrationModel>>();
            var eventRegistrations = new List<EventRegistrationModel>();
            if (request.EventCodes != null && request.EventCodes.Any())
            {
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
                if (!string.IsNullOrEmpty(request.DistrictName))
                {
                    request.DistrictName = request.DistrictName.ToLower(System.Globalization.CultureInfo.CurrentCulture).Trim();
                    var district = locationDistricts?.FirstOrDefault(x => x.Name != null && x.Name.ToLower(System.Globalization.CultureInfo.CurrentCulture).Trim() == request.DistrictName);
                    competitionEvents = competitionEvents.Where(x => district != null && x.Id == district.Id).ToList();
                }
                var schoolIds = competitionEvents.SelectMany(x => x.SchoolIds ?? new List<Guid>()).ToList();
                var schoolResults = await _systemService.GetSchoolsAsync(new GetListSchoolQueryModel { Ids = schoolIds });
                var schools = schoolResults.Content?.Result ?? new List<SchoolModel>();

                var courseLevels = EnumCourseLevelHelper.GetEnumCourseLevels(request.CourseType);
                eventRegistrations = await (from baseQ in _studentRepository.Queryable.Where(x => x.CourseId.HasValue)
                                            join sce in _studentCompetitionEventsRepository.Queryable.WhereBulkContains(competitionEventIds, x => x.CompetitionEventId) on baseQ.Id equals sce.StudentId
                                            join ce in _competitionEventsRepository.Queryable on sce.CompetitionEventId equals ce.Id
                                            join h in _humanRepository.Queryable on baseQ.HumanId equals h.Id
                                            join u in _userManager.Users on h.UserId equals u.Id
                                            where (!request.StudentId.HasValue || baseQ.Id == request.StudentId.Value)
                                            && baseQ.CourseLevel.HasValue
                                            && courseLevels.Contains(baseQ.CourseLevel.Value)
                                            select new EventRegistrationModel
                                            {
                                                StudentId = baseQ.Id,
                                                BirthDay = h.Birthday,
                                                District = ce.Name,
                                                UserName = u.UserName,
                                                DistrictId = ce.LocationId,
                                                Email = h.Email,
                                                ExpiredDate = baseQ.ExpiredDate,
                                                FullName = h.FullName,
                                                IsBussinessCheckBox = false,
                                                ParentEmail = baseQ.ParentEmail,
                                                ParentPhoneNumber = baseQ.ParentPhoneNumber,
                                                PhoneNumber = h.PhoneNumber,
                                                Province = string.Empty,
                                                ProvinceId = baseQ.ProvinceId ?? ce.LocationId,
                                                School = baseQ.School,
                                                SchoolClass = baseQ.SchoolClass,
                                                SchoolGrade = baseQ.SchoolGrade,
                                                SchoolId = baseQ.SchoolId,
                                                SchoolStudentCode = string.Empty,
                                                Status = EnumEventRegistrationStatus.Active,
                                                StudentMainMajor = string.Empty,
                                                TeacherPhoneNumber = string.Empty,
                                                CourseId = baseQ.CourseId,
                                                CourseLevel = baseQ.CourseLevel,
                                            }).ToListAsync(cancellationToken);
                foreach (var item in eventRegistrations)
                {
                    var names = item.FullName?.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var school = schools.FirstOrDefault(x => x.Id == item.SchoolId);

                    item.FirstName = names != null && names.Length > 1 ? string.Join(' ', names.Take(names.Length - 1)) : string.Empty;
                    item.LastName = names != null && names.Length > 0 ? names.Last() : string.Empty;
                    item.District = item.District ?? locationDistricts?.FirstOrDefault(x => x.Id == item.DistrictId)?.Name;
                }
            }
            else if (request.UserNames != null && request.UserNames.Any())
            {
                var courseLevels = EnumCourseLevelHelper.GetEnumCourseLevels(request.CourseType);
                eventRegistrations = await (from baseQ in _studentRepository.Queryable.Where(x => x.CourseId.HasValue)
                                            join sce in _studentCompetitionEventsRepository.Queryable on baseQ.Id equals sce.StudentId into sceGroup
                                            from sce in sceGroup.DefaultIfEmpty()
                                            join ce in _competitionEventsRepository.Queryable on sce.CompetitionEventId equals ce.Id into ceGroup
                                            from ce in ceGroup.DefaultIfEmpty()

                                            join h in _humanRepository.Queryable on baseQ.HumanId equals h.Id
                                            join u in _userManager.Users.WhereBulkContains(request.UserNames, x => x.UserName) on h.UserId equals u.Id
                                            where (!request.StudentId.HasValue || baseQ.Id == request.StudentId.Value)
                                            && baseQ.CourseLevel.HasValue
                                            && courseLevels.Contains(baseQ.CourseLevel.Value)
                                            select new EventRegistrationModel
                                            {
                                                StudentId = baseQ.Id,
                                                BirthDay = h.Birthday,
                                                UserName = u.UserName,
                                                DistrictId = baseQ.DistrictId ?? ce.LocationId,
                                                Email = h.Email,
                                                ExpiredDate = baseQ.ExpiredDate,
                                                FullName = h.FullName,
                                                IsBussinessCheckBox = false,
                                                ParentEmail = baseQ.ParentEmail,
                                                ParentPhoneNumber = baseQ.ParentPhoneNumber,
                                                PhoneNumber = h.PhoneNumber,
                                                ProvinceId = baseQ.ProvinceId ?? ce.LocationId,
                                                School = baseQ.School,
                                                SchoolClass = baseQ.SchoolClass,
                                                SchoolGrade = baseQ.SchoolGrade,
                                                SchoolId = baseQ.SchoolId,
                                                Status = EnumEventRegistrationStatus.Active,
                                                CourseId = baseQ.CourseId,
                                                CourseLevel = baseQ.CourseLevel,
                                            }).ToListAsync(cancellationToken);
                var districtIds = eventRegistrations.Where(x => x.DistrictId.HasValue).Select(x => x.DistrictId).ToList();
                var locationResults = await _systemService.GetLocationByIdsAsync(new GetLocationsByIdsQueryModel { IdsStr = string.Join(",", districtIds) });
                var locationDistricts = locationResults.Content?.Result;

                var schoolIds = eventRegistrations.Where(x => x.SchoolId.HasValue && !string.IsNullOrEmpty(x.School)).Select(x => x.SchoolId.GetValueOrDefault()).ToList();

                var schoolResults = await _systemService.GetSchoolsAsync(new GetListSchoolQueryModel { Ids = schoolIds });
                var schools = schoolResults.Content?.Result ?? new List<SchoolModel>();

                foreach (var item in eventRegistrations)
                {
                    var names = item.FullName?.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var school = schools.FirstOrDefault(x => x.Id == item.SchoolId);
                    item.FirstName = names != null && names.Length > 1 ? string.Join(' ', names.Take(names.Length - 1)) : string.Empty;
                    item.LastName = names != null && names.Length > 0 ? names.Last() : string.Empty;
                    item.District = locationDistricts?.FirstOrDefault(x => x.Id == item.DistrictId)?.Name;
                }
            }

            methodResult.Result = eventRegistrations;
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
