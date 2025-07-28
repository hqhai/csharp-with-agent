// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.EventCmd
{
    using System.ComponentModel.DataAnnotations;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using UserManager = Core.Base.Managers.UserManager<Domain.Entities.User>;


    public class AddUserToEventCommand : IRequest<MethodResult<EventManagerModel>>
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid CompetitionEventId { get; set; }
    }

    public class AddUserToEventCommandHandler : IRequestHandler<AddUserToEventCommand, MethodResult<EventManagerModel>>
    {
        private readonly IEventManagerRepository _eventManagerRepository;
        private readonly UserManager _userManager;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IMapper _mapper;

        public AddUserToEventCommandHandler(
            IEventManagerRepository eventManagerRepository,
            ICompetitionEventsRepository competitionEventsRepository,
            IMapper mapper,
            UserManager userManager)
        {
            _eventManagerRepository = eventManagerRepository;
            _competitionEventsRepository = competitionEventsRepository;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<MethodResult<EventManagerModel>> Handle(AddUserToEventCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<EventManagerModel>();

            // Kiểm tra xem user có tồn tại không
            var user = await _userManager.FindByIdAsync(request.UserId.ToString()).ConfigureAwait(false);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), "User not found");
                return methodResult;
            }

            // Kiểm tra xem event có tồn tại không
            var competitionEvent = await _competitionEventsRepository.GetByIdAsync(request.CompetitionEventId).ConfigureAwait(false);
            if (competitionEvent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), "Event not found");
                return methodResult;
            }

            // Kiểm tra xem đã tồn tại không
            var existingRecord = await _eventManagerRepository.Queryable
                .FirstOrDefaultAsync(x => x.UserId == request.UserId && x.CompetitionEventId == request.CompetitionEventId, cancellationToken)
                .ConfigureAwait(false);

            if (existingRecord != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), "User has already been assigned to this event");
                return methodResult;
            }

            await _eventManagerRepository.ExecuteTransactionAsync(async () =>
            {
                // Tạo mới event manager
                var eventManager = new EventManager
                {
                    UserId = request.UserId,
                    CompetitionEventId = request.CompetitionEventId
                };

                _eventManagerRepository.Add(eventManager);
                await _eventManagerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                var result = _mapper.Map<EventManagerModel>(eventManager);
                methodResult.Result = result;
                return methodResult;
            });

            return methodResult;
        }
    }
}
