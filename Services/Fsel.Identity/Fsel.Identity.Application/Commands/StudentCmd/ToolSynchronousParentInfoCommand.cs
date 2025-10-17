using System.Linq.Dynamic.Core;
using Fsel.Common.ActionResults;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.IRepositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    public class ToolSynchronousParentInfoCommand : IRequest<MethodResult<bool>>
    {
    }

    public class ToolSynchronousParentInfoCommandHandler : IRequestHandler<ToolSynchronousParentInfoCommand, MethodResult<bool>>
    {
        private readonly IParentStudentRepository _parentStudentRepository;
        private readonly IParentRepository _parentRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IHumanRepository _humanRepository;

        public ToolSynchronousParentInfoCommandHandler(IParentStudentRepository parentStudentRepository, IParentRepository parentRepository, IStudentRepository studentRepository, IHumanRepository humanRepository)
        {
            _parentStudentRepository = parentStudentRepository;
            _parentRepository = parentRepository;
            _studentRepository = studentRepository;
            _humanRepository = humanRepository;
        }

        public async Task<MethodResult<bool>> Handle(ToolSynchronousParentInfoCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var studentIds = await HandleOne(methodResult);
            await HandleTwo(studentIds, methodResult);

            return methodResult;
        }

        private async Task<List<Guid>> HandleOne(MethodResult<bool> methodResult)
        {
            var query = await (from s in _studentRepository.Queryable
                               join ps in _parentStudentRepository.Queryable on s.Id equals ps.StudentId
                               join p in _parentRepository.Queryable on ps.ParentId equals p.Id
                               join h in _humanRepository.Queryable on p.HumanId equals h.Id
                               select new
                               {
                                   StudentId = s.Id,
                                   Email = h.Email,
                                   PhoneNumber = h.PhoneNumber,
                                   FullName = h.FullName
                               }).ToListAsync(CancellationToken.None);

            query = query.DistinctBy(p => p.StudentId).ToList();

            var studentIds = query.Select(p => p.StudentId).ToList();

            var students = await _studentRepository.Queryable.WhereBulkContains(studentIds, p => p.Id).ToListAsync(CancellationToken.None);

            var studentEntities = new List<Student>();

            students.ForEach(p =>
            {
                var student = query.FirstOrDefault(x => x.StudentId == p.Id);
                if (student != null)
                {
                    p.ParentEmail = string.IsNullOrEmpty(student.Email) ? null : student.Email;
                    p.ParentPhoneNumber = string.IsNullOrEmpty(student.PhoneNumber) ? null : student.PhoneNumber;
                    studentEntities.Add(p);
                }
            });

            await _studentRepository.ExecuteTransactionAsync(async () =>
            {
                await _studentRepository.BulkUpdateList(studentEntities);
                await _studentRepository.UnitOfWork.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
                methodResult.Result = true;
                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });

            return studentEntities.Select(p => p.Id).ToList();
        }

        private async Task HandleTwo(IList<Guid> studentIds, MethodResult<bool> methodResult)
        {
            var query = await _studentRepository.Queryable.WhereBulkNotContains(studentIds, p => p.Id).Where(p => !string.IsNullOrEmpty(p.ParentEmail) || !string.IsNullOrEmpty(p.ParentPhoneNumber)).ToListAsync();

            var humans = new List<Human>();
            var parents = new List<Parent>();
            var parentStudents = new List<ParentStudent>();

            query.ForEach(p =>
            {
                var humanId = Guid.NewGuid();
                var parentId = Guid.NewGuid();
                var parentStudentId = Guid.NewGuid();

                humans.Add(new Human()
                {
                    Id = humanId,
                    FullName = "N/A",
                    Email = string.IsNullOrEmpty(p.ParentEmail) ? null : p.ParentEmail,
                    PhoneNumber = string.IsNullOrEmpty(p.ParentPhoneNumber) ? null : p.ParentPhoneNumber,
                });

                parents.Add(new Parent()
                {
                    Id = parentId,
                    HumanId = humanId,
                });

                parentStudents.Add(new ParentStudent()
                {
                    Id = parentStudentId,
                    ParentId = parentId,
                    StudentId = p.Id
                });
            });

            await _humanRepository.ExecuteTransactionAsync(async () =>
            {
                await _humanRepository.BulkMergeAsync(humans);
                await _parentRepository.BulkMergeAsync(parents);
                await _parentStudentRepository.BulkMergeAsync(parentStudents);

                await _studentRepository.UnitOfWork.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
                methodResult.Result = true;
                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });
        }
    }
}
