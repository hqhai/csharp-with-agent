// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentGoalSummaryQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchStudentGoalSummaryQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<StudentGoalSummaryModel>>>
    {
        public Guid CourseId { get; set; }
        public Guid StudentId { get; set; }
    }

    public class SearchStudentGoalSummaryQueryHandler : IRequestHandler<SearchStudentGoalSummaryQuery, MethodResult<PagingItemsModel<StudentGoalSummaryModel>>>
    {
        private readonly IStudentGoalAggregateRepository _studentGoalAggregateRepository;
        private readonly IStudentGoalSummaryRepository _studentGoalSummaryRepository;

        public SearchStudentGoalSummaryQueryHandler(IStudentGoalAggregateRepository studentGoalAggregateRepository,
            IStudentGoalSummaryRepository studentGoalSummaryRepository)
        {
            _studentGoalAggregateRepository = studentGoalAggregateRepository;
            _studentGoalSummaryRepository = studentGoalSummaryRepository;
        }

        public async Task<MethodResult<PagingItemsModel<StudentGoalSummaryModel>>> Handle(SearchStudentGoalSummaryQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<StudentGoalSummaryModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var query = _studentGoalAggregateRepository.ReadQueryable.Where(x => x.IsActive && x.StudentId == request.StudentId && x.CourseId == request.CourseId);

            var queryData = from baseQ in query
                            join sum in _studentGoalSummaryRepository.ReadQueryable on baseQ.Id equals sum.StudentGoalAggregateId
                            select new StudentGoalSummaryModel
                            {
                                Id = sum.Id,
                                CompletedLessons = sum.CompletedLessons,
                                CreatedDate = sum.CreatedDate,
                                CreatedFullName = sum.CreatedFullName,
                                CreatedUserId = sum.CreatedUserId,
                                EndDate = sum.EndDate,
                                LastCompletedAt = sum.LastCompletedAt,
                                LessonsPerWeek = sum.LessonsPerWeek,
                                ProgressStatus = sum.ProgressStatus,
                                StartDate = sum.StartDate,
                                UpdatedDate = sum.UpdatedDate,
                                UpdatedFullName = sum.UpdatedFullName,
                                UpdatedUserId = sum.UpdatedUserId,
                                CourseId = baseQ.CourseId,
                                StudentId = baseQ.StudentId,
                            };
            var totalItem = await queryData.CountAsync(cancellationToken);
            var lists = await queryData.OrderBy(x => x.StartDate).ApplyPaging(request)
                             .AsNoTracking()
                             .ToListAsync(cancellationToken: cancellationToken)
                             .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<StudentGoalSummaryModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
