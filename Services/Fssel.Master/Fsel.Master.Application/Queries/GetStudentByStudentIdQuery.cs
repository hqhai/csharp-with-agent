using Fsel.Common.ActionResults;
using Fsel.Master.Domain.Entities;
using Fsel.Master.Domain.IRepositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Master.Application.Queries
{
    public class GetStudentByStudentIdQuery : IRequest<MethodResult<StudentProfileReport>>
    {
        public Guid StudentId { get; set; }
    }

    public class GetStudentByStudentIdQueryHandler : IRequestHandler<GetStudentByStudentIdQuery, MethodResult<StudentProfileReport>>
    {
        private readonly IStudentProfileReportRepository _studentProfileReportRepository;

        public GetStudentByStudentIdQueryHandler(IStudentProfileReportRepository studentProfileReportRepository)
        {
            _studentProfileReportRepository = studentProfileReportRepository;
        }

        public async Task<MethodResult<StudentProfileReport>> Handle(GetStudentByStudentIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<StudentProfileReport>();

            var studentProfile = await _studentProfileReportRepository.Queryable.FirstOrDefaultAsync(p => p.StudentId == request.StudentId, cancellationToken);

            methodResult.Result = studentProfile;
            return methodResult;
        }
    }
}
