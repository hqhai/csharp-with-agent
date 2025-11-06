// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.CampusCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Models.ShareModels.CampusModel;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateCourseIdOfStudentsCommand : UpdateCourseIdOfStudentsCommandModels, IRequest<MethodResult<bool>>
    {
    }

    public class UpdateCourseIdOfStudentsCommandHandler : IRequestHandler<UpdateCourseIdOfStudentsCommand, MethodResult<bool>>
    {
        private readonly IStudentRepository _studentRepository;

        public UpdateCourseIdOfStudentsCommandHandler(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<bool>> Handle(UpdateCourseIdOfStudentsCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (request.Students == null || !request.Students.Any())
            {
                return methodResult;
            }

            var studentIds = request.Students.Select(p => p.StudentId).ToList();

            var students = await _studentRepository.Queryable.WhereBulkContains(studentIds, p => p.Id).ToListAsync(cancellationToken);
            students.ForEach(p =>
            {
                var student = request.Students.FirstOrDefault(x => x.StudentId == p.Id);
                if (student != null)
                {
                    p.CourseId = student.CourseId;
                    p.CourseLevel = student.CourseLevel;
                    p.ClassId = student.ClassId;
                }
            });

            await _studentRepository.ExecuteTransactionAsync(async () =>
            {
                await _studentRepository.BulkMergeAsync(students);
                await _studentRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
