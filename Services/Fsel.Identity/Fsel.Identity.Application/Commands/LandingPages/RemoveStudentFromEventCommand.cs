using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Base.Managers;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.CommandModels.StudentRanking;
using Fsel.Identity.Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Application.Commands.LandingPages
{
    public class RemoveStudentFromEventCommand : RemoveStudentFromEventCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class RemoveStudentFromEventCommandHandler : IRequestHandler<RemoveStudentFromEventCommand, MethodResult<bool>>
    {
        private readonly UserManager<User> _userManager;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;

        public RemoveStudentFromEventCommandHandler(UserManager<User> userManager, ICompetitionEventsRepository competitionEventsRepository, IStudentCompetitionEventsRepository studentCompetitionEventsRepository)
        {
            _userManager = userManager;
            _competitionEventsRepository = competitionEventsRepository;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
        }

        public async Task<MethodResult<bool>> Handle(RemoveStudentFromEventCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (request.Emails == null || request.Emails.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var @event = await _competitionEventsRepository.Queryable.FirstOrDefaultAsync(p => p.EventCode.ToLower() == request.EventCode.ToLower(), cancellationToken);
            if (@event == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var users = await _userManager.Users.Include(p => p.Human).ThenInclude(p => p.Student).Where(p => request.Emails.Contains(p.UserName) || request.Emails.Contains(p.Email)).ToListAsync(cancellationToken);

            if (users.Count != request.Emails.Count)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var studentIds = users.Select(p => p.Human).Select(p => p.Student).Select(p => p.Id).ToList();

            var studentEvents = await _studentCompetitionEventsRepository.Queryable.Where(p => studentIds != null && studentIds.Contains(p.StudentId) && p.CompetitionEventId == @event.Id).ToListAsync(cancellationToken);

            await _studentCompetitionEventsRepository.ExecuteTransactionAsync(async () =>
            {
                await _studentCompetitionEventsRepository.DeleteListAsync(studentEvents);
                await _studentCompetitionEventsRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
