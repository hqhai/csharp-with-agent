// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.CourseGoalQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.IRepositories.CourseGoals;
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.System.Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class SearchCourseGoalQuery : SearchCourseGoalQueryModel, IRequest<MethodResult<PagingItemsModel<CourseGoalModel>>>
    {
    }

    public class SearchCourseGoalQueryHandler : IRequestHandler<SearchCourseGoalQuery, MethodResult<PagingItemsModel<CourseGoalModel>>>
    {
        private readonly ICourseGoalRepository _courseGoalRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;

        public SearchCourseGoalQueryHandler(ICourseGoalRepository courseGoalRepository,
            AuthContext authContext,
            IUserService userService)
        {
            _courseGoalRepository = courseGoalRepository;
            _authContext = authContext;
            _userService = userService;
        }

        public async Task<MethodResult<PagingItemsModel<CourseGoalModel>>> Handle(SearchCourseGoalQuery request, CancellationToken cancellationToken)
        {
            MethodResult<PagingItemsModel<CourseGoalModel>> methodResult = new MethodResult<PagingItemsModel<CourseGoalModel>>();
            ArgumentNullException.ThrowIfNull(request);
            var courseLevels = request.CourseLevelStr.ToList<EnumCourseLevel>();
            var courseTypes = request.CourseTypeStr.ToList<EnumCourseType>();
            var schoolIds = request.SchoolIdStr.ToList<Guid>();
            var classIds = request.ClassIdStr.ToList<Guid>();
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            Guid? schoolId = null;

            var targetRoles = new List<string> { EnumRole.AdminSchool.ToString(), EnumRole.TeacherCampus.ToString(), EnumRole.AdminCampus.ToString() };
            var hasMatchedRole = _authContext.Roles != null && _authContext.Roles.Any(r => targetRoles.Contains(r));
            if (hasMatchedRole)
            {
                var schoolIdResult = await _userService.GetSchoolIdAsync();
                schoolId = schoolIdResult.Content?.Result;
            }

            var query = _courseGoalRepository.Queryable.Where(x => x.GoalCategory != EnumCourseGoalCategory.All);
            if (schoolId.HasValue)
            {
                query = query.Where(x => x.SchoolId == schoolId);
            }

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(x => x.Name != null && x.Name.Contains(request.Keyword));
            }

            if (courseLevels != null && courseLevels.Any())
            {
                query = query.Where(x => courseLevels.Contains(x.CourseLevel));
            }

            if (courseTypes != null && courseTypes.Any())
            {
                query = query.Where(x => courseTypes.Contains(x.CourseType));
            }

            if (request.CourseType.HasValue)
            {
                query = query.Where(x => x.CourseType == request.CourseType);
            }
            if (request.CourseLevel.HasValue)
            {
                query = query.Where(x => x.CourseLevel == request.CourseLevel);
            }
            if (schoolIds != null && schoolIds.Any())
            {
                query = query.Where(x => x.SchoolId != null).WhereBulkContains(schoolIds, x => x.SchoolId);
            }

            if (classIds != null && classIds.Any())
            {
                query = query.Where(x => x.ClassId != null).WhereBulkContains(classIds, x => x.ClassId);
            }

            return await _courseGoalRepository.GetListByPageResultAsync<CourseGoalModel>(query, request, cancellationToken);
        }
    }
}
