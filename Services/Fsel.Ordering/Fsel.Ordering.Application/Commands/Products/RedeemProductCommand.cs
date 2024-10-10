// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.Products
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Services.UserService;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Products;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class RedeemProductCommand : RedeemProductCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class RedeemProductCommandHandler : IRequestHandler<RedeemProductCommand, MethodResult<bool>>
    {
        private readonly IUserService _userService;
        private readonly IProductRepository _productRepository;
        private readonly AuthContext _authContext;

        public RedeemProductCommandHandler(IUserService userService, IProductRepository productRepository, AuthContext authContext)
        {
            _userService = userService;
            _productRepository = productRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<bool>> Handle(RedeemProductCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var product = await _productRepository.Queryable.Include(p => p.OrderTransactions).FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);
            if (product == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            return methodResult;
        }
    }
}
