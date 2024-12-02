// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.ManagerReportQuery
{
    using System.Linq;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentsDashboardQuery : IRequest<MethodResult<IList<StudentDtoModel>>>
    {
        public IList<EnumCourseLevel>? CourseLevels { get; set; }
        public IList<string>? SchoolClasses { get; set; }
    }

    public class GetStudentsDashboardQueryHandler : IRequestHandler<GetStudentsDashboardQuery, MethodResult<IList<StudentDtoModel>>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IUserSchoolRepository _userSchoolRepository;

        public GetStudentsDashboardQueryHandler(IStudentRepository studentRepository, IUserSchoolRepository userSchoolRepository)
        {
            _studentRepository = studentRepository;
            _userSchoolRepository = userSchoolRepository;
        }

        public async Task<MethodResult<IList<StudentDtoModel>>> Handle(GetStudentsDashboardQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<StudentDtoModel>> methodResult = new MethodResult<IList<StudentDtoModel>>();
            var schoolId = await _userSchoolRepository.GetSchoolIdAsync();
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
            if (schoolId.HasValue)
            {
                query = query.Where(x => x.SchoolId.HasValue && x.SchoolId == schoolId.Value);
            }
            if (request.SchoolClasses != null)
            {
                query = query.Where(x => !string.IsNullOrEmpty(x.SchoolClass) && request.SchoolClasses.Contains(x.SchoolClass));
            }
            if (request.CourseLevels != null)
            {
                query = query.Where(x => x.CourseLevel.HasValue && request.CourseLevels.Contains(x.CourseLevel.Value));
            }

            var lists = await query.AsNoTracking().ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            methodResult.Result = lists;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
