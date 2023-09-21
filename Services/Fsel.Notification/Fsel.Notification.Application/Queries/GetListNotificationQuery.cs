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
using AutoMapper;

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
        private readonly IMapper _mapper;

        public GetListNotificationQueryQueryHandler(INotificationsRepository notificationsRepository, AuthContext authContext, IUserService userService, IMapper mapper)
        {
            _notificationsRepository = notificationsRepository;
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<MethodResult<PagingItemsModel<NotificationMessageModel>>> Handle(GetListNotificationQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<NotificationMessageModel>>();

            var notificationQuery = _notificationsRepository.Queryable.Include(x => x.NotificationType)
                                                                      .Where(x => x.UserId == _authContext.CurrentUserId);
            var notificationSenderIds = notificationQuery.Where(p => p.SenderId.HasValue).Select(x => x.SenderId.ToString() ?? string.Empty).Distinct().ToList();

            var listSender = await _userService.GetUsersByIdsAsync(new GetUsersByIdsQueryModel { UserIds = notificationSenderIds });

            var listSenderInfo = listSender?.Content?.Result;

            var notificationResultQuery = notificationQuery.Select(x => _mapper.Map<NotificationMessageModel>(x));

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
