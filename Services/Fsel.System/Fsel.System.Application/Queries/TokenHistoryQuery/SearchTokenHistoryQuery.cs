// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.TokenHistoryQuery
{
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

        public GetListTokenHistoryQueryHandler(ITokenHistoryRepository tokenHistoryRepository, AuthContext authContext)
        {
            _tokenHistoryRepository = tokenHistoryRepository;
            _authContext = authContext;
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

            var tokenHistorys = _tokenHistoryRepository.Queryable.AsQueryable();

            if (request.StartDate != null || request.EndDate != null)
            {
                tokenHistorys = tokenHistorys.Where(m =>
                                    (request.StartDate == null || m.CreatedDate.Date >= request.StartDate.Value.Date) &&
                                    (request.EndDate == null || m.CreatedDate.Date <= request.EndDate.Value.Date));
            }

            if (request.Type != null)
            {
                tokenHistorys = tokenHistorys.Where(x => x.Type == request.Type);
            }

            var tokenHistoryQuery = tokenHistorys.Where(x => x.UserId == userId && (!request.CourseResultId.HasValue || x.CourseResultId == request.CourseResultId))
                            .GroupBy(x => x.CreatedDate.Date)
                            .OrderByDescending(x => x.Key)
                            .Select(x => new TokenHistoryListModel
                            {
                                Date = x.Key,
                                Features = x.GroupBy(x => x.Feature).Select(x => new FeatureModel
                                {
                                    Feature = x.Key,
                                    InitialToken = x.Sum(x => x.InitialToken),
                                    VolatileToken = x.Sum(x => x.VolatileToken),
                                    RemainToken = x.Sum(x => x.RemainToken),
                                    Type = x.Select(x => x.Type).FirstOrDefault(),
                                    TokenHistories = x.Select(x => new TokenHistoryModel
                                    {
                                        Id = x.Id,
                                        TokenConfigId = x.TokenConfigId,
                                        Config = x.Config,
                                        ConfigData = x.ConfigData,
                                        CreatedDate = x.CreatedDate,
                                        CreatedFullName = x.CreatedFullName,
                                        CreatedUserId = x.UserId,
                                        Feature = x.Feature,
                                        InitialToken = x.InitialToken,
                                        VolatileToken = x.VolatileToken,
                                        RemainToken = x.RemainToken,
                                        Mission = x.Mission,
                                        ObjectId = x.ObjectId,
                                        Type = x.Type,
                                        UserId = x.UserId,
                                    }).ToList(),
                                }).ToList(),
                            });

            int totalItem = await tokenHistoryQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await tokenHistoryQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<TokenHistoryListModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
