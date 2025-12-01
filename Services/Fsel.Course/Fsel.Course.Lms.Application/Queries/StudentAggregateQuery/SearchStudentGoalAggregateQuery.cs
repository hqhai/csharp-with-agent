// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentAggregateQuery
{
    using Common.ActionResults;
    using Core.Base;
    using Core.Base.BaseModels;
    using Core.Extensions;
    using Domain.IRepositories;
    using Domain.Models.EntityModels;
    using Domain.Models.QueryModels.StudentProgress;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Services.UserServices;
    using Services.UserServices.QueryModels;
    using Shared.Enums;
    using Shared.Helpers;

    public class SearchStudentGoalAggregateQuery : SearchStudentGoalAggregateQueryModel, IRequest<MethodResult<PagingItemsModel<StudentGoalAggregateModel>>>
    {
    }

    public class SearchStudentGoalAggregateQueryHandler : IRequestHandler<SearchStudentGoalAggregateQuery, MethodResult<PagingItemsModel<StudentGoalAggregateModel>>>
    {
        private readonly IStudentGoalAggregateRepository _studentGoalAggregateRepository;
        private readonly IStudentGoalSummaryRepository _studentGoalSummaryRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;

        public SearchStudentGoalAggregateQueryHandler(IStudentGoalAggregateRepository studentGoalAggregateRepository,
            IStudentGoalSummaryRepository studentGoalSummaryRepository,
            IUserService userService,
            AuthContext authContext)
        {
            _studentGoalAggregateRepository = studentGoalAggregateRepository;
            _studentGoalSummaryRepository = studentGoalSummaryRepository;
            _userService = userService;
            _authContext = authContext;
        }

        public async Task<MethodResult<PagingItemsModel<StudentGoalAggregateModel>>> Handle(SearchStudentGoalAggregateQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<StudentGoalAggregateModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var (weekStartUtc, weekEndUtc) = DateTimeHelper.GetCurrentWeekRangeNow();

            Guid? schoolId = null;

            var targetRoles = new List<string> { EnumRole.AdminSchool.ToString(), EnumRole.TeacherCampus.ToString(), EnumRole.AdminCampus.ToString() };

            var hasMatchedRole = _authContext.Roles != null && _authContext.Roles.Any(r => targetRoles.Contains(r));
            if (hasMatchedRole)
            {
                schoolId = (await _userService.GetSchoolIdAsync()).Content?.Result;
            }

            var query = _studentGoalAggregateRepository.Queryable.Where(x => x.IsActive);
            if (schoolId.HasValue)
            {
                query = query.Where(x => x.SchoolId == schoolId);
            }

            if (request.CourseType.HasValue)
            {
                query = query.Where(x => x.CourseType == request.CourseType);
            }

            if (request.SchoolId.HasValue)
            {
                query = query.Where(x => x.SchoolId == request.SchoolId);
            }

            if (request.CombinedProgress.HasValue)
            {
                query = query.Where(x => x.CombinedProgress == request.CombinedProgress);
            }

            if (!string.IsNullOrEmpty(request.ClassIdStr))
            {
                var classIds = request.ClassIdStr.ToList<Guid>();
                query = query.WhereBulkContains(classIds, x => x.ClassId);
            }

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                var studentResult = await _userService.SearchStudentAsync(new SearchStudentsQueryModel { Keyword = request.Keyword });
                var studentId = studentResult.Content?.Result?.Items?.FirstOrDefault()?.Id;
                query = query.Where(x => x.StudentId == studentId);
            }

            var queryData = from baseQ in query
                join sum in _studentGoalSummaryRepository.Queryable.AsNoTracking() on baseQ.Id equals sum.StudentGoalAggregateId
                where sum.StartDate.Date <= weekStartUtc && sum.EndDate.Date >= weekEndUtc
                select new StudentGoalAggregateModel
                {
                    Id = baseQ.Id,
                    ClassName = baseQ.ClassName,
                    CombinedProgress = baseQ.CombinedProgress,
                    TotalCompletedLessons = baseQ.TotalCompletedLessons,
                    CreatedFullName = baseQ.CreatedFullName,
                    CreatedDate = baseQ.CreatedDate,
                    CourseType = baseQ.CourseType,
                    CourseLevel = baseQ.CourseLevel,
                    CourseId = baseQ.CourseId,
                    ConsecutiveBehindWeeks = baseQ.ConsecutiveBehindWeeks,
                    CompletedLessons = sum.CompletedLessons,
                    CreatedUserId = baseQ.CreatedUserId,
                    StudentId = baseQ.StudentId,
                    ProgressStatus = sum.ProgressStatus,
                    UpdatedDate = baseQ.UpdatedDate,
                    UpdatedFullName = baseQ.UpdatedFullName,
                    UpdatedUserId = baseQ.UpdatedUserId,
                    TotalTargetLessons = sum.TotalTargetLessons,
                    LessonsPerWeek = sum.LessonsPerWeek,
                };

            var lists = await queryData
                .AsNoTracking()
                .ToListAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            var studentIds = lists.Select(l => l.StudentId).ToList();
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(studentIds);
            var students = studentResults.Content?.Result;

            var studentGoalAggregateIds = lists.Select(x => x.Id).ToList();
            var summarys = await _studentGoalSummaryRepository.Queryable
                .AsNoTracking()
                .WhereBulkContains(studentGoalAggregateIds, x => x.StudentGoalAggregateId)
                .ToListAsync(cancellationToken);

            var summarySumMap = summarys.GroupBy(s => s.StudentGoalAggregateId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.LessonsPerWeek)); // hoặc x.TotalPercent

            foreach (var item in lists)
            {
                var student = students?.FirstOrDefault(x => x.Id == item.StudentId);
                item.FullName = student?.User?.FullName;
                item.Email = student?.User?.Email;
                item.UserId = student?.User?.Id;
                item.ClassCampusCode = student?.ClassCampusCode;
                item.StudentCampusCode = student?.StudentCampusCode;
                item.PhoneNumber = student?.User?.PhoneNumber;
                item.StatusStudentCampus = student?.StatusStudentCampus;
                if (summarySumMap.TryGetValue(item.Id, out var totalScore))
                {
                    item.IsActive = totalScore <= item.TotalTargetLessons; // hoặc logic khác tùy ngưỡng bạn muốn
                }
            }

            if (!string.IsNullOrEmpty(request.ClassCampusCode))
            {
                lists = lists.Where(l => l.ClassCampusCode == request.ClassCampusCode).ToList();
            }

            if (!string.IsNullOrEmpty(request.StudentCampusCode))
            {
                lists = lists.Where(l => l.StudentCampusCode == request.StudentCampusCode).ToList();
            }

            if (request.StatusStudentCampus != null)
            {
                lists = lists.Where(l => l.StatusStudentCampus == request.StatusStudentCampus).ToList();
            }

            IEnumerable<StudentGoalAggregateModel> ordered = lists;
            ordered = ordered.OrderByDescending(l => l.TotalCompletedLessons);
            if (!string.IsNullOrEmpty(request.SortCompletedLessons))
            {
                if (request.SortCompletedLessons.Equals("desc", StringComparison.OrdinalIgnoreCase))
                {
                    ordered = ordered.OrderByDescending(l => l.TotalCompletedLessons);
                }
                else
                {
                    ordered = ordered.OrderBy(l => l.TotalCompletedLessons);
                }
            }
            if (!string.IsNullOrEmpty(request.SortCompletedConfig))
            {
                if (request.SortCompletedConfig.Equals("desc", StringComparison.OrdinalIgnoreCase))
                {
                    ordered = ordered.OrderByDescending(l => l.TotalTargetLessons);
                }
                else
                {
                    ordered = ordered.OrderBy(l => l.TotalTargetLessons);
                }
            }
            if (!string.IsNullOrEmpty(request.SortSlowProgress))
            {
                if (request.SortSlowProgress.Equals("desc", StringComparison.OrdinalIgnoreCase))
                {
                    ordered = ordered.OrderByDescending(l => l.ConsecutiveBehindWeeks);
                }
                else
                {
                    ordered = ordered.OrderBy(l => l.ConsecutiveBehindWeeks);
                }
            }
            if (!string.IsNullOrEmpty(request.SortDir))
            {
                if (request.SortDir.Equals("za", StringComparison.OrdinalIgnoreCase))
                {
                    ordered = ordered.OrderByDescending(l => l.FullName ?? string.Empty);
                }
                else
                {
                    ordered = ordered.OrderBy(l => l.FullName ?? string.Empty);
                }
            }

            var totalItem = ordered.Count();

            var pagedLists = ordered
                .AsQueryable()
                .ApplyPaging(request)
                .ToList();

            methodResult.Result = new PagingItemsModel<StudentGoalAggregateModel>(pagedLists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
