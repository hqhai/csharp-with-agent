using System.Globalization;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Base.Managers;
using Fsel.Identity.Application.Services.SenderService;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.CommandModels.StudentRanking;
using Fsel.Shared.Enums;
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
        private readonly ISenderService _senderService;
        private const string Subject = "Thông báo bạn không đủ điều kiện nhận quà sự kiện LeaderBoard";

        public RemoveStudentFromEventCommandHandler(UserManager<User> userManager, ICompetitionEventsRepository competitionEventsRepository, IStudentCompetitionEventsRepository studentCompetitionEventsRepository, ISenderService senderService)
        {
            _userManager = userManager;
            _competitionEventsRepository = competitionEventsRepository;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
            _senderService = senderService;
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

            var @event = await _competitionEventsRepository.Queryable.FirstOrDefaultAsync(p => p.EventCode == request.EventCode, cancellationToken);
            if (@event == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var userQuery = _userManager.Users.Include(p => p.Student);
            var usersByUserName = userQuery.Where(p => p.UserName != null && request.Emails.Contains(p.UserName));
            var usersByEmail = userQuery.Where(p => p.Email != null && request.Emails.Contains(p.Email));

            var users = await usersByUserName
                .Union(usersByEmail)
                .ToListAsync(cancellationToken);

            if (users.Count != request.Emails.Count)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var studentIds = users.Select(p => p.Student).Select(p => p.Id).ToList();

            var studentEvents = await _studentCompetitionEventsRepository.Queryable.Where(p => studentIds != null && studentIds.Contains(p.StudentId) && p.CompetitionEventId == @event.Id).ToListAsync(cancellationToken);

            await _studentCompetitionEventsRepository.ExecuteTransactionAsync(async () =>
            {
                await _studentCompetitionEventsRepository.DeleteListAsync(studentEvents);
                await _studentCompetitionEventsRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.Result = true;

                foreach (var user in users)
                {
                    await SendMail(user, request.SupportDay, EnumSenderTemplate.NotEligibleToParticipate, Subject);
                }

                return methodResult;
            });
            return methodResult;
        }

        private async Task SendMail(User user, DateTime supportDay, EnumSenderTemplate senderTemplate, string subject)
        {
            await _senderService.SendEmailAsync(new SendEmailByTemplateCommandModel
            {
                ToEmails = new List<string>() { user.Email ?? string.Empty },
                Template = senderTemplate,
                Subject = subject,
                Params = new
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    Date = supportDay.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture)
                },
            });
        }
    }
}
