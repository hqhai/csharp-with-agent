// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.Products
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Queues.Publishers;
    using Fsel.Ordering.Application.Services.UserService;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Products;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class RedeemProductCommand : RedeemProductCommandModel, IRequest<MethodResult<string>>
    {
    }

    public class RedeemProductCommandHandler : IRequestHandler<RedeemProductCommand, MethodResult<string>>
    {
        private readonly IUserService _userService;
        private readonly IProductRepository _productRepository;
        private readonly AuthContext _authContext;
        private readonly IOrderTransactionRepository _orderTransactionRepository;
        private readonly CreateTokenHistoryPublisher _createTokenHistoryPublisher;

        public RedeemProductCommandHandler(IUserService userService, IProductRepository productRepository, AuthContext authContext, IOrderTransactionRepository orderTransactionRepository, CreateTokenHistoryPublisher createTokenHistoryPublisher)
        {
            _userService = userService;
            _productRepository = productRepository;
            _authContext = authContext;
            _orderTransactionRepository = orderTransactionRepository;
            _createTokenHistoryPublisher = createTokenHistoryPublisher;
        }

        public async Task<MethodResult<string>> Handle(RedeemProductCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<string>();

            var product = await _productRepository.Queryable.Include(p => p.OrderTransactions).FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);
            if (product == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var eventResults = await _userService.GetEventByUserId(null);
            if (!eventResults.IsSuccessStatusCode)
            {
                methodResult.AddError(eventResults.Error);
                return methodResult;
            }
            var events = eventResults.Content?.Result;

            events = events?.Where(p => p.EventContent != null && p.EventContent.Actions != null && p.EventContent.Actions.Contains(EnumSchoolEventRuleAction.ShowStoreFSEL)).ToList();

            var eventIds = events?.Select(e => e.Id).ToList();

            if (eventIds == null || eventIds.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumProductErrorCode.NotPartOfTheEvent));
                return methodResult;
            }

            if (product.EventIds == null || product.EventIds.Count == 0 || !product.EventIds.Any(a => eventIds.Contains(a)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumProductErrorCode.NotPartOfTheEvent));
                return methodResult;
            }

            if (product.ExpireDate.Date < DateTime.UtcNow.Date)
            {
                methodResult.AddErrorBadRequest(nameof(EnumProductErrorCode.ExchangeExpirationDate));
                return methodResult;
            }

            var quantityChanged = product.OrderTransactions.Where(p => p.Status == EnumOrderTransactionStatus.Requested || p.Status == EnumOrderTransactionStatus.Received).Count();

            if (quantityChanged >= product.Quantity)
            {
                methodResult.AddErrorBadRequest(nameof(EnumProductErrorCode.OutOfQuantity));
                return methodResult;
            }

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return methodResult;
            }
            var student = studentResult.Content?.Result;

            var token = student?.NumberOfToken ?? 0;

            if (token < product.Price)
            {
                methodResult.AddErrorBadRequest(nameof(EnumProductErrorCode.NotEnoughTokens));
                return methodResult;
            }

            var codes = await _orderTransactionRepository.Queryable.Where(p => !string.IsNullOrEmpty(p.Code)).Select(p => p.Code).ToListAsync(cancellationToken);
            string code;
            do
            {
                code = Shared.Helpers.NumberHelper.GenerateCode(7);
            } while (codes.Contains(code));

            await _productRepository.ExecuteTransactionAsync(async () =>
            {
                var orderTransaction = _orderTransactionRepository.Add(new OrderTransaction() { Type = EnumOrderTransactionType.Product, Status = EnumOrderTransactionStatus.Requested, Code = code, ProductId = product.Id, RequestBody = request });
                await _orderTransactionRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                var configs = new List<object> { new
                {
                    Id = product.Id,
                    Title = product.Name,
                    Price = product.Price
                }};
                var tokenHistories = new List<TokenHistoryQueueModel>
                {
                    new TokenHistoryQueueModel
                    {
                        ObjectId = product.Id,
                        VolatileToken = product.Price,
                        Feature = EnumTokenFeature.MarketPlace,
                        Type = EnumTokenHistoryType.Exchanged,
                        UserId = _authContext.CurrentUserId,
                        Config = configs,
                        Mission = EnumTokenMission.FselStore
                    }
                };
                await _createTokenHistoryPublisher.Publish(tokenHistories, cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = code;
                return methodResult;
            });

            return methodResult;
        }
    }
}
