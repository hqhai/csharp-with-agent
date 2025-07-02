// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.Products
{
    using System;
    using System.Collections.Concurrent;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Caching;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Commands.MarketplacePremiumCmd;
    using Fsel.Ordering.Application.Queues.Publishers;
    using Fsel.Ordering.Application.Services.UserService;
    using Fsel.Ordering.Application.Services.UserService.Models;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Products;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class RedeemProductCommand : RedeemProductCommandModel, IRequest<MethodResult<string>>
    {
    }

    // Tạo static class để quản lý các lock
    public static class ProductRedeemLockManager
    {
        // Lưu trữ các khóa theo key user_product
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new ConcurrentDictionary<string, SemaphoreSlim>();

        // Thời gian chờ tối đa để lấy khóa
        private const int LOCK_TIMEOUT_SECONDS = 10;

        public static async Task<IDisposable> AcquireLockAsync(string key, CancellationToken cancellationToken)
        {
            // Tạo hoặc lấy semaphore cho key
            var semaphore = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

            // Thử lấy lock với timeout
            bool lockTaken = await semaphore.WaitAsync(TimeSpan.FromSeconds(LOCK_TIMEOUT_SECONDS), cancellationToken);

            if (!lockTaken)
            {
                throw new TimeoutException($"Không thể lấy khóa cho key {key} trong {LOCK_TIMEOUT_SECONDS} giây");
            }

            // Trả về một disposable để release lock khi xong
            return new LockReleaser(semaphore, key);
        }

        // Lớp IDisposable để release lock khi xong
        private class LockReleaser : IDisposable
        {
            private readonly SemaphoreSlim _semaphore;
            private readonly string _key;
            private bool _disposed = false;

            public LockReleaser(SemaphoreSlim semaphore, string key)
            {
                _semaphore = semaphore;
                _key = key;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _semaphore.Release();
                    // Xóa khóa khỏi dictionary nếu không có ai đang chờ
                    if (_semaphore.CurrentCount == 1)
                    {
                        _locks.TryRemove(_key, out _);
                    }
                    _disposed = true;
                }
            }
        }
    }

    public class RedeemProductCommandHandler : IRequestHandler<RedeemProductCommand, MethodResult<string>>
    {
        private readonly IUserService _userService;
        private readonly IProductRepository _productRepository;
        private readonly AuthContext _authContext;
        private readonly IOrderTransactionRepository _orderTransactionRepository;
        private readonly CreateTokenHistoryPublisher _createTokenHistoryPublisher;
        private readonly ICacheService<StudentModel> _cacheService;
        private readonly ILogger<RedeemProductPremiumCommandHandler> _logger;

        public RedeemProductCommandHandler(
            IUserService userService,
            IProductRepository productRepository,
            AuthContext authContext,
            IOrderTransactionRepository orderTransactionRepository,
            CreateTokenHistoryPublisher createTokenHistoryPublisher,
            ICacheService<StudentModel> cacheService,
            ILogger<RedeemProductPremiumCommandHandler> logger)
        {
            _userService = userService;
            _productRepository = productRepository;
            _authContext = authContext;
            _orderTransactionRepository = orderTransactionRepository;
            _createTokenHistoryPublisher = createTokenHistoryPublisher;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<MethodResult<string>> Handle(RedeemProductCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<string>();

            // Tạo key độc nhất cho mỗi cặp product-user
            var lockKey = $"redeem_product_{request.ProductId}_{_authContext.CurrentUserId}";

            try
            {
                // Lấy khóa cho transaction này
                using (await ProductRedeemLockManager.AcquireLockAsync(lockKey, cancellationToken))
                {
                    // Kiểm tra các điều kiện ban đầu
                    if (string.IsNullOrEmpty(request.PhoneNumber))
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumProductErrorCode.EmptyPhoneNumber), nameof(request.PhoneNumber), request.PhoneNumber);
                        return methodResult;
                    }

                    if (!request.PhoneNumber.IsValidPhoneNumber())
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumProductErrorCode.PhoneNumberIsInvalid), nameof(request.PhoneNumber), request.PhoneNumber);
                        return methodResult;
                    }

                    // Lấy thông tin sản phẩm
                    var product = await _productRepository.Queryable
                        .Include(p => p.OrderTransactions)
                        .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

                    if (product == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumProductErrorCode.ProductNotExist));
                        return methodResult;
                    }

                    // Kiểm tra xem người dùng đã có transaction đang xử lý chưa
                    var pendingTransaction = product.OrderTransactions
                        .FirstOrDefault(t =>
                            t.Status == EnumOrderTransactionStatus.Requested &&
                            t.RequestBody != null &&
                            JsonSerializer.Deserialize<RedeemProductCommandModel>(
                                t.RequestBody.ToString())?.PhoneNumber == request.PhoneNumber);

                    if (pendingTransaction != null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumProductErrorCode.TransactionInProgress));
                        return methodResult;
                    }

                    // Kiểm tra các event
                    var eventResults = await _userService.GetEventByUserId(null);
                    if (!eventResults.IsSuccessStatusCode)
                    {
                        methodResult.AddError(eventResults.Error);
                        return methodResult;
                    }

                    var events = eventResults.Content?.Result;
                    events = events?.Where(p => p.EventContent != null &&
                                            p.EventContent.Actions != null &&
                                            p.EventContent.Actions.Contains(EnumSchoolEventRuleAction.ShowStoreFSEL))
                                .ToList();

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

                    // Kiểm tra hạn sử dụng
                    var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);
                    if (product.ExpireDate < currentDate)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumProductErrorCode.ExchangeExpirationDate));
                        return methodResult;
                    }

                    // Kiểm tra số lượng còn lại
                    var quantityChanged = product.OrderTransactions
                        .Where(p => p.Status == EnumOrderTransactionStatus.Requested ||
                                    p.Status == EnumOrderTransactionStatus.Received)
                        .Count();

                    if (quantityChanged >= product.Quantity)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumProductErrorCode.OutOfQuantity));
                        return methodResult;
                    }

                    // Kiểm tra token của người dùng
                    var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
                    if (!studentResult.IsSuccessStatusCode)
                    {
                        methodResult.AddError(studentResult.Error);
                        return methodResult;
                    }

                    var student = studentResult.Content?.Result;

                    if (student == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                        return methodResult;
                    }

                    var key = $"StudentRedeemProduct_{_authContext.CurrentUserId}";

                    var studentRedeemProductCache = await _cacheService.GetAsync(key);
                    if (studentRedeemProductCache != null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumProductErrorCode.TransactionInProgress));
                        return methodResult;
                    }

                    await _cacheService.SetAsync(key, student, TimeSpan.FromSeconds(5));

                    if (student.NumberOfToken < product.Price)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumProductErrorCode.NotEnoughTokens), nameof(student.NumberOfToken), student.NumberOfToken);
                        return methodResult;
                    }

                    // Tạo mã code duy nhất
                    var codes = await _orderTransactionRepository.Queryable
                        .Where(p => !string.IsNullOrEmpty(p.Code))
                        .Select(p => p.Code)
                        .ToListAsync(cancellationToken);

                    string code;
                    do
                    {
                        code = Shared.Helpers.NumberHelper.GenerateCode(7);
                    } while (codes.Contains(code));

                    // Thực hiện transaction
                    await _productRepository.ExecuteTransactionAsync(async () =>
                    {
                        var result = await _userService.DeductCoinOfStudent(new DeductCoinOfStudentCommandModel()
                        {
                            UserId = student?.UserId ?? default,
                            NumberOfCoinsDeducted = product.Price,
                            Feature = EnumTokenFeature.MarketPlace,
                            Mission = EnumTokenMission.FselStore,
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

                        // Tạo transaction
                        var orderTransaction = _orderTransactionRepository.Add(new OrderTransaction
                        {
                            Type = EnumOrderTransactionType.Product,
                            Status = EnumOrderTransactionStatus.Requested,
                            Code = code,
                            ProductId = product.Id,
                            RequestBody = request
                        });

                        await _orderTransactionRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

                        //// Tạo token history
                        //var tokenHistoryTranslations = product.Translations.Select(item => new TokenHistoryTranslationModel
                        //{
                        //    Language = item.Language,
                        //    Config = new List<object>
                        //    {
                        //        new { Title = item.Name }
                        //    }
                        //}).ToList();

                        //var language = RegionHelper.GetCountry(EnumCountryKey.Vietnam)?.CultureCode;
                        //var configDefault = tokenHistoryTranslations.FirstOrDefault(x => x.Language == language);

                        //var tokenHistories = new List<TokenHistoryQueueModel>
                        //{
                        //    new TokenHistoryQueueModel
                        //    {
                        //        ObjectId = product.Id,
                        //        VolatileToken = product.Price,
                        //        Feature = EnumTokenFeature.MarketPlace,
                        //        Type = EnumTokenHistoryType.Exchanged,
                        //        UserId = _authContext.CurrentUserId,
                        //        Config = configDefault?.Config ?? tokenHistoryTranslations.FirstOrDefault()?.Config,
                        //        TokenHistoryTranslations = tokenHistoryTranslations,
                        //        Mission = EnumTokenMission.FselStore
                        //    }
                        //};

                        //await _createTokenHistoryPublisher.Publish(tokenHistories, cancellationToken);

                        methodResult.StatusCode = StatusCodes.Status200OK;
                        methodResult.Result = code;
                        return methodResult;
                    });

                    return methodResult;
                }
            }
            catch (TimeoutException)
            {
                // Nếu không lấy được khóa sau thời gian chờ
                methodResult.AddErrorBadRequest(nameof(EnumProductErrorCode.TransactionInProgress));
                return methodResult;
            }
            catch (Exception ex)
            {
                return methodResult;
            }
        }
    }
}
