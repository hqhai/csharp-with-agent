// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentAggregateQuery
{
    using AutoMapper;
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
        private readonly IMapper _mapper;

        public SearchStudentGoalAggregateQueryHandler(IStudentGoalAggregateRepository studentGoalAggregateRepository,
            IStudentGoalSummaryRepository studentGoalSummaryRepository,
            IUserService userService,
            AuthContext authContext,
            IMapper mapper)
        {
            _studentGoalAggregateRepository = studentGoalAggregateRepository;
            _studentGoalSummaryRepository = studentGoalSummaryRepository;
            _userService = userService;
            _authContext = authContext;
            _mapper = mapper;
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

            var targetRoles = new List<string> { nameof(EnumRole.AdminSchool), nameof(EnumRole.TeacherCampus), nameof(EnumRole.AdminCampus) };

            var hasMatchedRole = _authContext.Roles != null && _authContext.Roles.Any(r => targetRoles.Contains(r));
            if (hasMatchedRole)
            {
                schoolId = (await _userService.GetSchoolIdAsync()).Content?.Result;
            }

            var query = _studentGoalAggregateRepository.ReadQueryable.Where(x => x.IsActive);
            if (schoolId.HasValue)
            {
                query = query.Where(x => x.SchoolId == schoolId);
            }

            if (request.LevelId.HasValue)
            {
                query = query.Where(x => x.LevelId == request.LevelId);
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
                if (classIds != null && classIds.Any())
                {
                    query = query.WhereBulkContains(classIds, x => x.ClassId);
                }
                else
                {
                    return methodResult;
                }
            }
            if (!string.IsNullOrEmpty(request.ProgramIdStr))
            {
                var programIds = request.ProgramIdStr.ToList<Guid>();
                if (programIds != null && programIds.Any())
                {
                    query = query.WhereBulkContains(programIds, x => x.ProgramId);
                }
                else
                {
                    return methodResult;
                }
            }

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                var studentResult = await _userService.SearchStudentAsync(new SearchStudentsQueryModel { Keyword = request.Keyword });
                var studentId = studentResult.Content?.Result?.Items?.FirstOrDefault()?.Id;
                query = query.Where(x => x.StudentId == studentId);
            }

            var queryData = from baseQ in query
                            join sum in _studentGoalSummaryRepository.ReadQueryable.AsNoTracking() on baseQ.Id equals sum.StudentGoalAggregateId
                            where sum.StartDate.Date <= weekStartUtc && sum.EndDate.Date >= weekEndUtc
                            select new StudentGoalAggregateModel
                            {
                                Id = baseQ.Id,
                                ClassName = baseQ.ClassName,
                                CombinedProgress = baseQ.CombinedProgress,
                                TotalCompletedLessons = baseQ.TotalCompletedLessons,
                                CreatedFullName = baseQ.CreatedFullName,
                                CreatedDate = baseQ.CreatedDate,
                                LevelId = baseQ.LevelId,
                                CourseResultId = baseQ.CourseResultId,
                                ProgramId = baseQ.ProgramId,
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
                                Level = _mapper.Map<LevelModel>(baseQ.Level),
                                Program = _mapper.Map<CategoryModel>(baseQ.Program),
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
                .ToDictionary(g => g.Key, g => g.Sum(x => x.LessonsPerWeek));

            foreach (var item in lists)
            {
                var student = students?.FirstOrDefault(x => x.Id == item.StudentId);
                item.FullName = student?.User?.FullName;
                item.Email = student?.User?.Email;
                item.UserId = student?.UserId;
                item.ClassCampusCode = student?.ClassCampusCode;
                item.StudentCampusCode = student?.StudentCampusCode;
                item.PhoneNumber = student?.User?.PhoneNumber;
                item.StatusStudentCampus = student?.StatusStudentCampus;
                if (summarySumMap.TryGetValue(item.Id, out var totalScore))
                {
                    item.IsActive = totalScore <= item.TotalTargetLessons;
                }
            }

            if (!string.IsNullOrEmpty(request.ClassCampusCode))
            {
                var classCampusCodes = request.ClassCampusCode.ToList<string>();

                lists = lists
                    .Where(l => !string.IsNullOrEmpty(l.ClassCampusCode) && classCampusCodes.Contains(l.ClassCampusCode!))
                    .ToList();
            }

            if (request.StatusStudentCampus != null && request.StatusStudentCampus.Any())
            {
                var statusList = request.StatusStudentCampus;
                ;
                lists = lists.Where(l => l.StatusStudentCampus.HasValue && statusList.Contains(l.StatusStudentCampus.Value))
                    .ToList();
            }

            if (!string.IsNullOrEmpty(request.StudentCampusCode))
            {
                lists = lists.Where(l => l.StudentCampusCode == request.StudentCampusCode).ToList();
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
