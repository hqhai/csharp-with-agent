// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCoursesByLevelQuery : IRequest<MethodResult<IList<CourseModel>>>
    {
        public EnumCourseStatus? DifferentStatus { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public bool? IsDefault { get; set; }
        public Guid? SchoolId { get; set; }
    }

    public class GetCoursesByLevelQueryHandler : IRequestHandler<GetCoursesByLevelQuery, MethodResult<IList<CourseModel>>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly ICurriculumRepository _curriculumRepository;

        public GetCoursesByLevelQueryHandler(ICourseRepository courseRepository,
            AuthContext authContext,
            IUserService userService,
            ICurriculumRepository curriculumRepository)
        {
            _courseRepository = courseRepository;
            _authContext = authContext;
            _userService = userService;
            _curriculumRepository = curriculumRepository;
        }

        public async Task<MethodResult<IList<CourseModel>>> Handle(GetCoursesByLevelQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<CourseModel>>();

            var queryCourse = _courseRepository.Queryable.Where(x => !x.IsArchive)
                                .Where(x => !request.DifferentStatus.HasValue || x.Status != request.DifferentStatus)
                                .Where(x => request.CourseLevel != null && x.CourseLevel == request.CourseLevel);

            var queryCurriculum = _curriculumRepository.Queryable;

            var targetRoles = new List<string> { EnumRole.AdminSchool.ToString(), EnumRole.TeacherCampus.ToString(), EnumRole.AdminCampus.ToString() };
            var hasMatchedRole = _authContext.Roles != null && _authContext.Roles.Any(r => targetRoles.Contains(r));
            if (hasMatchedRole)
            {
                var schoolId = (await _userService.GetSchoolIdAsync()).Content?.Result;
                queryCurriculum = queryCurriculum.Where(m => m.SchoolId == schoolId);
            }
            else if (request.SchoolId.HasValue)
            {
                queryCurriculum = queryCurriculum.Where(m => m.SchoolId == request.SchoolId.Value);
            }

            IQueryable<CourseModel> query;
            var baseQuery = from c in queryCourse
                            join cu in queryCurriculum on c.Id equals cu.CourseCloneId into g
                            from cu in g.DefaultIfEmpty()
                            select new { c, cu };

            if (request.IsDefault is true)
            {
                // chỉ course KHÔNG có curriculum
                query = baseQuery
                    .Where(x => x.cu == null)
                    .Select(x => new CourseModel
                    {
                        Id = x.c.Id,
                        Code = x.c.Code,
                        Status = x.c.Status,
                        Name = x.c.Name,
                        CourseLevel = x.c.CourseLevel,
                        CreatedDate = x.c.CreatedDate,
                        UpdatedDate = x.c.UpdatedDate,
                    });
            }
            else if (request.IsDefault is false)
            {
                // chỉ course CÓ curriculum
                query = baseQuery
                    .Where(x => x.cu != null)
                    .Select(x => new CourseModel
                    {
                        Id = x.c.Id,
                        Code = x.c.Code,
                        Status = x.c.Status,
                        Name = x.cu.CurriculumName,
                        CourseLevel = x.c.CourseLevel,
                        CreatedDate = x.c.CreatedDate,
                        UpdatedDate = x.c.UpdatedDate,
                    });
            }
            else
            {
                // lấy tất cả, ưu tiên tên từ curriculum
                query = baseQuery
                    .Select(x => new CourseModel
                    {
                        Id = x.c.Id,
                        Code = x.c.Code,
                        Status = x.c.Status,
                        Name = x.cu != null ? x.cu.CurriculumName : x.c.Name,
                        CourseLevel = x.c.CourseLevel,
                        CreatedDate = x.c.CreatedDate,
                        UpdatedDate = x.c.UpdatedDate,
                    });
            }

            var courses = await query.OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                                     .ToListAsync(cancellationToken);
            methodResult.Result = courses;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
