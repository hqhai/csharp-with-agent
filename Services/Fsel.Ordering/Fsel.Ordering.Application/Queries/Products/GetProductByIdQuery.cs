// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.Products
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetProductByIdQuery : IRequest<MethodResult<ProductModel>>
    {
        public Guid ProductId { get; set; }
    }

    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, MethodResult<ProductModel>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public GetProductByIdQueryHandler(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<ProductModel>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ProductModel>();

            var product = await _productRepository.Queryable.Include(p => p.Translations).Include(p => p.OrderTransactions).FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);
            if (product == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var quantityChanged = product.OrderTransactions.Where(p => p.Status == EnumOrderTransactionStatus.Requested || p.Status == EnumOrderTransactionStatus.Received).Count();

            var productModel = _mapper.Map<ProductModel>(product);

            productModel.QuantityChanged = quantityChanged;
            productModel.RemainingQuantity = productModel.Quantity - quantityChanged;

            methodResult.Result = productModel;

            return methodResult;
        }
    }
}
