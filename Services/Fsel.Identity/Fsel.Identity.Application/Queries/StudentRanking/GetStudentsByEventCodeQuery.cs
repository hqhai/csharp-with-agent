// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentRanking
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentsByEventCodeQuery : IRequest<MethodResult<IList<StudentModel>>>
    {
        public string? EventCode { get; set; }
    }

    public class GetStudentsByEventCodeQueryHandler : IRequestHandler<GetStudentsByEventCodeQuery, MethodResult<IList<StudentModel>>>
    {
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IMapper _mapper;

        public GetStudentsByEventCodeQueryHandler(ICompetitionEventsRepository competitionEventsRepository, IStudentCompetitionEventsRepository studentCompetitionEventsRepository, IStudentRepository studentRepository, IMapper mapper)
        {
            _competitionEventsRepository = competitionEventsRepository;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
            _studentRepository = studentRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<StudentModel>>> Handle(GetStudentsByEventCodeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<StudentModel>>();

            if (string.IsNullOrEmpty(request.EventCode))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required), nameof(request.EventCode), request.EventCode);
                return methodResult;
            }

            var competitionEvent = _competitionEventsRepository.Queryable.FirstOrDefault(p => p.EventCode == request.EventCode);
            if (competitionEvent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.EventCode), request.EventCode);
                return methodResult;
            }

            var studentIds = await _studentCompetitionEventsRepository.Queryable.Where(p => p.CompetitionEventId == competitionEvent.Id).Select(p => p.StudentId).Distinct().ToListAsync(cancellationToken);

            var students = await _studentRepository.Queryable.Include(x => x.User).WhereBulkContains(studentIds, p => p.Id).ToListAsync(cancellationToken);

            methodResult.Result = _mapper.Map<IList<StudentModel>>(students);
            return methodResult;
        }
    }
}
