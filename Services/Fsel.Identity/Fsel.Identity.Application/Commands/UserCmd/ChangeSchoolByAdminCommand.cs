// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ChangeSchoolByAdminCommand : IRequest<MethodResult<bool>>
    {
        public string? EventCode { get; set; }

        public string? LocalId { get; set; }

        public Guid StudentId { get; set; }
    }

    public class ChangeSchoolByAdminCommandHandler : IRequestHandler<ChangeSchoolByAdminCommand, MethodResult<bool>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly ISystemService _systemService;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;

        public ChangeSchoolByAdminCommandHandler(IStudentRepository studentRepository,
                                                 ISystemService systemService,
                                                 ICompetitionEventsRepository competitionEventsRepository)
        {
            _studentRepository = studentRepository;
            _systemService = systemService;
            _competitionEventsRepository = competitionEventsRepository;
        }

        public async Task<MethodResult<bool>> Handle(ChangeSchoolByAdminCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.LocalId);
            ArgumentNullException.ThrowIfNull(request.EventCode);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var student = await _studentRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.StudentId, cancellationToken);
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student), request.StudentId);
                return methodResult;
            }

            var locationResult = await _systemService.GetLocationByLocalId(request.LocalId);
            var location = locationResult.Content?.Result;
            if (!locationResult.IsSuccessStatusCode || location == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(location), request.LocalId);
                return methodResult;
            }

            var competitionEvent = await _competitionEventsRepository.Queryable
                                                                     .FirstOrDefaultAsync(x => x.EventCode != null && x.EventCode.Trim() == request.EventCode.Trim(), cancellationToken);
            if (competitionEvent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(competitionEvent), request.EventCode);
                return methodResult;
            }

            if (competitionEvent.SchoolIds == null || !competitionEvent.SchoolIds.Any(x => x == location.Id))
            {
                var parentEventId = await ParentEventAsync(competitionEvent.ParentEventId ?? Guid.Empty, cancellationToken);
                var checkSchoolInEvent = await CheckSchoolInEvent(location.Id, new List<Guid> { parentEventId == Guid.Empty ? competitionEvent.Id : parentEventId }, cancellationToken);
                if (!checkSchoolInEvent)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumStudentErrorCode.SchoolNotInEvent), nameof(request.LocalId), request.LocalId);
                    return methodResult;
                }
            }

            await _studentRepository.ExecuteTransactionAsync(async () =>
            {
                student.SchoolId = location.Id;
                student.School = location.Name;

                _studentRepository.Update(student);
                await _studentRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }

        private async Task<Guid> ParentEventAsync(Guid parentEventId, CancellationToken cancellationToken)
        {
            var competitionEvent = await _competitionEventsRepository.Queryable.FirstOrDefaultAsync(x => x.Id == parentEventId, cancellationToken);
            if (competitionEvent == null || competitionEvent.ParentEventId == null)
            {
                return parentEventId;
            }
            else
            {
                await ParentEventAsync(competitionEvent.ParentEventId.Value, cancellationToken);
            }

            return parentEventId;
        }

        private async Task<bool> CheckSchoolInEvent(Guid schoolId, IList<Guid> eventIds, CancellationToken cancellationToken)
        {
            var competitionEvents = await _competitionEventsRepository.Queryable
                                                                      .Where(x => x.ParentEventId.HasValue && eventIds.Contains(x.ParentEventId.Value))
                                                                      .ToListAsync(cancellationToken);

            if (competitionEvents.Any(x => x.SchoolIds != null && x.SchoolIds.Any(c => c == schoolId)))
            {
                return true;
            }
            else
            {
                var competitionEventIds = competitionEvents.Select(x => x.Id).ToList();
                if (competitionEventIds != null && competitionEventIds.Any())
                {
                    await CheckSchoolInEvent(schoolId, competitionEventIds, cancellationToken);
                }
            }

            return false;
        }
    }
}
