// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.ErrorReportQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.System.Application.Services.CourseServices;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.System.Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchErrorReportQuery : SearchErrorReportQueryModel, IRequest<MethodResult<PagingItemsModel<ErrorReportModel>>>
    {
    }

    public class SearchErrorReportQueryHandler : IRequestHandler<SearchErrorReportQuery, MethodResult<PagingItemsModel<ErrorReportModel>>>
    {
        private readonly IErrorReportRepository _errorReportRepository;
        private readonly ICourseService _courseService;

        public SearchErrorReportQueryHandler(IErrorReportRepository errorReportRepository, ICourseService courseService)
        {
            _errorReportRepository = errorReportRepository;
            _courseService = courseService;
        }

        public async Task<MethodResult<PagingItemsModel<ErrorReportModel>>> Handle(SearchErrorReportQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<PagingItemsModel<ErrorReportModel>>();
            ArgumentNullException.ThrowIfNull(request);
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var query = _errorReportRepository.Queryable.Select(x => new ErrorReportModel
            {
                Id = x.Id,
                CreatedDate = x.CreatedDate,
                CourseId = x.CourseId,
                CreatedFullName = x.CreatedFullName,
                CreatedUserId = x.CreatedUserId,
                FeedBack = x.FeedBack,
                ImagePaths = x.ImagePaths,
                FeatureLearn = x.FeatureLearn,
                LessonId = x.LessonId,
                FeaturePlatform = x.FeaturePlatform,
                Priority = x.Priority,
                Status = x.Status,
                Content = x.Content,
                Type = x.Type,
                UnitId = x.UnitId,
                UpdatedDate = x.UpdatedDate,
                Url = x.Url,
            });

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(m => m.CreatedUserId.ToString() == request.Keyword || (m.Content ?? string.Empty).ToLower().Trim().Contains(request.Keyword.ToLower().Trim()));
            }
            if (request.Type != null)
            {
                query = query.Where(m => m.Type == request.Type);
            }

            if (request.CourseId != null)
            {
                query = query.Where(m => m.CourseId == request.CourseId);
            }

            if (request.UnitId != null)
            {
                query = query.Where(m => m.UnitId == request.UnitId);
            }

            if (request.LessonId != null)
            {
                query = query.Where(m => m.LessonId == request.LessonId);
            }

            if (request.Priority != null)
            {
                query = query.Where(m => m.Priority == request.Priority);
            }

            if (request.Status != null)
            {
                query = query.Where(m => m.Status == request.Status);
            }

            if (request.StartDate != null || request.EndDate != null)
            {
                query = query.Where(m =>
                    (request.StartDate == null || m.CreatedDate!.Value.Date >= request.StartDate.Value.Date) &&
                    (request.EndDate == null || m.CreatedDate!.Value.Date <= request.EndDate.Value.Date));
            }
            if (request.FeatureLearn != null)
            {
                query = query.Where(m => m.FeatureLearn == request.FeatureLearn);
            }

            if (request.FeaturePlatform != null)
            {
                query = query.Where(m => m.FeaturePlatform == request.FeaturePlatform);
            }

            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var courseResults = await _courseService.GetListCourseByIds(lists.Select(x => x.CourseId ?? default).ToList());

            var courses = courseResults.Content?.Result;

            var unitResults = await _courseService.GetListUnitByIds(lists.Select(x => x.UnitId ?? default).ToList());

            var units = unitResults.Content?.Result;

            var lessonResults = await _courseService.GetListLessonByIds(lists.Select(x => x.LessonId ?? default).ToList());

            var lessons = lessonResults.Content?.Result;

            foreach (var item in lists)
            {
                if (item.FeatureLearn != null)
                {
                    var course = courses?.FirstOrDefault(x => x.Id == item.CourseId);
                    var unit = units?.FirstOrDefault(x => x.Id == item.UnitId);
                    var lesson = lessons?.FirstOrDefault(x => x.Id == item.LessonId);
                    item.CourseLevel = course.CourseLevel;
                    item.CourseName = course.Name;
                    item.CourseType = course.CourseType;
                    item.UnitName = unit?.Name;
                    item.LessonName = lesson?.Name;
                }
            }

            if (request.CourseType != null)
            {
                lists = lists.Where(m => m.CourseType == request.CourseType).ToList();
            }
            if (request.CourseLevel != null)
            {
                lists = lists.Where(m => m.CourseLevel == request.CourseLevel).ToList();
            }
            methodResult.Result = new PagingItemsModel<ErrorReportModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
