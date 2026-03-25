// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Application.Queries.FilterQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Master.Domain.Entities;
    using Fsel.Master.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SubjectModel
    {
        public Guid SubjectId { get; set; }

        public string? SubjectCode { get; set; }

        public string? SubjectName { get; set; }

        public bool IsHasValue { get; set; }
    }

    public class GetSubjectsQuery : IRequest<MethodResult<IList<SubjectModel>>>
    {
    }

    public class GetSubjectsQueryHandler : IRequestHandler<GetSubjectsQuery, MethodResult<IList<SubjectModel>>>
    {
        private readonly IMasterBaseRepository<PlacementTestReport> _placementTestRepository;
        private readonly IMasterBaseRepository<Subject> _subjectRepository;
        private readonly IMasterBaseRepository<Program> _programRepository;

        public GetSubjectsQueryHandler(IMasterBaseRepository<PlacementTestReport> placementTestRepository, IMasterBaseRepository<Subject> subjectRepository, IMasterBaseRepository<Program> programRepository)
        {
            _placementTestRepository = placementTestRepository;
            _subjectRepository = subjectRepository;
            _programRepository = programRepository;
        }

        public async Task<MethodResult<IList<SubjectModel>>> Handle(GetSubjectsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<SubjectModel>>();

            var subjectIds = await (from pt in _placementTestRepository.Queryable
                                    join p in _programRepository.Queryable on pt.ProgramId equals p.ProgramId
                                    select p.SubjectId).Distinct().ToListAsync(cancellationToken);

            var subjects = await _subjectRepository.Queryable.Select(p => new SubjectModel()
            {
                SubjectId = p.SubjectId,
                SubjectCode = p.SubjectCode,
                SubjectName = p.SubjectName,
                IsHasValue = subjectIds.Contains(p.SubjectId)
            }).ToListAsync(cancellationToken);

            methodResult.Result = subjects;
            return methodResult;
        }
    }
}
