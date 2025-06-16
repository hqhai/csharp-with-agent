namespace Fsel.Ordering.Application.Queries.MarketplacePremiumQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class CheckShowMarketplacePremiumQuery : IRequest<MethodResult<CheckShowMarketplacePremiumModel>>
    {
    }

    public class CheckShowMarketplacePremiumQueryHandler : IRequestHandler<CheckShowMarketplacePremiumQuery, MethodResult<CheckShowMarketplacePremiumModel>>
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

        public async Task<MethodResult<CheckShowMarketplacePremiumModel>> Handle(CheckShowMarketplacePremiumQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CheckShowMarketplacePremiumModel>();

            var model = new CheckShowMarketplacePremiumModel();

            var startDate = _appSetting.MarketplacePremiumConfig?.StartDate;
            var endDate = _appSetting.MarketplacePremiumConfig?.EndDate;
            var startDateButton = _appSetting.MarketplacePremiumConfig?.StartDateButton;
            var endDateButton = _appSetting.MarketplacePremiumConfig?.EndDateButton;

            if (!startDate.HasValue || !endDate.HasValue || !startDateButton.HasValue || !endDateButton.HasValue)
            {
                return methodResult;
            }

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);
            if (startDateButton.Value <= currentDate && endDateButton >= currentDate)
            {
                model.IsShowButton = true;
            }

            var orders = await (from o in _orderRepository.Queryable
                                join p in _packageRepository.Queryable on o.PackageId equals p.Id
                                where o.UserId == _authContext.CurrentUserId && !o.IsTrial && o.Status == EnumOrderStatus.Payment && o.RevenueType == EnumPaymentRevenueType.Revenue
                                select new
                                {
                                    Order = o,
                                    Package = p
                                }).ToListAsync(cancellationToken);

            if (orders != null)
            {
                foreach (var orderItem in orders)
                {
                    DateTime orderDate = orderItem.Order.UpdatedDate.HasValue
                        ? orderItem.Order.UpdatedDate.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam)
                        : orderItem.Order.CreatedDate.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

                    bool isInDateRange = orderDate >= startDate && orderDate <= endDate;

                    if (isInDateRange)
                    {
                        model.IsUserPremium = true;
                        break;
                    }
                }
            }
            methodResult.Result = model;
            return methodResult;
        }
    }
}
