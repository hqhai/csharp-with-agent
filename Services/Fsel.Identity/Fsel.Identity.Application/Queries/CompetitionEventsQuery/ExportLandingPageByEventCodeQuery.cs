// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CompetitionEventsQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.CompetitionEvent;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ExportLandingPageByEventCodeQuery : IRequest<MethodResult<Stream>>
    {
        public string? EventCode { get; set; }

        public string? District { get; set; }

        public string? School { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }

    public class ExportLandingPageByEventCodeQueryHandler : IRequestHandler<ExportLandingPageByEventCodeQuery, MethodResult<Stream>>
    {
        private readonly IMapper _mapper;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IEventRegistrationRepository _eventRegistrationRepository;

        public ExportLandingPageByEventCodeQueryHandler(IMapper mapper,
                                                        ICompetitionEventsRepository competitionEventsRepository,
                                                        IEventRegistrationRepository eventRegistrationRepository)
        {
            _mapper = mapper;
            _competitionEventsRepository = competitionEventsRepository;
            _eventRegistrationRepository = eventRegistrationRepository;
        }

        public async Task<MethodResult<Stream>> Handle(ExportLandingPageByEventCodeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<Stream> methodResult = new MethodResult<Stream>();

            if (string.IsNullOrEmpty(request.EventCode) || string.IsNullOrEmpty(request.District))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request));
                return methodResult;
            }

            var competitionEvent = await _competitionEventsRepository.Queryable.FirstOrDefaultAsync(x => x.EventCode != null && x.EventCode.Trim() == request.EventCode.Trim(), cancellationToken);
            if (competitionEvent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(competitionEvent));
                return methodResult;
            }

            var eventRegistrations = await _eventRegistrationRepository.Queryable
                                                                       .Where(x => x.CompetitionEventId == competitionEvent.Id && x.CreatedDate.Date >= request.StartDate.Date && x.CreatedDate.Date <= request.EndDate.Date && x.District.Trim() == request.District.Trim())
                                                                       .ToListAsync(cancellationToken);
            if (eventRegistrations == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(eventRegistrations));
                return methodResult;
            }

            if (!string.IsNullOrEmpty(request.School))
            {
                eventRegistrations = eventRegistrations.Where(x => x.School != null && x.School.Trim() == request.School.Trim()).ToList();
            }

            var template = _mapper.Map<IList<ExportLandingPageByEventCodeModel>>(eventRegistrations);
            methodResult.Result = template.ExportExcel();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
