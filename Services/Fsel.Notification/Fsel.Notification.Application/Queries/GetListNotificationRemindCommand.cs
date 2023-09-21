using Fsel.Notification.Domain.IRepositories;
using Fsel.Notification.Domain.Model.EntityModels;
using Fsel.Common.ActionResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Fsel.Core.Base;
using AutoMapper;
using Fsel.Shared.Enums;

namespace Fsel.Notification.Application.Queries
{
    public class GetListNotificationRemindCommand : IRequest<MethodResult<IList<NotificationRemindModel>>>
    {
        public IList<Guid>? ObjectIds { get; set; }

        public EnumNotificationRemindStatus Status { get; set; }
    }

    public class GetListNotificationRemindQueryQueryHandler : IRequestHandler<GetListNotificationRemindCommand, MethodResult<IList<NotificationRemindModel>>>
    {
        private readonly INotificationRemindRepository _notificationRemindRepository;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;

        public GetListNotificationRemindQueryQueryHandler(AuthContext authContext, IMapper mapper, INotificationRemindRepository notificationRemindRepository)
        {
            _authContext = authContext;
            _mapper = mapper;
            _notificationRemindRepository = notificationRemindRepository;
        }

        public async Task<MethodResult<IList<NotificationRemindModel>>> Handle(GetListNotificationRemindCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<NotificationRemindModel>>();



            if (request.ObjectIds == null || request.ObjectIds.Count == 0)
            {
                methodResult.Result = new List<NotificationRemindModel>();
                return methodResult;
            }

            var query = await _notificationRemindRepository.Queryable
                        .Where(x => request.ObjectIds.Contains(x.ObjectId) && x.UserId == _authContext.CurrentUserId && x.Status == request.Status).ToListAsync(cancellationToken);

            methodResult.Result = _mapper.Map<IList<NotificationRemindModel>>(query);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
