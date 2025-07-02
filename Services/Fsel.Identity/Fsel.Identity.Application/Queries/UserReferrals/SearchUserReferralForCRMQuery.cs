namespace Fsel.Identity.Application.Queries.UserReferrals
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.OrderService.QueryModels;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class SearchUserReferralForCRMQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<UserReferralForCRMModel>>>
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class SearchUserReferralForCRMQueryHandler : IRequestHandler<SearchUserReferralForCRMQuery, MethodResult<PagingItemsModel<UserReferralForCRMModel>>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly UserManager<User> _userManager;
        private readonly IUserReferralRepository _userReferralRepository;
        private readonly IOrderService _orderService;

        public SearchUserReferralForCRMQueryHandler(IStudentRepository studentRepository, UserManager<User> userManager, IUserReferralRepository userReferralRepository, IOrderService orderService)
        {
            _studentRepository = studentRepository;
            _userManager = userManager;
            _userReferralRepository = userReferralRepository;
            _orderService = orderService;
        }

        public async Task<MethodResult<PagingItemsModel<UserReferralForCRMModel>>> Handle(SearchUserReferralForCRMQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<UserReferralForCRMModel>>();

            var query = from u in _userManager.Users
                        join s in _studentRepository.Queryable on u.Id equals s.UserId
                        join ur in _userReferralRepository.Queryable on u.Id equals ur.ReceiverId
                        select new
                        {
                            User = u,
                            Student = s,
                            UserReferral = ur
                        };

            if (request.StartDate.HasValue || request.EndDate.HasValue)
            {
                if (request.StartDate.HasValue && request.EndDate.HasValue)
                {
                    query = query.Where(p => p.UserReferral.CreatedDate.AddHours(7).Date >= request.StartDate.Value.Date && p.UserReferral.CreatedDate.AddHours(7).Date <= request.EndDate.Value.Date);
                }
                else if (request.StartDate.HasValue)
                {
                    query = query.Where(p => p.UserReferral.CreatedDate.AddHours(7).Date >= request.StartDate.Value.Date);
                }
                else if (request.EndDate.HasValue)
                {
                    query = query.Where(p => p.UserReferral.CreatedDate.AddHours(7).Date <= request.EndDate.Value.Date);
                }
            }

            var queryData = query.Select(x => new UserReferralForCRMModel
            {
                Id = x.UserReferral.Id,
                CreatedDate = x.UserReferral.CreatedDate.AddHours(7),
                StudentId = x.Student.Id,
                StudentCode = x.User.Code,
                Name = x.User.FullName,
                SenderId = x.UserReferral.SenderId,
                ReceiveId = x.UserReferral.ReceiverId
            });

            int totalItem = await queryData.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await queryData
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var senderIds = lists.Select(p => p.SenderId).ToList();
            var users = await _userManager.Users.WhereBulkContains(senderIds, p => p.Id).ToListAsync(cancellationToken);

            var receiveIds = lists.Select(p => p.ReceiveId).ToList();
            var orderResults = await _orderService.GetUserHasOrderRevenue(new GetUserHasOrderRevenueModel() { UserIds = receiveIds });
            var orders = orderResults.Content?.Result;

            lists.ForEach(p =>
            {
                var user = users.FirstOrDefault(x => x.Id == p.SenderId);
                p.ReferralCode = user?.Code;
                var order = orders?.FirstOrDefault(x => x.UserId == p.ReceiveId);
                if (order != null)
                {
                    p.Package = $"{order.Package?.MonthNumber} tháng";
                    p.Status = ReferralHistoryStatus.Payment;
                }
                else
                {
                    p.Status = ReferralHistoryStatus.NotPayment;
                }
            });

            methodResult.Result = new PagingItemsModel<UserReferralForCRMModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
