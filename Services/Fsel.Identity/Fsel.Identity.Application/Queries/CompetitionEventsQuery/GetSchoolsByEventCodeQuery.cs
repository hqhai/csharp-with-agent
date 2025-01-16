// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CompetitionEventsQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.SystemService.Model;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MassTransit.Initializers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetSchoolsByEventCodeQuery : IRequest<MethodResult<IList<SchoolModel>>>
    {
        public string? EventCode { get; set; }

        public Guid? LocationId { get; set; }
        public EnumEducationLevel? EducationLevel { get; set; }
    }
    public class GetSchoolsByEventCodeQueryHandler : IRequestHandler<GetSchoolsByEventCodeQuery, MethodResult<IList<SchoolModel>>>
    {
        private readonly ISystemService _systemService;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        public GetSchoolsByEventCodeQueryHandler(ISystemService systemService, ICompetitionEventsRepository competitionEventsRepository)
        {
            _systemService = systemService;
            _competitionEventsRepository = competitionEventsRepository;
        }

        public async Task<MethodResult<IList<SchoolModel>>> Handle(GetSchoolsByEventCodeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<SchoolModel>>();

            CompetitionEvent? competitionEvent = new CompetitionEvent();

            #region Validate Params
            if (string.IsNullOrEmpty(request.EventCode))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required), nameof(request.EventCode), request.EventCode);
                return methodResult;
            }
            #endregion


            if (!string.IsNullOrEmpty(request.EventCode) && request.LocationId == null)
            {
                competitionEvent = await _competitionEventsRepository.Queryable.FirstOrDefaultAsync(x => x.EventCode == request.EventCode, cancellationToken);

            }
            else
            {
                var parentEventId = await _competitionEventsRepository.Queryable.FirstOrDefaultAsync(x => x.EventCode == request.EventCode, cancellationToken).Select(x => x.Id);

                competitionEvent = await _competitionEventsRepository.Queryable.FirstOrDefaultAsync(x => x.LocationId == request.LocationId && x.ParentEventId == parentEventId, cancellationToken);
            }

            if (competitionEvent == null || competitionEvent.SchoolIds == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            IList<Guid> schoolIds = competitionEvent.SchoolIds;
            var listSchool = await _systemService.GetSchoolByIds(schoolIds);
            if (listSchool == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var listSchoolTemp = listSchool.Content?.Result;
            if (request.EducationLevel != null)
            {
                listSchoolTemp = listSchoolTemp?.Where(x => x.EducationLevel == request.EducationLevel || x.EducationLevel == EnumEducationLevel.InterLevel).ToList();
            }

            methodResult.Result = listSchoolTemp;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
