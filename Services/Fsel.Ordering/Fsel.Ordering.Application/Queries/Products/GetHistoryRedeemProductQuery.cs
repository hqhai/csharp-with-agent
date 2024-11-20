// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.Products
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetHistoryRedeemProductQuery : IRequest<MethodResult<HistoryRedeemProductModels>>
    {
    }

    public class GetHistoryRedeemProductQueryHandler : IRequestHandler<GetHistoryRedeemProductQuery, MethodResult<HistoryRedeemProductModels>>
    {
        private readonly IOrderTransactionRepository _orderTransactionRepository;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;

        public GetHistoryRedeemProductQueryHandler(IOrderTransactionRepository orderTransactionRepository, AuthContext authContext, IMapper mapper)
        {
            _orderTransactionRepository = orderTransactionRepository;
            _authContext = authContext;
            _mapper = mapper;
        }

        public async Task<MethodResult<HistoryRedeemProductModels>> Handle(GetHistoryRedeemProductQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<HistoryRedeemProductModels>();

            var transactions = await _orderTransactionRepository.Queryable.Include(p => p.Product).ThenInclude(p => p.Translations).Where(p => p.CreatedUserId == _authContext.CurrentUserId && p.Type == EnumOrderTransactionType.Product && (p.Status == EnumOrderTransactionStatus.Requested || p.Status == EnumOrderTransactionStatus.Received) && p.ProductId.HasValue).ToListAsync(cancellationToken);

            var result = new HistoryRedeemProductModels();

            transactions.ForEach(p =>
            {
                var model = new HistoryRedeemProductModel()
                {
                    Id = p.Id,
                    Status = p.Status,
                    Code = p.Code,
                    Type = p.Type,
                    RequestBody = p.RequestBody,
                    CreatedDate = p.CreatedDate,
                    UpdatedDate = p.UpdatedDate,
                    Product = p.Product != null ? _mapper.Map<ProductModel>(p.Product) : null,
                };
                if (p.Status == EnumOrderTransactionStatus.Requested)
                {
                    result.Requested.Add(model);
                }
                else if (p.Status == EnumOrderTransactionStatus.Received)
                {
                    result.Received.Add(model);
                }
            });

            result.Requested = result.Requested.OrderByDescending(p => p.CreatedDate).ToList();
            result.Received = result.Received.OrderByDescending(p => p.UpdatedDate).ToList();

            methodResult.Result = result;
            return methodResult;
        }
    }
}
