// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentAggregateQuery
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.StudentProgress;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchStudentGoalAggregateQuery : SearchStudentGoalAggregateQueryModel, IRequest<MethodResult<PagingItemsModel<StudentGoalAggregateModel>>>
    {
    }

    public class SearchStudentGoalAggregateQueryHandler : IRequestHandler<SearchStudentGoalAggregateQuery, MethodResult<PagingItemsModel<StudentGoalAggregateModel>>>
    {
        private readonly IStudentGoalAggregateRepository _studentGoalAggregateRepository;
        private readonly IStudentGoalSummaryRepository _studentGoalSummaryRepository;
        private readonly IUserService _userService;

        public SearchStudentGoalAggregateQueryHandler(IStudentGoalAggregateRepository studentGoalAggregateRepository,
            IStudentGoalSummaryRepository studentGoalSummaryRepository,
            IUserService userService)
        {
            _studentGoalAggregateRepository = studentGoalAggregateRepository;
            _studentGoalSummaryRepository = studentGoalSummaryRepository;
            _userService = userService;
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
            var nowUtc = DateTime.UtcNow;
            int diff = ((int)nowUtc.DayOfWeek + 6) % 7;
            var weekStartUtc = nowUtc.Date.AddDays(-diff).Date;
            var weekEndUtc = weekStartUtc.AddDays(7).Date;

            var query = _studentGoalAggregateRepository.Queryable;
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
                var studentResult = await _userService.GetStudentByEmailAsync(request.Keyword);
                var studentId = studentResult.Content?.Result?.Id;

                query = query.Where(x => x.StudentId == studentId);
            }

            var queryData = from baseQ in query
                            join sum in _studentGoalSummaryRepository.Queryable.AsNoTracking() on baseQ.Id equals sum.StudentGoalAggregateId
                            where sum.StartDate.Date <= weekEndUtc && sum.EndDate.Date >= weekStartUtc
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
                                UpdatedDate = baseQ.UpdatedDate,
                                UpdatedFullName = baseQ.UpdatedFullName,
                                UpdatedUserId = baseQ.UpdatedUserId,
                                TotalTargetLessons = sum.TotalTargetLessons,
                                LessonsPerWeek = sum.LessonsPerWeek,
                            };
            var totalItem = await queryData.CountAsync(cancellationToken);
            var lists = await queryData.ApplySortAndPaging(request)
                             .AsNoTracking()
                             .ToListAsync(cancellationToken: cancellationToken)
                             .ConfigureAwait(false);
            var studentIds = lists.Select(l => l.StudentId).ToList();
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(studentIds);
            var students = studentResults.Content?.Result;
            foreach (var item in lists)
            {
                var student = students?.FirstOrDefault(x => x.Id == item.StudentId);
                item.FullName = student?.Human?.FullName;
                item.Email = student?.Human?.Email;
            }

            methodResult.Result = new PagingItemsModel<StudentGoalAggregateModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
