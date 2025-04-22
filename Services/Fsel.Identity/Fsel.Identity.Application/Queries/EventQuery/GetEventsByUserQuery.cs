// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.EventQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using UserManager = Core.Base.Managers.UserManager<Domain.Entities.User>;


    public class GetEventsByUserQuery : IRequest<MethodResult<List<CompetitionEventsModel>>>
    {
        public Guid UserId { get; set; }
    }

    public class GetEventsByUserQueryHandler : IRequestHandler<GetEventsByUserQuery, MethodResult<List<CompetitionEventsModel>>>
    {
        private readonly IEventManagerRepository _eventManagerRepository;
        private readonly UserManager _userManager;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IMapper _mapper;

        public GetEventsByUserQueryHandler(
            IEventManagerRepository eventManagerRepository,
            IMapper mapper,
            UserManager userManager,
            ICompetitionEventsRepository competitionEventsRepository)
        {
            _eventManagerRepository = eventManagerRepository;
            _mapper = mapper;
            _userManager = userManager;
            _competitionEventsRepository = competitionEventsRepository;
        }

        public async Task<MethodResult<List<CompetitionEventsModel>>> Handle(GetEventsByUserQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<List<CompetitionEventsModel>>();

            // Kiểm tra xem user có tồn tại không
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.UserId), request.UserId);
                return methodResult;
            }

            // Lấy danh sách sự kiện của user sử dụng from in để tránh LEFT JOIN không cần thiết
            var events = await (from em in _eventManagerRepository.Queryable
                                from ce in _competitionEventsRepository.Queryable
                                where em.UserId == request.UserId
                                && em.CompetitionEventId == ce.Id
                                && !ce.IsDeleted
                                select new
                                {
                                    ce.Id,
                                    em.UserId,
                                    ce.LocationId
                                })
                          .ToListAsync(cancellationToken)
                          .ConfigureAwait(false);

            var result = _mapper.Map<List<CompetitionEventsModel>>(events);
            methodResult.Result = result;

            return methodResult;
        }
    }
}
