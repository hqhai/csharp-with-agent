// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.ManagerReportQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.SystemService.QueryModels;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.QueryModels.ManagerReports;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentsQuery : SearchStudentQueryModel, IRequest<MethodResult<IList<StudentDtoModel>>>
    {
    }

    public class GetStudentsQueryHandler : IRequestHandler<GetStudentsQuery, MethodResult<IList<StudentDtoModel>>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IUserSchoolRepository _userSchoolRepository;
        private readonly AuthContext _authContext;
        private readonly ISystemService _systemService;

        public GetStudentsQueryHandler(IStudentRepository studentRepository,
            IUserSchoolRepository userSchoolRepository,
            AuthContext authContext,
            ISystemService systemService)
        {
            _studentRepository = studentRepository;
            _userSchoolRepository = userSchoolRepository;
            _authContext = authContext;
            _systemService = systemService;
        }

        public async Task<MethodResult<IList<StudentDtoModel>>> Handle(GetStudentsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<StudentDtoModel>> methodResult = new MethodResult<IList<StudentDtoModel>>();

            var query = _studentRepository.Queryable.Select(i => new StudentDtoModel
            {
                Id = i.Id,
                FullName = i.Human!.FullName,
                BirthDay = i.Human.Birthday,
                Email = i.Human.Email,
                PhoneNumber = i.Human.PhoneNumber,
                CourseLevel = i.CourseLevel,
                ExpiredDate = i.ExpiredDate,
                School = i.School,
                SchoolClass = i.SchoolClass,
                SchoolGrade = i.SchoolGrade,
                BaseCourseLevel = i.BaseCourseLevel,
                SchoolId = i.SchoolId,
                CourseId = i.CourseId,
                UserId = i.Human.UserId,
                CreatedDate = i.CreatedDate,
            });
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                request.Keyword = request.Keyword.Trim().ToLower(System.Globalization.CultureInfo.CurrentCulture);
                query = query.Where(m => (m.FullName ?? string.Empty).Trim().ToLower().Contains(request.Keyword) ||
                                         (m.PhoneNumber ?? string.Empty).Trim().ToLower().Contains(request.Keyword) ||
                                         (m.Email ?? string.Empty).Trim().ToLower().Contains(request.Keyword));
            }

            if (_authContext.Roles != null && _authContext.Roles.Contains(EnumRole.AdminSchool.ToString()))
            {
                var schoolId = await _userSchoolRepository.GetSchoolIdAsync();
                query = query.Where(x => x.SchoolId.HasValue && x.SchoolId == schoolId.Value);
            }
            if (_authContext.Roles != null && _authContext.Roles.Contains(EnumRole.Admin.ToString()))
            {
                var schoolIdResults = await _systemService.GetSchoolIdsAsync(new GetSchoolsQueryModel
                {
                    DistrictIds = request.DistrictIds,
                    ProvinceIds = request.ProvinceIds,
                    SchoolIds = request.SchoolIds,
                });
                var schoolIds = schoolIdResults.Content?.Result;
                query = query.Where(x => x.SchoolId.HasValue && schoolIds != null && schoolIds.Contains(x.SchoolId.Value));
            }

            if (request.CourseType.HasValue)
            {
                var courseLevels = EnumCourseLevelHelper.GetEnumCourseLevels(request.CourseType.Value);
                query = query.Where(x => x.CourseLevel.HasValue && courseLevels.Contains(x.CourseLevel.Value));
            }
            if (request.CourseLevel.HasValue)
            {
                query = query.Where(x => x.CourseLevel.HasValue && x.CourseLevel == request.CourseLevel.Value);
            }

            if (!string.IsNullOrEmpty(request.SchoolGrade))
            {
                query = query.Where(x => !string.IsNullOrEmpty(x.SchoolGrade) && x.SchoolGrade.Trim().ToLower() == request.SchoolGrade.Trim().ToLower());
            }
            if (!string.IsNullOrEmpty(request.SchoolClass))
            {
                query = query.Where(x => !string.IsNullOrEmpty(x.SchoolClass) && x.SchoolClass.Trim().ToLower().Contains(request.SchoolClass.Trim().ToLower()));
            }
            if (request.LearningStatus.HasValue)
            {
                if (request.LearningStatus.Value == EnumLearningStatus.InProgress)
                {
                    query = query.Where(x => x.ExpiredDate.HasValue && x.ExpiredDate.Value > DateTime.UtcNow);
                }
                else
                {
                    query = query.Where(x => x.ExpiredDate.HasValue && x.ExpiredDate.Value <= DateTime.UtcNow);
                }
            }
            if (request.IsLearning.HasValue)
            {
                query = query.Where(x => request.IsLearning.Value ? x.CourseId.HasValue : !x.CourseId.HasValue);
            }
            var list = await query.AsNoTracking().ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            methodResult.Result = list.OrderBy(x => int.TryParse(x.SchoolGrade, out int graded) ? graded : 0).ThenBy(x => x.SchoolClass).ThenBy(x => x.FullName).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
