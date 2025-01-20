// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CompetitionEventsQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.SystemService.QueryModels;
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
        private readonly IEventRegistrationRepository _eventRegistrationRepository;
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;

        public GetStudentEventRegistrationsQueryHandler(ICompetitionEventsRepository competitionEventsRepository,
            IStudentRepository studentRepository,
            ISystemService systemService,
            IEventRegistrationRepository eventRegistrationRepository,
            IStudentCompetitionEventsRepository studentCompetitionEventsRepository)
        {
            _competitionEventsRepository = competitionEventsRepository;
            _studentRepository = studentRepository;
            _systemService = systemService;
            _eventRegistrationRepository = eventRegistrationRepository;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
        }

        public async Task<MethodResult<IList<EventRegistrationModel>>> Handle(GetStudentEventRegistrationsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<EventRegistrationModel>>();
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
                competitionEvents = competitionEvents.Where(x => district != null && x.Id == district.Id).ToList();
            }
            var courseLevels = EnumCourseLevelHelper.GetEnumCourseLevels(request.CourseType);
            var eventRegistrations = await (from baseQ in _studentRepository.Queryable
                                            join er in _eventRegistrationRepository.Queryable on baseQ.Id equals er.StudentId
                                            join sce in _studentCompetitionEventsRepository.Queryable on baseQ.Id equals sce.StudentId
                                            where er.DistrictId.HasValue && competitionEvents.Select(x => x.LocationId).Contains(er.DistrictId.Value)
                                            && competitions.Select(x => x.Id).Contains(er.CompetitionEventId)
                                            && (!request.StudentId.HasValue || baseQ.Id == request.StudentId.Value)
                                            && baseQ.CourseLevel.HasValue && courseLevels.Contains(baseQ.CourseLevel.Value)
                                            && baseQ.CourseId.HasValue
                                            select new EventRegistrationModel
                                            {
                                                StudentId = er.StudentId,
                                                BirthDay = er.BirthDay,
                                                District = er.District,
                                                DistrictId = er.DistrictId,
                                                Email = er.Email,
                                                ExpiredDate = baseQ.ExpiredDate,
                                                FirstName = er.FirstName,
                                                LastName = er.LastName,
                                                IsBussinessCheckBox = er.IsBussinessCheckBox,
                                                ParentEmail = er.ParentEmail,
                                                ParentPhoneNumber = er.ParentPhoneNumber,
                                                PhoneNumber = er.PhoneNumber,
                                                Province = er.Province,
                                                ProvinceId = er.ProvinceId,
                                                School = er.School,
                                                SchoolClass = er.SchoolClass,
                                                SchoolGrade = er.SchoolGrade,
                                                SchoolId = er.SchoolId,
                                                SchoolStudentCode = er.SchoolStudentCode,
                                                Status = er.Status,
                                                StudentMainMajor = er.StudentMainMajor,
                                                TeacherPhoneNumber = er.TeacherPhoneNumber,
                                                CourseId = baseQ.CourseId,
                                                CourseLevel = baseQ.CourseLevel,
                                            }).ToListAsync(cancellationToken);

            methodResult.Result = eventRegistrations;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
