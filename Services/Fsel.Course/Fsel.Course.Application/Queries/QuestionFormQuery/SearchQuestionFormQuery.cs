// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.QuestionFormQuery
{
    using System.Globalization;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.QuestionForms;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchQuestionFormQuery : SearchQuestionFormQueryModel, IRequest<MethodResult<PagingItemsModel<QuestionFormModel>>>
    {
    }

    public class SearchQuestionFormQueryHandler : IRequestHandler<SearchQuestionFormQuery, MethodResult<PagingItemsModel<QuestionFormModel>>>
    {
        private readonly IQuestionFormRepository _questionFormRepository;

        public SearchQuestionFormQueryHandler(IQuestionFormRepository questionFormRepository)
        {
            _questionFormRepository = questionFormRepository;
        }

        public async Task<MethodResult<PagingItemsModel<QuestionFormModel>>> Handle(SearchQuestionFormQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<QuestionFormModel>> methodResult = new MethodResult<PagingItemsModel<QuestionFormModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var questionFormQuery = _questionFormRepository.Queryable.Select
                                        (x => new QuestionFormModel
                                        {
                                            Id = x.Id,
                                            Name = x.Name,
                                            CreatedDate = x.CreatedDate,
                                            Type = x.Type,
                                            Config = x.Config
                                        });

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                questionFormQuery = questionFormQuery.Where(m => m.Id.ToString() == request.Keyword || (m.Name ?? string.Empty).ToLower().Trim().Contains(request.Keyword.ToLower().Trim()));
            }

            int totalItem = await questionFormQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await questionFormQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<QuestionFormModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
