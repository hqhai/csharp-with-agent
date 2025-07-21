// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserReferrals
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Managers;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchDetailReferralCodeQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<SearchDetailReferralCodeModel>>>
    {
        public Guid SenderId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class SearchDetailReferralCodeQueryHandler : IRequestHandler<SearchDetailReferralCodeQuery, MethodResult<PagingItemsModel<SearchDetailReferralCodeModel>>>
    {
        private readonly IUserReferralRepository _userReferralRepository;
        private readonly UserManager<User> _userManager;

        public SearchDetailReferralCodeQueryHandler(IUserReferralRepository userReferralRepository, UserManager<User> userManager)
        {
            _userReferralRepository = userReferralRepository;
            _userManager = userManager;
        }

        public async Task<MethodResult<PagingItemsModel<SearchDetailReferralCodeModel>>> Handle(SearchDetailReferralCodeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<SearchDetailReferralCodeModel>>();

            var userReferrals = await _userReferralRepository.Queryable.Where(p => p.SenderId == request.SenderId).ToListAsync(cancellationToken);
            var receiverIds = userReferrals.Select(p => p.ReceiverId).ToList();
            var users = await _userManager.Users.WhereBulkContains(receiverIds, p => p.Id).ToListAsync(cancellationToken);

            var models = new List<SearchDetailReferralCodeModel>();

            foreach (var userReferral in userReferrals)
            {
                var user = users.FirstOrDefault(p => p.Id == userReferral.ReceiverId);

                if (userReferral.FeatureMissions != null && userReferral.FeatureMissions.Count > 0)
                {
                    foreach (var item in userReferral.FeatureMissions)
                    {
                        models.Add(new SearchDetailReferralCodeModel
                        {
                            ReceiverId = userReferral.ReceiverId,
                            Email = user?.Email,
                            FullName = user?.FullName,
                            CreatedDate = item.CreatedDate.ConvertTimeFromUtc(EnumCountryKey.Vietnam),
                            Type = userReferral.Type,
                            Token = item.Token,
                            UserReferral = item.FeatureUserReferral
                        });
                    }
                }
                else
                {
                    models.Add(new SearchDetailReferralCodeModel
                    {
                        ReceiverId = userReferral.ReceiverId,
                        Email = user?.Email,
                        FullName = user?.FullName,
                        CreatedDate = userReferral.CreatedDate.ConvertTimeFromUtc(EnumCountryKey.Vietnam),
                        Type = userReferral.Type,
                        Token = 0,
                        UserReferral = null
                    });
                }
            }

            if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                models = models.Where(p => p.CreatedDate.Date >= request.StartDate.Value.Date && p.CreatedDate.Date <= request.EndDate.Value.Date).ToList();
            }

            int totalItem = models.Count;
            var lists = models
                    .ApplySortAndPaging(request)
                    .ToList();

            methodResult.Result = new PagingItemsModel<SearchDetailReferralCodeModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
