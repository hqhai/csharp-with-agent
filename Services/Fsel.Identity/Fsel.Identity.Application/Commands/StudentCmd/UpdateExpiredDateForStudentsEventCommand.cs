namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class UpdateExpiredDateForStudentsEventCommand : UpdateExpiredDateForStudentsEventCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class UpdateExpiredDateForStudentsEventCommandHandler : IRequestHandler<UpdateExpiredDateForStudentsEventCommand, MethodResult<bool>>
    {
        private readonly IStudentRepository _studentRepository;

        public UpdateExpiredDateForStudentsEventCommandHandler(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<bool>> Handle(UpdateExpiredDateForStudentsEventCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            if (request.StudentIds == null || !request.StudentIds.Any())
            {
                methodResult.Result = true;
                return methodResult;
            }
            var students = await _studentRepository.Queryable.WhereBulkContains(request.StudentIds, n => n.Id).ToListAsync(cancellationToken);

            students.ForEach(p => p.ExpiredDate = request.ExpiredDate);

            await _studentRepository.BulkUpdateList(students, bulk =>
            {
                bulk.ColumnInputExpression = entity => new { entity.ExpiredDate };
            });

            methodResult.Result = true;
            return methodResult;
        }
    }
}
