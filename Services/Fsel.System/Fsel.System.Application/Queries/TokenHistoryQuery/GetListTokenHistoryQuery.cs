// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.TokenHistoryQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Enums;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetListTokenHistoryQuery : IRequest<MethodResult<IList<TokenHistoryListModel>>>
    {
        public Guid? UserId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public EnumTokenHistoryType? Type { get; set; }
    }

    public class GetListTokenHistoryQueryHandler : IRequestHandler<GetListTokenHistoryQuery, MethodResult<IList<TokenHistoryListModel>>>
    {
        private readonly ITokenHistoryRepository _tokenHistoryRepository;
        private readonly AuthContext _authContext;

        public GetListTokenHistoryQueryHandler(ITokenHistoryRepository tokenHistoryRepository, AuthContext authContext)
        {
            _tokenHistoryRepository = tokenHistoryRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<IList<TokenHistoryListModel>>> Handle(GetListTokenHistoryQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<TokenHistoryListModel>>();

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
                                    (request.StartDate == null || m.CreatedDate.Date >= request.StartDate.Value.Date
                                    && m.CreatedDate.Month >= request.StartDate.Value.Month
                                    && m.CreatedDate.Year >= request.StartDate.Value.Year) &&
                                    (request.EndDate == null || m.CreatedDate.Date <= request.EndDate.Value.Date
                                    && m.CreatedDate.Month <= request.EndDate.Value.Month
                                    && m.CreatedDate.Year <= request.EndDate.Value.Year));
            }

            if (request.Type != null)
            {
                tokenHistorys = tokenHistorys.Where(x => x.Type == request.Type);
            }

            var tokenHistoryModel = await tokenHistorys.Where(x => x.UserId == userId)
                            .GroupBy(x => x.CreatedDate.Date)
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
                            }).ToListAsync(cancellationToken);

            methodResult.Result = tokenHistoryModel;
            return methodResult;
        }
    }
}
