using System.Linq.Dynamic.Core;
using Fsel.Common.ActionResults;
using Fsel.Core.Base.Managers;
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
        private readonly UserManager<User> _userManager;

        public ToolSynchronousParentInfoCommandHandler(IParentStudentRepository parentStudentRepository, IParentRepository parentRepository, IStudentRepository studentRepository, UserManager<User> userManager)
        {
            _parentStudentRepository = parentStudentRepository;
            _parentRepository = parentRepository;
            _studentRepository = studentRepository;
            _userManager = userManager;
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
                               join u in _userManager.Users on s.UserId equals u.Id
                               select new
                               {
                                   StudentId = s.Id,
                                   Email = u.Email,
                                   PhoneNumber = u.PhoneNumber,
                                   FullName = u.FullName
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

            var parents = new List<Parent>();
            var parentStudents = new List<ParentStudent>();

            query.ForEach(async p =>
            {
                var parentId = Guid.NewGuid();
                var parentStudentId = Guid.NewGuid();

                var user = new User()
                {
                    LastName = "N/A",
                    FirstName = "N/A",
                    Email = string.IsNullOrEmpty(p.ParentEmail) ? null : p.ParentEmail,
                    PhoneNumber = string.IsNullOrEmpty(p.ParentPhoneNumber) ? null : p.ParentPhoneNumber,
                };

                await _userManager.CreateAsync(user);

                parents.Add(new Parent()
                {
                    Id = parentId,
                    UserId = user.Id,
                });

                parentStudents.Add(new ParentStudent()
                {
                    Id = parentStudentId,
                    ParentId = parentId,
                    StudentId = p.Id
                });
            });

            await _parentRepository.ExecuteTransactionAsync(async () =>
            {
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
