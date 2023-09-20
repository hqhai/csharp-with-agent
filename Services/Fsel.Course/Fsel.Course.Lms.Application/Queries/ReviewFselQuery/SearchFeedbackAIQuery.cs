// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReviewFselQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ReviewFsels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchFeedbackAIQuery : SearchFeedbackAIQueryModel, IRequest<MethodResult<PagingItemsModel<FeedbackClassForumAIModel>>>
    {
    }

    public class SearchFeedbackAIQueryHandler : IRequestHandler<SearchFeedbackAIQuery, MethodResult<PagingItemsModel<FeedbackClassForumAIModel>>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IUnitLessonRepository _unitLessonRepository;
        private readonly IStudentFeedbackRepository _studentFeedbackRepository;
        private readonly IClassForumRepository _classForumRepository;

        public SearchFeedbackAIQueryHandler(ICourseRepository courseRepository, IClassForumResultRepository classForumResultRepository, ILessonResultRepository lessonResultRepository, ILessonRepository lessonRepository, ICourseUnitMockTestRepository courseUnitMockTestRepository, IUnitRepository unitRepository, IUnitLessonRepository unitLessonRepository, IStudentFeedbackRepository studentFeedbackRepository, IClassForumRepository classForumRepository)
        {
            _courseRepository = courseRepository;
            _classForumResultRepository = classForumResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _lessonRepository = lessonRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
            _unitRepository = unitRepository;
            _unitLessonRepository = unitLessonRepository;
            _studentFeedbackRepository = studentFeedbackRepository;
            _classForumRepository = classForumRepository;
        }

        public async Task<MethodResult<PagingItemsModel<FeedbackClassForumAIModel>>> Handle(SearchFeedbackAIQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<FeedbackClassForumAIModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var query = from baseQ in _lessonRepository.Queryable
                        join cf in _classForumRepository.Queryable on baseQ.Id equals cf.LessonId
                        join ul in _unitLessonRepository.Queryable on baseQ.Id equals ul.LessonId
                        join u in _unitRepository.Queryable on ul.UnitId equals u.Id
                        join cmt in _courseUnitMockTestRepository.Queryable on u.Id equals cmt.UnitId
                        join c in _courseRepository.Queryable on cmt.CourseId equals c.Id
                        join lr in _lessonResultRepository.Queryable on new { LessonId = baseQ.Id, UnitId = u.Id, CourseId = c.Id } equals new { lr.LessonId, lr.UnitId, lr.CourseId }
                        join cfr in _classForumResultRepository.Queryable on lr.Id equals cfr.LessonResultId
                        join s in _studentFeedbackRepository.Queryable on cfr.Id equals s.ObjectId
                        select new FeedbackClassForumAIModel
                        {
                            CourseId = c.Id,
                            CourseName = c.Name,
                            Code = c.Code,
                            UnitId = u.Id,
                            LessonId = baseQ.Id,
                            ClassForumId = cf.Id,
                            NumberOfStarts = s.FeedBackStars ?? default,
                            UnitDisplayOrder = cmt.DisplayOrder,
                            LessonDisplayOrder = ul.DisplayOrder,
                        };

            var groupedQuery = (from result in query
                                group result by new { result.LessonId, result.UnitId, result.CourseId, result.ClassForumId } into grouped
                                select new FeedbackClassForumAIModel
                                {
                                    ClassForumId = grouped.Key.ClassForumId,
                                    CourseId = grouped.Key.CourseId,
                                    LessonId = grouped.Key.LessonId,
                                    UnitId = grouped.Key.UnitId,
                                    CourseName = grouped.First().CourseName,
                                    Code = grouped.First().Code,
                                    NumberOfStarts = Math.Round(grouped.Select(x => x.NumberOfStarts).Average(), 0),
                                    TotalRating = grouped.Select(x => x.NumberOfStarts).Where(x => x <= 2).Count()
                                });
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                groupedQuery = groupedQuery.Where(m => (m.CourseName ?? string.Empty).Contains(request.Keyword));
            }

            if (request.NumberOfStarts != null)
            {
                groupedQuery = groupedQuery.Where(m => m.NumberOfStarts == request.NumberOfStarts);
            }

            int totalItem = await groupedQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await groupedQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<FeedbackClassForumAIModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
