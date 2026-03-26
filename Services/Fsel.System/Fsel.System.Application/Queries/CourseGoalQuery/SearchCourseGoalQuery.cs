// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.CourseGoalQuery
{
    using Common.ActionResults;
    using Core.Base;
    using Core.Base.BaseModels;
    using Shared.Enums;
    using Shared.Helpers;
    using Services.UserServices;
    using Domain.IRepositories.CourseGoals;
    using Domain.Models.EntityModels;
    using Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Fsel.System.Application.Services.CourseServices;

    public class SearchCourseGoalQuery : SearchCourseGoalQueryModel, IRequest<MethodResult<PagingItemsModel<CourseGoalModel>>>
    {
    }

    public class SearchCourseGoalQueryHandler : IRequestHandler<SearchCourseGoalQuery, MethodResult<PagingItemsModel<CourseGoalModel>>>
    {
        private readonly ICourseGoalRepository _courseGoalRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly ICourseService _courseService;

        public SearchCourseGoalQueryHandler(ICourseGoalRepository courseGoalRepository,
            AuthContext authContext,
            IUserService userService,
            ICourseService courseService)
        {
            _courseGoalRepository = courseGoalRepository;
            _authContext = authContext;
            _userService = userService;
            _courseService = courseService;
        }

        public async Task<MethodResult<PagingItemsModel<CourseGoalModel>>> Handle(SearchCourseGoalQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<PagingItemsModel<CourseGoalModel>>();
            ArgumentNullException.ThrowIfNull(request);

            var courseTypeByLevelIds = request.CourseTypeStr.ToList<Guid>();
            var schoolIds = request.SchoolIdStr.ToList<Guid>();
            var classIds = request.ClassIdStr.ToList<Guid>();
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            Guid? schoolId = null;

            var targetRoles = new List<string> { nameof(EnumRole.AdminSchool), nameof(EnumRole.TeacherCampus), nameof(EnumRole.AdminCampus) };
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

            if (courseTypeByLevelIds != null && courseTypeByLevelIds.Any())
            {
                query = query.Where(x => x.LevelId != null && courseTypeByLevelIds.Contains(x.LevelId.Value));
            }

            if (schoolIds != null && schoolIds.Any())
            {
                query = query.Where(x => x.SchoolId != null).WhereBulkContains(schoolIds, x => x.SchoolId);
            }

            if (classIds != null && classIds.Any())
            {
                query = query.Where(x => x.ClassId != null).WhereBulkContains(classIds, x => x.ClassId);
            }

            var courseGoals = await _courseGoalRepository.GetListByPageResultAsync<CourseGoalModel>(query, request, cancellationToken);
            var levelIds = courseGoals?.Result?.Items?.Where(x => x != null).Select(x => x.LevelId.Value);
            if (levelIds != null && levelIds.Any())
            {
                var levels = await _courseService.GetLevels(levelIds.ToList());
                foreach (var courseGoal in courseGoals.Result.Items)
                {
                    courseGoal.LevelName = levels?.Content?.Result?.FirstOrDefault(x => x.Id == courseGoal.LevelId)?.Name;
                }
            }

            return courseGoals;
        }
    }
}
