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
    public class GetListNotificationQuery : SearchNotificationModel, IRequest<MethodResult<PagingItemsModel<NotificationsModel>>>
    {
    }

    public class GetListNotificationQueryQueryHandler : IRequestHandler<GetListNotificationQuery, MethodResult<PagingItemsModel<NotificationsModel>>>
    {
        private readonly IMapper _mapper;
        private readonly INotificationsRepository _notificationsRepository;

        public GetListNotificationQueryQueryHandler(IMapper mapper, INotificationsRepository notificationsRepository)
        {
            _mapper = mapper;
            _notificationsRepository = notificationsRepository;
        }

        public async Task<MethodResult<PagingItemsModel<NotificationsModel>>> Handle(GetListNotificationQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<NotificationsModel>>();

            var notificationQuery = _notificationsRepository.Queryable.Include(x => x.NotificationType)
                                                                      .Where(x => x.Status == EnumNotificationStatus.Sent
                                                                            && x.NotificationType!.Type == request.Type)
                                                                      .Select(x => new NotificationsModel
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



            if (request.UserId != Guid.Empty)
            {
                notificationQuery = notificationQuery.Where(m => m.UserId == request.UserId);
            }

            if (request.RoleId != Guid.Empty)
            {
                notificationQuery = notificationQuery.Where(m => m.RoleId == request.RoleId);
            }

            int totalItem = await notificationQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await notificationQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<NotificationsModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
