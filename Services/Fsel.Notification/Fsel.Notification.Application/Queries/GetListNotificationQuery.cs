using Fsel.Notification.Domain.IRepositories;
using Fsel.Notification.Domain.Model.EntityModels;
using Fsel.Notification.Domain.Model.QueryModels;
using Fsel.Common.ActionResults;
using Fsel.Core.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Fsel.Core.Base;
using Fsel.Notification.Application.Services;
using Fsel.Notification.Application.Services.Models;
using AutoMapper;
using Fsel.Core.Base.BaseModels;
using Fsel.Shared.Enums;

namespace Fsel.Notification.Application.Queries
{
    public class GetListNotificationQuery : SearchNotificationModel, IRequest<MethodResult<PagingItemsNotificationModel>>
    {
    }

    public class GetListNotificationQueryQueryHandler : IRequestHandler<GetListNotificationQuery, MethodResult<PagingItemsNotificationModel>>
    {
        private readonly INotificationsRepository _notificationsRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public GetListNotificationQueryQueryHandler(INotificationsRepository notificationsRepository, AuthContext authContext, IUserService userService, IMapper mapper)
        {
            _notificationsRepository = notificationsRepository;
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<MethodResult<PagingItemsNotificationModel>> Handle(GetListNotificationQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsNotificationModel>();

            var notificationQuery = _notificationsRepository.Queryable.Include(x => x.NotificationType)
                                                                      .Include(x => x.Translations)
                                                                      .Where(x => x.UserId == _authContext.CurrentUserId);
            var notificationSenderIds = await notificationQuery.Where(p => p.SenderId.HasValue).Select(x => x.SenderId ?? default).Distinct().ToListAsync(cancellationToken);

            var listSender = await _userService.GetUsersByIdsAsync(new GetUsersByIdsQueryModel { UserIds = notificationSenderIds });

            var listSenderInfo = listSender?.Content?.Result;

            if (request.Status != null)
            {
                notificationQuery = notificationQuery.Where(m => m.Status == request.Status);
            }

            int totalItem = await notificationQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            int totalRead = await notificationQuery.Where(m => m.Status == EnumNotificationStatus.Read).CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            int totalSent = await notificationQuery.Where(m => m.Status == EnumNotificationStatus.Sent).CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            var notificationResults = await notificationQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var lists = notificationResults.Select(x => _mapper.Map<NotificationMessageModel>(x)).ToList();



            // Gán lại AvatarPath cho các notificationMessage có người gửi
            if (listSenderInfo != null)
            {
                foreach (var notify in lists)
                {
                    notify.AvatarPath = listSenderInfo.FirstOrDefault(x => notify.SenderId.HasValue && x.Id == notify.SenderId)?.AvatarPath;
                    notify.Content = notify.NotificationType?.Content ?? default;
                    notify.Type = notify.NotificationType?.Type ?? default;
                }
            }

            var result = new PagingItemsModel<NotificationMessageModel>(lists, request, totalItem);

            methodResult.Result = new PagingItemsNotificationModel
            {
                Items = result.Items,
                PagingInfo = result.PagingInfo,
                TotalRead = totalRead,
                TotalSent = totalSent
            };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
