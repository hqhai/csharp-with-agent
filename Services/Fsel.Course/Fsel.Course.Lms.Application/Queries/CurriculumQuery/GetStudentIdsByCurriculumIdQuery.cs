// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CurriculumQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentIdsByCurriculumIdQuery : IRequest<MethodResult<IList<Guid>>>
    {
        public Guid CurriculumId { get; set; }
    }

    public class GetStudentIdsByCurriculumIdQueryHandler : IRequestHandler<GetStudentIdsByCurriculumIdQuery, MethodResult<IList<Guid>>>
    {
        private readonly ICurriculumStudentRepository _curriculumStudentRepository;

        public GetStudentIdsByCurriculumIdQueryHandler(ICurriculumStudentRepository curriculumStudentRepository)
        {
            _curriculumStudentRepository = curriculumStudentRepository;
        }

        public async Task<MethodResult<IList<Guid>>> Handle(GetStudentIdsByCurriculumIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<Guid>>();

            var studentIds = await _curriculumStudentRepository.Queryable.Where(p => p.CurriculumId == request.CurriculumId).Select(p => p.StudentId).ToListAsync(cancellationToken);

            methodResult.Result = studentIds;
            return methodResult;
        }
    }
}
