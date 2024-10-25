// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.Products
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteProductsCommand : IRequest<MethodResult<bool>>
    {
        public IList<Guid>? ProductIds { get; set; }
    }

    public class DeleteProductsCommandHandler : IRequestHandler<DeleteProductsCommand, MethodResult<bool>>
    {
        private readonly IProductRepository _productRepository;

        public DeleteProductsCommandHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteProductsCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (request.ProductIds == null || request.ProductIds.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            var products = await _productRepository.Queryable.Include(p => p.OrderTransactions).Include(p => p.Translations).Where(p => request.ProductIds.Contains(p.Id)).ToListAsync(cancellationToken);

            if (products == null || products.Count != request.ProductIds.Count)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            if (products.Any(p => p.OrderTransactions.Count > 0))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist));
                return methodResult;
            }
            await _productRepository.ExecuteTransactionAsync(async () =>
            {
                await _productRepository.DeleteListAsync(products);
                await _productRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
