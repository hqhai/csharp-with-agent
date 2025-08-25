// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Application.Queries.ExamPracticeQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetHistoryExamPracticeQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<ExamPracticeModel>>>
    {
        public Guid OriginalId { get; set; }


        public class GetHistoryExamPracticeQueryHandler : IRequestHandler<GetHistoryExamPracticeQuery, MethodResult<PagingItemsModel<ExamPracticeModel>>>
        {
            private readonly IExamPracticeRepository _examPracticeRepository;

            public GetHistoryExamPracticeQueryHandler(IExamPracticeRepository examPracticeRepository)
            {
                _examPracticeRepository = examPracticeRepository;
            }

            public async Task<MethodResult<PagingItemsModel<ExamPracticeModel>>> Handle(GetHistoryExamPracticeQuery request, CancellationToken cancellationToken)
            {
                ArgumentNullException.ThrowIfNull(request);
                MethodResult<PagingItemsModel<ExamPracticeModel>> methodResult = new MethodResult<PagingItemsModel<ExamPracticeModel>>();

                var query = _examPracticeRepository.Queryable
                                                   .Where(x => x.OriginalId == request.OriginalId && !x.IsArchive)
                                                   .OrderByDescending(x => x.Version)
                                                   .AsNoTracking();

                return await _examPracticeRepository.GetListByPageResultAsync<ExamPracticeModel>(query, request, cancellationToken);
            }
        }
    }
}
