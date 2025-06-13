namespace Fsel.Ordering.Application.Commands.MarketplacePremiumCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Caching;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Common.ValueSettings;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Queries.MarketplacePremiumQuery;
    using Fsel.Ordering.Application.Queues.Publishers;
    using Fsel.Ordering.Application.Services.UrBoxService;
    using Fsel.Ordering.Application.Services.UrBoxService.Models.Response;
    using Fsel.Ordering.Application.Services.UserService;
    using Fsel.Ordering.Application.Services.UserService.Models;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.MarketPlacePremium;
    using Fsel.Ordering.Domain.Models.CommandModels.UrBox;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class RedeemProductPremiumCommand : RedeemProductPremiumCommandModel, IRequest<MethodResult<RedemptionResponseModel>>
    {
    }

    public class RedeemProductPremiumCommandHandler : IRequestHandler<RedeemProductPremiumCommand, MethodResult<RedemptionResponseModel>>
    {
        private readonly IUrBoxService _urBoxService;
        private readonly CreateTokenHistoryPublisher _createTokenHistoryPublisher;
        private readonly AppSetting _appSetting;
        private readonly IMediator _mediator;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IOrderTransactionRepository _orderTransactionRepository;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IProductRepository _productRepository;
        private readonly ICacheService<StudentModel> _cacheService;
        private readonly ILogger<RedeemProductPremiumCommandHandler> _logger;

        public RedeemProductPremiumCommandHandler(IUrBoxService urBoxService, CreateTokenHistoryPublisher createTokenHistoryPublisher, AppSetting appSetting, IMediator mediator, AuthContext authContext, IUserService userService, IOrderTransactionRepository orderTransactionRepository, IHostEnvironment hostEnvironment, IProductRepository productRepository, ICacheService<StudentModel> cacheService, ILogger<RedeemProductPremiumCommandHandler> logger)
        {
            _urBoxService = urBoxService;
            _createTokenHistoryPublisher = createTokenHistoryPublisher;
            _appSetting = appSetting;
            _mediator = mediator;
            _authContext = authContext;
            _userService = userService;
            _orderTransactionRepository = orderTransactionRepository;
            _hostEnvironment = hostEnvironment;
            _productRepository = productRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<MethodResult<RedemptionResponseModel>> Handle(RedeemProductPremiumCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<RedemptionResponseModel>();

            if (string.IsNullOrEmpty(request.PhoneNumber) || !request.PhoneNumber.IsValidPhoneNumber())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.PhoneNumber));
                return methodResult;
            }

            var checkResult = await _mediator.Send(new CheckShowMarketplacePremiumQuery(), cancellationToken);
            if (!checkResult.IsOK)
            {
                methodResult.AddError(checkResult.ErrorMessages);
                return methodResult;
            }
            if (!checkResult.Result)
            {
                methodResult.AddErrorBadRequest(nameof(EnumProductErrorCode.NotPartOfTheEvent), nameof(checkResult));
                return methodResult;
            }

            var product = await _productRepository.Queryable.Include(p => p.OrderTransactions).FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);
            if (product == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(product));
                return methodResult;
            }

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);
            if (product.ExpireDate < currentDate)
            {
                methodResult.AddErrorBadRequest(nameof(EnumProductErrorCode.ExchangeExpirationDate));
                return methodResult;
            }

            var quantityChanged = product.OrderTransactions
                .Where(p => p.Status == EnumOrderTransactionStatus.Requested ||
                            p.Status == EnumOrderTransactionStatus.Received || p.Status == EnumOrderTransactionStatus.Success)
                .Count();

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
            if (studentResult.Content?.Result == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var key = $"NumberOfToken_{_authContext.CurrentUserId}";
            var student = _cacheService.Get(key,
                TimeSpan.FromMinutes(1),
                () => studentResult.Content.Result,
                _logger);

            if (student == null)
            {
                student = studentResult.Content.Result;
            }
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            student.NumberOfToken -= product.Price;
            await _cacheService.SetAsync(key, student, TimeSpan.FromSeconds(5));

            var token = student.NumberOfToken;
            if (token < product.Price)
            {
                methodResult.AddErrorBadRequest(nameof(EnumProductErrorCode.NotEnoughTokens), nameof(token), token);
                return methodResult;
            }

            if (product.MarketPlaceType == EnumMarketPlaceType.UrBox && product.IsPremium)
            {
                var result = await _mediator.Send(new RedeemUrboxPremiumCommand()
                {
                    ProductId = product.Id,
                    Price = product.Price,
                    PhoneNumber = request.PhoneNumber,
                    DataBuy = new List<DataBuyCommandModel>()
                    {
                        new DataBuyCommandModel()
                        {
                            PriceId  = product.GlobalId,
                            Quantity = "1"
                        }
                    }
                }, cancellationToken);

                if (!result.IsOK)
                {
                    methodResult.AddError(result.ErrorMessages);
                    return methodResult;
                }
            }
            else if (product.MarketPlaceType == EnumMarketPlaceType.FSEL && product.IsPremium)
            {
                await RedeemProductFSEL(product, student?.Human?.UserId ?? default, request, methodResult, cancellationToken);
                return methodResult;
            }
            else
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(product));
                return methodResult;
            }

            return methodResult;
        }

        private async Task<MethodResult<RedemptionResponseModel>> RedeemProductFSEL(Product product, Guid userId, RedeemProductPremiumCommandModel request, MethodResult<RedemptionResponseModel> methodResult, CancellationToken cancellationToken)
        {
            var codes = await _orderTransactionRepository.Queryable
                .Where(p => !string.IsNullOrEmpty(p.Code))
                .Select(p => p.Code)
                .ToListAsync(cancellationToken);

            string code;
            do
            {
                code = Shared.Helpers.NumberHelper.GenerateCode(7);
            } while (codes.Contains(code));

            await _productRepository.ExecuteTransactionAsync(async () =>
            {
                var result = await _userService.DeductCoinOfStudent(new DeductCoinOfStudentCommandModel()
                {
                    UserId = userId,
                    NumberOfCoinsDeducted = product.Price,
                    Feature = EnumTokenFeature.MarketPlace,
                    ObjectId = product.Id,
                    Translations = product.Translations.Select(p => new TokenHistoryTranslationModel()
                    {
                        Language = p.Language,
                        Config = new List<object>
                        {
                                new { Title = p.Name }
                        }
                    }).ToList()
                });

                if (!result.IsSuccessStatusCode)
                {
                    methodResult.AddError(result.Error);
                    return methodResult;
                }

                var orderTransaction = _orderTransactionRepository.Add(new OrderTransaction
                {
                    Type = EnumOrderTransactionType.ProductPremium,
                    Status = EnumOrderTransactionStatus.Requested,
                    Code = code,
                    ProductId = product.Id,
                    RequestBody = request
                });

                await _orderTransactionRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });

            return methodResult;
        }
    }
}
