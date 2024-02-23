// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.UrBoxQuery
{
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Services.UrBoxService;
    using Fsel.Ordering.Application.Services.UrBoxService.Models.Request;
    using Fsel.Ordering.Application.Services.UrBoxService.Models.Response;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetListExchangeHistoryQuery : IRequest<MethodResult<ExchangeHistoryModel>>
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class GetListExchangeHistoryQueryHandler : IRequestHandler<GetListExchangeHistoryQuery, MethodResult<ExchangeHistoryModel>>
    {
        private readonly IUrBoxService _urBoxService;
        private readonly AppSetting _appSetting;
        private readonly AuthContext _authContext;
        private readonly IUrBoxTransactionRepository _urBoxTransactionRepository;
        private readonly IMapper _mapper;

        public GetListExchangeHistoryQueryHandler(IUrBoxService urBoxService, AppSetting appSetting, AuthContext authContext, IUrBoxTransactionRepository urBoxTransactionRepository, IMapper mapper)
        {
            _urBoxService = urBoxService;
            _appSetting = appSetting;
            _authContext = authContext;
            _urBoxTransactionRepository = urBoxTransactionRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<ExchangeHistoryModel>> Handle(GetListExchangeHistoryQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ExchangeHistoryModel>();

            var orderTransactions = await _urBoxTransactionRepository.Queryable.Where(p => p.CreatedUserId == _authContext.CurrentUserId && p.Status == EnumUrBoxTransactionStatus.Success).ToListAsync(cancellationToken);

            var responses = orderTransactions.Select(p => p?.RequestBodyStr).Where(requestBodyStr => requestBodyStr != null).Select(p => _mapper.Map<RedemptionResponseModel>(p)).ToList();

            var gifts = responses.Select(d => d.Data).Select(c => c.Cart).SelectMany(clg => clg.CodeLinkGift).ToList();

            List<Task<List<DetailExchangeHistory>?>> tasks = new List<Task<List<DetailExchangeHistory>?>>();

            foreach (var item in orderTransactions)
            {
                tasks.Add(GetDetailExchangeHistoryAsync(item.Id));
            }

            var detailExchangeHistories = new List<DetailExchangeHistory>();

            await Task.WhenAll(tasks);

            tasks.ForEach(t =>
            {
                if (t.Result != null)
                {
                    detailExchangeHistories.AddRange(t.Result);
                };
            });

            var history = new List<CodeLinkGift>();
            //foreach (var gift in gifts)
            //{
            //    var item = detailExchangeHistories.FirstOrDefault(p => p.Id == gift.CardId)
            //}

            return methodResult;
        }

        private async Task<List<DetailExchangeHistory>?> GetDetailExchangeHistoryAsync(Guid id)
        {
            var detailExchangeHistoryResult = await _urBoxService.GetDetailExchangeHistory(new GetDetailExchangeHistoryModel(_appSetting)
            {
                TransactionId = id.ToString(),
            });

            if (detailExchangeHistoryResult.IsSuccessStatusCode)
            {
                return detailExchangeHistoryResult.Content?.Data?.Detail?.ToList();
            }

            return null;
        }
    }
}
