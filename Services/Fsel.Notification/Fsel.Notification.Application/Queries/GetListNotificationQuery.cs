using Fsel.Notification.Domain.IRepositories;
using Fsel.Notification.Domain.Model.EntityModels;
using Fsel.Notification.Domain.Model.QueryModels;
using Fsel.Common.ActionResults;
using Fsel.Core.Base.BaseModels;
using Fsel.Core.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Fsel.Core.Base;
using Fsel.Notification.Application.Services;
using Fsel.Notification.Application.Services.UserServices;
using Fsel.Notification.Application.Services.Models;

namespace Fsel.Notification.Application.Queries
{
    public class GetListNotificationQuery : SearchNotificationModel, IRequest<MethodResult<PagingItemsModel<NotificationMessageModel>>>
    {
    }

    public class GetListNotificationQueryQueryHandler : IRequestHandler<GetListNotificationQuery, MethodResult<PagingItemsModel<NotificationMessageModel>>>
    {
        private readonly INotificationsRepository _notificationsRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        public GetListNotificationQueryQueryHandler(INotificationsRepository notificationsRepository, AuthContext authContext, IUserService userService)
        {
            _notificationsRepository = notificationsRepository;
            _authContext = authContext;
            _userService = userService;
        }

        public async Task<MethodResult<PagingItemsModel<NotificationMessageModel>>> Handle(GetListNotificationQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<NotificationMessageModel>>();

            var notificationQuery = _notificationsRepository.Queryable.Include(x => x.NotificationType)
                                                                      .Where(x => x.UserId == _authContext.CurrentUserId);
            var notificationSenderIds = notificationQuery.Select(x => x.SenderId).ToList();

            //Tạo 1 HashSet để loại bỏ những phần tử trùng.
            HashSet<Guid?> uniqueSenderGuids = new HashSet<Guid?>(notificationSenderIds);
            IList<string> uniqueStringSenderList = new List<string>();

            // Chuyển sang kiểu Ilist<string> để truy vấn dữ liệu của listUserIds
            if (uniqueSenderGuids.Count > 0)
            {
                uniqueStringSenderList = uniqueSenderGuids!.Select(guid => guid.ToString()).ToList()!;
            }
            var listSender = await _userService.GetUsersByIdsAsync(new GetUsersByIdsQueryModel { UserIds = uniqueStringSenderList });

            var listSenderInfo = listSender?.Content?.Result;

            var notificationResultQuery = notificationQuery.Select(x => new NotificationMessageModel
            {
                Id = x.Id,
                UserId = x.UserId,
                RoleId = x.RoleId,
                Status = x.Status,
                Message = x.Message,
                AvatarPath = string.Empty,
                Link = x.Link,
                ObjectId = x.ObjectId,
                CreatedDate = x.CreatedDate,
                CreatedUserId = x.CreatedUserId,
                CreatedFullName = x.CreatedFullName,
                NotificationTypeId = x.NotificationTypeId,
                SenderId = x.SenderId
            });

            if (request.Status != null)
            {
                notificationResultQuery = notificationResultQuery.Where(m => m.Status == request.Status);
            }

            int totalItem = await notificationResultQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await notificationResultQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            // Gán lại AvatarPath cho các notificationMessage có người gửi
            if (listSenderInfo != null)
            {
                foreach (var notify in lists)
                {
                    notify.AvatarPath = listSenderInfo.FirstOrDefault(x => notify.SenderId.HasValue && x.UserId == notify.SenderId.ToString())?.AvatarPath;
                }
            }

            methodResult.Result = new PagingItemsModel<NotificationMessageModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
