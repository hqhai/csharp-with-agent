// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.TokenHistoryQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Enums;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.System.Domain.Models.QueryModels;
    using global::System;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchTokenHistoryQuery : SearchTokenHistoryQueryModel, IRequest<MethodResult<PagingItemsModel<TokenHistoryListModel>>>
    {
    }

    public class GetListTokenHistoryQueryHandler : IRequestHandler<SearchTokenHistoryQuery, MethodResult<PagingItemsModel<TokenHistoryListModel>>>
    {
        private readonly ITokenHistoryRepository _tokenHistoryRepository;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;

        public GetListTokenHistoryQueryHandler(ITokenHistoryRepository tokenHistoryRepository, AuthContext authContext, IMapper mapper)
        {
            _tokenHistoryRepository = tokenHistoryRepository;
            _authContext = authContext;
            _mapper = mapper;
        }

        public async Task<MethodResult<PagingItemsModel<TokenHistoryListModel>>> Handle(SearchTokenHistoryQuery request, CancellationToken cancellationToken)
        {
            MethodResult<PagingItemsModel<TokenHistoryListModel>> methodResult = new MethodResult<PagingItemsModel<TokenHistoryListModel>>();
            ArgumentNullException.ThrowIfNull(request);
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            if (request.StartDate != null && request.StartDate!.Value.Year > DateTime.UtcNow.Year + 2)
            {
                methodResult.AddErrorBadRequest(nameof(EnumTokenHistory.StartDateYearIsOverdue));
                return methodResult;
            }

            if (request.EndDate != null && request.EndDate!.Value.Year > DateTime.UtcNow.Year + 2)
            {
                methodResult.AddErrorBadRequest(nameof(EnumTokenHistory.EndDateYearIsOverdue));
                return methodResult;
            }

            var userId = request.UserId ?? _authContext.CurrentUserId;

            var query = _tokenHistoryRepository.Queryable.Include(x => x.Translations)
                                                    .Where(x => x.UserId == userId && (!request.CourseResultId.HasValue || x.CourseResultId == request.CourseResultId))
                                                    .Where(x => !request.Type.HasValue || x.Type == request.Type)
                                                    .Where(x => !request.StartDate.HasValue || x.CreatedDate.Date >= request.StartDate.Value.Date)
                                                    .Where(x => !request.EndDate.HasValue || x.CreatedDate.Date <= request.EndDate.Value.Date)
                                                    .GroupBy(x => x.CreatedDate.Date);
            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var exeQuery = await query.AsNoTracking()
                                   .ToListAsync(cancellationToken: cancellationToken)
                                   .ConfigureAwait(false);
            var lists = exeQuery.Select(x => new TokenHistoryListModel
            {
                Date = x.Key,
                Features = x.GroupBy(x => x.Feature).Select(y => new FeatureModel
                {
                    Feature = y.Key,
                    InitialToken = y.Sum(x => x.InitialToken),
                    VolatileToken = y.Sum(x => x.VolatileToken),
                    RemainToken = y.Sum(x => x.RemainToken),
                    Type = y.Select(x => x.Type).FirstOrDefault(),
                    TokenHistories = y.Select(tokenHistory => _mapper.Map<TokenHistoryModel>(tokenHistory)).ToList(),
                }).ToList(),
            }).ToList();

            methodResult.Result = new PagingItemsModel<TokenHistoryListModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
