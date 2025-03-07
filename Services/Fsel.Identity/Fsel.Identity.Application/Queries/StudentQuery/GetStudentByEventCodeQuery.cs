// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentByEventCodeQuery : IRequest<MethodResult<IList<StudentModel>>>
    {
        public string? EventCode { get; set; }
    }

    public class GetStudentByEventCodeQueryHandler : IRequestHandler<GetStudentByEventCodeQuery, MethodResult<IList<StudentModel>>>
    {
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IMapper _mapper;

        public GetStudentByEventCodeQueryHandler(ICompetitionEventsRepository competitionEventsRepository,
                                                 IStudentRepository studentRepository,
                                                 IMapper mapper)
        {
            _competitionEventsRepository = competitionEventsRepository;
            _studentRepository = studentRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<StudentModel>>> Handle(GetStudentByEventCodeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<StudentModel>> methodResult = new MethodResult<IList<StudentModel>>();

            #region Validate Params
            if (string.IsNullOrEmpty(request.EventCode))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required), nameof(request.EventCode), request.EventCode);
                return methodResult;
            }
            #endregion

            var competitionEvent = await _competitionEventsRepository.Queryable
                                                                     .Include(x => x.CompetitionEvents)
                                                                     .FirstOrDefaultAsync(x => x.EventCode == request.EventCode, cancellationToken);
            if (competitionEvent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(competitionEvent));
                return methodResult;
            }

            List<Guid> schoolIds = competitionEvent.CompetitionEvents.Where(x => x.SchoolIds != null && x.SchoolIds.Any()).SelectMany(x => x.SchoolIds!).ToList();

            var students = await _studentRepository.Queryable
                                                   .Where(x => x.SchoolId.HasValue && schoolIds.Contains(x.SchoolId.Value))
                                                   .ToListAsync(cancellationToken);
            if (students == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(students));
                return methodResult;
            }

            methodResult.Result = _mapper.Map<IList<StudentModel>>(students);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
