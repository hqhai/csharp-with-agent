// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReviewFselQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ReviewFsels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchStudentFeedbackAIQuery : SearchStudentFeedbackAIQueryModel, IRequest<MethodResult<PagingItemsModel<StudentFeedbackModel>>>
    {
    }

    public class SearchStudentFeedbackAIQueryHandler : IRequestHandler<SearchStudentFeedbackAIQuery, MethodResult<PagingItemsModel<StudentFeedbackModel>>>
    {
        private readonly IStudentFeedbackRepository _studentFeedbackRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;

        public SearchStudentFeedbackAIQueryHandler(IStudentFeedbackRepository studentFeedbackRepository, IClassForumRepository classForumRepository, IClassForumResultRepository classForumResultRepository)
        {
            _studentFeedbackRepository = studentFeedbackRepository;
            _classForumRepository = classForumRepository;
            _classForumResultRepository = classForumResultRepository;
        }

        public async Task<MethodResult<PagingItemsModel<StudentFeedbackModel>>> Handle(SearchStudentFeedbackAIQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<PagingItemsModel<StudentFeedbackModel>>();
            ArgumentNullException.ThrowIfNull(request);
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var query = from baseQ in _studentFeedbackRepository.Queryable
                        join cfr in _classForumResultRepository.Queryable on baseQ.ObjectId equals cfr.Id
                        join cf in _classForumRepository.Queryable on cfr.ClassForumId equals cf.Id
                        where cf.Id == request.ClassForumId && baseQ.Feature == EnumFeature.ClassForum
                        select new StudentFeedbackModel
                        {
                            Id = baseQ.Id,
                            CreatedDate = baseQ.CreatedDate,
                            CreatedFullName = baseQ.CreatedFullName,
                            CreatedUserId = baseQ.CreatedUserId,
                            Feature = baseQ.Feature,
                            FeedBackNegativesStr = baseQ.FeedBackNegativesStr,
                            FeedBackNote = baseQ.FeedBackNote,
                            FeedBackPositivesStr = baseQ.FeedBackPositivesStr,
                            FeedBackStars = baseQ.FeedBackStars,
                            Type = baseQ.Type,
                        };

            if (request.NumberOfStars != null)
            {
                query = query.Where(x => x.FeedBackStars + 0.5 >= request.NumberOfStars && x.FeedBackStars < request.NumberOfStars + 0.5);
            }
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(m => (m.CreatedFullName ?? string.Empty).Contains(request.Keyword));
            }
            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<StudentFeedbackModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
