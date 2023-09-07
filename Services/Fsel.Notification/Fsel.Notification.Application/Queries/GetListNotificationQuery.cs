using AutoMapper;
using Fsel.Notification.Domain.IRepositories;
using Fsel.Notification.Domain.Model.EntityModels;
using Fsel.Notification.Domain.Model.QueryModels;
using Fsel.Shared.Enums;
using Fsel.Common.ActionResults;
using Fsel.Core.Base.BaseModels;
using Fsel.Core.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Notification.Application.Queries
{
    public class GetListNotificationQuery : SearchNotificationModel, IRequest<MethodResult<PagingItemsModel<NotificationMessageModel>>>
    {
    }

    public class GetListNotificationQueryQueryHandler : IRequestHandler<GetListNotificationQuery, MethodResult<PagingItemsModel<NotificationMessageModel>>>
    {
        private readonly INotificationsRepository _notificationsRepository;

        public GetListNotificationQueryQueryHandler(INotificationsRepository notificationsRepository)
        {
            _notificationsRepository = notificationsRepository;
        }

        public async Task<MethodResult<PagingItemsModel<NotificationMessageModel>>> Handle(GetListNotificationQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<NotificationMessageModel>>();

            var notificationQuery = _notificationsRepository.Queryable.Include(x => x.NotificationType)
                                                                      .Where(x => x.Status == EnumNotificationStatus.Sent
                                                                            && x.NotificationType!.Type == request.Type)
                                                                      .Select(x => new NotificationMessageModel
                                                                      {
                                                                          Id = x.Id,
                                                                          UserId = x.UserId,
                                                                          RoleId = x.RoleId,
                                                                          Status = x.Status,
                                                                          Message = x.Message,
                                                                          ObjectId = x.ObjectId,
                                                                          CreatedDate = x.CreatedDate,
                                                                          CreatedUserId = x.CreatedUserId,
                                                                          CreatedFullName = x.CreatedFullName,
                                                                          NotificationTypeId = x.NotificationTypeId,
                                                                          Template = x.NotificationType!.Template
                                                                      });



            if (request.UserId != null)
            {
                notificationQuery = notificationQuery.Where(m => m.UserId == request.UserId);
            }

            if (request.RoleId != null)
            {
                notificationQuery = notificationQuery.Where(m => m.RoleId == request.RoleId);
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
