// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CheckSchoolImportHistoryCommand : IRequest<MethodResult<bool>>
    {
        public Guid SchoolId { get; set; }

        public Guid DistrictId { get; set; }
    }

    public class CheckSchoolImportHistoryCommandHandler : IRequestHandler<CheckSchoolImportHistoryCommand, MethodResult<bool>>
    {
        private readonly ISchoolImportHistoryRepository _schoolImportHistoryRepository;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;

        public CheckSchoolImportHistoryCommandHandler(ISchoolImportHistoryRepository schoolImportHistoryRepository, ICompetitionEventsRepository competitionEventsRepository)
        {
            _schoolImportHistoryRepository = schoolImportHistoryRepository;
            _competitionEventsRepository = competitionEventsRepository;
        }

        public async Task<MethodResult<bool>> Handle(CheckSchoolImportHistoryCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var competitionEvent = await _competitionEventsRepository.Queryable.Include(x => x.CompetitionEventParent).FirstOrDefaultAsync(p => p.Id == request.DistrictId, cancellationToken);
            if (competitionEvent == null || competitionEvent.CompetitionEventParent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(competitionEvent));
                return methodResult;
            }

            var checkImportSchool = await _schoolImportHistoryRepository.Queryable.AnyAsync(x => x.SchoolId == request.SchoolId && x.CompetitionEventId == competitionEvent.CompetitionEventParent.ParentEventId, cancellationToken);
            if (checkImportSchool)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUserSchoolErrorCode.SchoolAlreadyImported), nameof(request.SchoolId), request.SchoolId);
                return methodResult;
            }

            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
