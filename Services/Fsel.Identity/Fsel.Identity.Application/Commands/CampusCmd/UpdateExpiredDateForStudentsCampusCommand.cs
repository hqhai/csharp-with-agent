// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.Campus
{
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Models.ShareModels.CampusModel;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class UpdateExpiredDateForStudentsCampusCommand : UpdateExpiredDateForStudentsCampusCommandModels, IRequest<MethodResult<bool>>
    {
    }

    public class UpdateExpiredDateForStudentsCampusCommandHandler : IRequestHandler<UpdateExpiredDateForStudentsCampusCommand, MethodResult<bool>>
    {
        private readonly IStudentRepository _studentRepository;

        public UpdateExpiredDateForStudentsCampusCommandHandler(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<bool>> Handle(UpdateExpiredDateForStudentsCampusCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (request.Students == null || !request.Students.Any())
            {
                methodResult.Result = true;
                return methodResult;
            }

            var studentIds = request.Students.Select(p => p.StudentId).ToList();

            var studentEntities = await _studentRepository.Queryable
                .WhereBulkContains(studentIds, n => n.Id)
                .ToListAsync(cancellationToken);

            var studentDict = request.Students.ToDictionary(x => x.StudentId, x => x);

            var studentsBag = new ConcurrentBag<Student>();

            Parallel.ForEach(studentEntities, p =>
            {
                if (studentDict.TryGetValue(p.Id, out var student))
                {
                    bool updateExpiredDate =
                        !p.ExpiredDate.HasValue ||
                        student.ExpiredDate.HasValue && p.ExpiredDate.Value < student.ExpiredDate ||
                        !student.ExpiredDate.HasValue;

                    if (updateExpiredDate)
                    {
                        p.ExpiredDate = student.ExpiredDate;
                        studentsBag.Add(p);
                    }
                }
            });

            var students = studentsBag.ToList();

            await _studentRepository.BulkUpdateList(students);

            methodResult.Result = true;
            return methodResult;
        }
    }
}
