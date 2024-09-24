// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CompetitionEventsQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.SystemService.Model;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetSchoolsByEventCodeQuery : IRequest<MethodResult<IList<SchoolModel>>>
    {
        public string? EventCode { get; set; }
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

            var competitionEvent = _competitionEventsRepository.Queryable.FirstOrDefault(x => x.EventCode == request.EventCode);

            #region Validate

            if (competitionEvent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(competitionEvent), competitionEvent);
                return methodResult;
            }

            if (competitionEvent.SchoolIds == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(competitionEvent.SchoolIds), competitionEvent.SchoolIds);
                return methodResult;
            }
            #endregion

            IList<Guid> schoolIds = competitionEvent.SchoolIds;

            var listSchool = await _systemService.GetSchoolByIds(schoolIds);

            if (listSchool == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(listSchool), listSchool);
                return methodResult;
            }

            methodResult.Result = listSchool.Content?.Result;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
