// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.Products
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Products;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SaveProductCommand : SaveProductCommandModel, IRequest<MethodResult<ProductModel>>
    {
    }

    public class SaveProductCommandHandler : IRequestHandler<SaveProductCommand, MethodResult<ProductModel>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public SaveProductCommandHandler(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<ProductModel>> Handle(SaveProductCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ProductModel>();

            if (!string.IsNullOrEmpty(request.Code) && !StringHelper.ContainsWhitespaceOrSpecialChars(request.Code))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                return methodResult;
            }

            if (request.Translations == null || request.Translations.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            if (request.ExpireDate.Date < DateTime.UtcNow.Date)
            {
                methodResult.AddErrorBadRequest(nameof(EnumProductErrorCode.WrongExpirationDate));
                return methodResult;
            }

            if (request.Id.HasValue)
            {
                var product = await _productRepository.Queryable.Include(p => p.OrderTransactions).FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
                if (product == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }
                if (_productRepository.Queryable.Any(p => p.Code == request.Code && p.Id != request.Id))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumProductErrorCode.CodeAlreadyExists));
                    return methodResult;
                }
                //if (_productRepository.Queryable.Any(p => p.Name.ToLower() == request.Name.ToLower() && p.Id != request.Id))
                //{
                //    methodResult.AddErrorBadRequest(nameof(EnumProductErrorCode.NameAlreadyExists));
                //    return methodResult;
                //}
                var quantityChanged = product.OrderTransactions.Where(p => p.Status == EnumOrderTransactionStatus.Requested || p.Status == EnumOrderTransactionStatus.Received).Count();
                if (request.Quantity < quantityChanged)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumProductErrorCode.WrongQuantity));
                    return methodResult;
                }
                _mapper.Map(request, product);
                if (!product.IsValid())
                {
                    methodResult.AddError(product.ErrorMessages);
                    return methodResult;
                }
                await _productRepository.ExecuteTransactionAsync(async () =>
                {
                    product = _productRepository.Update(product);
                    await _productRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    methodResult.StatusCode = StatusCodes.Status200OK;
                    methodResult.Result = _mapper.Map<ProductModel>(product);
                    return methodResult;
                });
            }
            else
            {
                if (_productRepository.Queryable.Any(p => p.Code == request.Code))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist));
                    return methodResult;
                }
                //if (_productRepository.Queryable.Any(p => p.Name.ToLower() == request.Name.ToLower()))
                //{
                //    methodResult.AddErrorBadRequest(nameof(EnumProductErrorCode.NameAlreadyExists));
                //    return methodResult;
                //}

                var product = _mapper.Map<Product>(request);
                product.MarketPlaceType = EnumMarketPlaceType.FSEL;
                if (!product.IsValid())
                {
                    methodResult.AddError(product.ErrorMessages);
                    return methodResult;
                }
                await _productRepository.ExecuteTransactionAsync(async () =>
                {
                    product = _productRepository.Add(product);
                    await _productRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    methodResult.StatusCode = StatusCodes.Status200OK;
                    methodResult.Result = _mapper.Map<ProductModel>(product);
                    return methodResult;
                });
            }
            return methodResult;
        }
    }
}
