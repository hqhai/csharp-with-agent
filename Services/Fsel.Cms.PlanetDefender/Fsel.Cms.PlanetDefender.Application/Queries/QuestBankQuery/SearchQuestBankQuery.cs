// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queries.QuestBankQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Cms.PlanetDefender.Application.Services.SystemServices;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Cms.PlanetDefender.Domain.Models.QueryModels.QuestBanks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

    public class SearchQuestBankQuery : SearchQuestBankQueryModel, IRequest<MethodResult<PagingItemsModel<QuestBankModel>>>
    {
    }
    public class SearchQuestBankQueryHandler : IRequestHandler<SearchQuestBankQuery, MethodResult<PagingItemsModel<QuestBankModel>>>
    {
        private readonly IQuestBankRepository _questBankRepository;
        private readonly ISystemService _systemService;

        public SearchQuestBankQueryHandler(IQuestBankRepository questBankRepository, ISystemService systemService)
        {
            _questBankRepository = questBankRepository;
            _systemService = systemService;
        }

        public async Task<MethodResult<PagingItemsModel<QuestBankModel>>> Handle(SearchQuestBankQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<QuestBankModel>> methodResult = new MethodResult<PagingItemsModel<QuestBankModel>>();
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var query = _questBankRepository.Queryable.
            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            /*var platformsResult = await _userService.GetAllPlatform();
            if (!platformsResult.IsSuccessStatusCode || platformsResult.Content?.Result == null)
            {
                methodResult.AddError(platformsResult.Error);
                return methodResult;
            }
            var platforms = platformsResult.Content.Result;

            lists.ForEach(p =>
            {
                p.PlatformName = platforms.FirstOrDefault(x => x.Id == p.PlatformId)?.Name;
            });*/

            methodResult.Result = new PagingItemsModel<QuestBankModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
