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
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchStudentFeedbackQuery : SearchStudentFeedbackQueryModel, IRequest<MethodResult<PagingItemsModel<StudentFeedbackModel>>>
    {
    }

    public class SearchStudentFeedbackQueryHandler : IRequestHandler<SearchStudentFeedbackQuery, MethodResult<PagingItemsModel<StudentFeedbackModel>>>
    {
        private readonly IStudentFeedbackRepository _studentFeedbackRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;

        public SearchStudentFeedbackQueryHandler(IStudentFeedbackRepository studentFeedbackRepository, IClassForumRepository classForumRepository, IClassForumResultRepository classForumResultRepository)
        {
            _studentFeedbackRepository = studentFeedbackRepository;
            _classForumRepository = classForumRepository;
            _classForumResultRepository = classForumResultRepository;
        }

        public async Task<MethodResult<PagingItemsModel<StudentFeedbackModel>>> Handle(SearchStudentFeedbackQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<PagingItemsModel<StudentFeedbackModel>>();
            ArgumentNullException.ThrowIfNull(request);
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var studentFeedback = from baseQ in _studentFeedbackRepository.Queryable
                                  join cfr in _classForumResultRepository.Queryable on baseQ.ObjectId equals cfr.Id
                                  join cf in _classForumRepository.Queryable on cfr.ClassForumId equals cf.Id
                                  where cf.Id == request.ClassForumId
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

            if (request.FeedBackStars != null)
            {
                studentFeedback = studentFeedback.Where(m => m.FeedBackStars == request.FeedBackStars);
            }
            int totalItem = await studentFeedback.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await studentFeedback
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
