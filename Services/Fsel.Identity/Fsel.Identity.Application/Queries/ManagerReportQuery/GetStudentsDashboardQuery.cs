// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.ManagerReportQuery
{
    using System.Linq;
    using System.Text.Json.Serialization;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentsDashboardQuery : IRequest<MethodResult<IList<StudentDtoModel>>>
    {
        public string? SchoolClassStr { get; set; }

        [JsonIgnore]
        public IList<string>? SchoolClasses
        {
            get
            {
                return SchoolClassStr.ToList<string>();
            }
        }
    }

    public class GetStudentsDashboardQueryHandler : IRequestHandler<GetStudentsDashboardQuery, MethodResult<IList<StudentDtoModel>>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IUserSchoolRepository _userSchoolRepository;
        private readonly AuthContext _authContext;

        public GetStudentsDashboardQueryHandler(IStudentRepository studentRepository,
            IUserSchoolRepository userSchoolRepository,
            AuthContext authContext)
        {
            _studentRepository = studentRepository;
            _userSchoolRepository = userSchoolRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<IList<StudentDtoModel>>> Handle(GetStudentsDashboardQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<StudentDtoModel>> methodResult = new MethodResult<IList<StudentDtoModel>>();
            var query = _studentRepository.Queryable.Where(x => x.CourseId.HasValue).Select(i => new StudentDtoModel
            {
                Id = i.Id,
                FullName = i.User!.FullName,
                BirthDay = i.User.Birthday,
                Email = i.User.Email,
                PhoneNumber = i.User.PhoneNumber,
                CourseLevel = i.CourseLevel,
                ExpiredDate = i.ExpiredDate,
                School = i.School,
                SchoolClass = i.SchoolClass,
                SchoolGrade = i.SchoolGrade,
                BaseCourseLevel = i.BaseCourseLevel,
                SchoolId = i.SchoolId,
                CourseId = i.CourseId,
                UserId = i.UserId,
                CreatedDate = i.CreatedDate,
            });
            var targetRoles = new List<string> { EnumRole.AdminSchool.ToString(), EnumRole.TeacherCampus.ToString(), EnumRole.AdminCampus.ToString() };
            var hasMatchedRole = _authContext.Roles != null && _authContext.Roles.Any(r => targetRoles.Contains(r));
            if (hasMatchedRole)
            {
                var schoolId = await _userSchoolRepository.GetSchoolIdAsync();
                query = query.Where(x => x.SchoolId.HasValue && x.SchoolId == schoolId);
            }

            if (request.SchoolClasses != null && request.SchoolClasses.Any())
            {
                query = query.Where(x => !string.IsNullOrEmpty(x.SchoolClass) && request.SchoolClasses.Any(y => y == x.SchoolClass));
            }

            var lists = await query.AsNoTracking().ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            methodResult.Result = lists;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
