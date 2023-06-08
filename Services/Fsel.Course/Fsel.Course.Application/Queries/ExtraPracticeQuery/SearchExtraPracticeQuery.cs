// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.ExtraPracticeQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ExtraPractices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchExtraPracticeQuery : SearchExtraPracticeQueryModel, IRequest<MethodResult<PagingItemsModel<ExtraPracticeModel>>>
    {
    }

    public class SearchExtraPracticeQueryHandler : IRequestHandler<SearchExtraPracticeQuery, MethodResult<PagingItemsModel<ExtraPracticeModel>>>
    {
        private readonly IExtraPracticeRepository _extraPracticeRepository;

        public SearchExtraPracticeQueryHandler(IExtraPracticeRepository extraPracticeRepository)
        {
            _extraPracticeRepository = extraPracticeRepository;
        }

        public async Task<MethodResult<PagingItemsModel<ExtraPracticeModel>>> Handle(SearchExtraPracticeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<ExtraPracticeModel>> methodResult = new MethodResult<PagingItemsModel<ExtraPracticeModel>>();
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var extraPracticeQuery = _extraPracticeRepository.Queryable
                                                .Select(x => new ExtraPracticeModel
                                                {
                                                    Id = x.Id,
                                                    Name = x.Name,
                                                    Code = x.Code,
                                                    CourseLevel = x.CourseLevel,
                                                    Type = x.Type,
                                                    CreatedDate = x.CreatedDate,
                                                    CreatedFullName = x.CreatedFullName,
                                                    IsActive = x.IsActive,
                                                });
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                extraPracticeQuery = extraPracticeQuery.Where(m => m.Id.ToString() == request.Keyword || (m.Name ?? string.Empty).Contains(request.Keyword));
            }
            if (request.CourseLevel != null)
            {
                extraPracticeQuery = extraPracticeQuery.Where(m => m.CourseLevel == request.CourseLevel);
            }
            if (request.Type != null)
            {
                extraPracticeQuery = extraPracticeQuery.Where(m => m.Type == request.Type);
            }
            int totalItem = await extraPracticeQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await extraPracticeQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            methodResult.Result = new PagingItemsModel<ExtraPracticeModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
