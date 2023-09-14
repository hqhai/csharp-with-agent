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

namespace Fsel.Notification.Application.Queries
{
    public class GetListNotificationQuery : SearchNotificationModel, IRequest<MethodResult<PagingItemsModel<NotificationMessageModel>>>
    {
    }

    public class GetListNotificationQueryQueryHandler : IRequestHandler<GetListNotificationQuery, MethodResult<PagingItemsModel<NotificationMessageModel>>>
    {
        private readonly INotificationsRepository _notificationsRepository;
        private readonly AuthContext _authContext;

        public GetListNotificationQueryQueryHandler(INotificationsRepository notificationsRepository, AuthContext authContext)
        {
            _notificationsRepository = notificationsRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<PagingItemsModel<NotificationMessageModel>>> Handle(GetListNotificationQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<NotificationMessageModel>>();

            var notificationQuery = _notificationsRepository.Queryable.Include(x => x.NotificationType)
                                                                      .Where(x => x.UserId == _authContext.CurrentUserId)
                                                                      .Select(x => new NotificationMessageModel
                                                                      {
                                                                          Id = x.Id,
                                                                          UserId = x.UserId,
                                                                          RoleId = x.RoleId,
                                                                          Status = x.Status,
                                                                          Message = x.Message,
                                                                          Link = x.Link,
                                                                          ObjectId = x.ObjectId,
                                                                          CreatedDate = x.CreatedDate,
                                                                          CreatedUserId = x.CreatedUserId,
                                                                          CreatedFullName = x.CreatedFullName,
                                                                          NotificationTypeId = x.NotificationTypeId,
                                                                      });

            if (request.Status != null)
            {
                notificationQuery = notificationQuery.Where(m => m.Status == request.Status);
            }

            int totalItem = await notificationQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await notificationQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<NotificationMessageModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
