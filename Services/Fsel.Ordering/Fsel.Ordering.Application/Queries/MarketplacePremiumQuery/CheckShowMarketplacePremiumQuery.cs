namespace Fsel.Ordering.Application.Queries.MarketplacePremiumQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class CheckShowMarketplacePremiumQuery : IRequest<MethodResult<bool>>
    {
    }

    public class CheckShowMarketplacePremiumQueryHandler : IRequestHandler<CheckShowMarketplacePremiumQuery, MethodResult<bool>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IPackageRepository _packageRepository;
        private readonly AuthContext _authContext;
        private readonly AppSetting _appSetting;

        public CheckShowMarketplacePremiumQueryHandler(IOrderRepository orderRepository, AuthContext authContext, AppSetting appSetting, IPackageRepository packageRepository)
        {
            _orderRepository = orderRepository;
            _authContext = authContext;
            _appSetting = appSetting;
            _packageRepository = packageRepository;
        }

        public async Task<MethodResult<bool>> Handle(CheckShowMarketplacePremiumQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            methodResult.Result = false;

            var startDate = _appSetting.MarketplacePremiumConfig?.StartDate;
            var endDate = _appSetting.MarketplacePremiumConfig?.EndDate;
            var packages = _appSetting.MarketplacePremiumConfig?.Packages;

            if (!startDate.HasValue || !endDate.HasValue || packages == null || !packages.Any())
            {
                return methodResult;
            }

            var orders = await (from o in _orderRepository.Queryable
                                join p in _packageRepository.Queryable on o.PackageId equals p.Id
                                where o.UserId == _authContext.CurrentUserId && !o.IsTrial && o.Status == EnumOrderStatus.Payment && o.RevenueType == EnumPaymentRevenueType.Revenue
                                select new
                                {
                                    Order = o,
                                    Package = p
                                }).ToListAsync(cancellationToken);

            if (orders != null && orders.Any(p => packages.Contains(p.Package.MonthNumber) && (p.Order.UpdatedDate.HasValue ? (p.Order.UpdatedDate.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam) >= startDate && p.Order.UpdatedDate.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam) <= endDate) : (p.Order.CreatedDate.ConvertTimeFromUtc(EnumCountryKey.Vietnam) >= startDate && p.Order.CreatedDate.ConvertTimeFromUtc(EnumCountryKey.Vietnam) <= endDate))))
            {
                methodResult.Result = true;
                return methodResult;
            }

            return methodResult;
        }
    }
}
