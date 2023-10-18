// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentQuery
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentsByIdsQuery : IRequest<MethodResult<List<StudentModel>>>
    {
        public IList<Guid>? Ids { get; set; }
    }

    public class GetStudentsByIdsQueryHandler : IRequestHandler<GetStudentsByIdsQuery, MethodResult<List<StudentModel>>>
    {
        private readonly IHumanRepository _humanRepository;
        private readonly IStudentRepository _studentRepository;

        public GetStudentsByIdsQueryHandler(IHumanRepository humanRepository, IStudentRepository studentRepository)
        {
            _humanRepository = humanRepository;
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<List<StudentModel>>> Handle(GetStudentsByIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<List<StudentModel>>();

            if (request.Ids == null || request.Ids.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Ids));
                return methodResult;
            }

            var humans = await _humanRepository.Queryable.Where(p => p.UserId.HasValue && request.Ids.Contains(p.UserId.Value)).Select(p => p.Id).ToListAsync(cancellationToken);
            var students = await _studentRepository.Queryable.Include(h => h.Human).Select(p => new StudentModel
            {
                Id = p.Id,
                PackageId = p.PackageId,
                Occupation = p.Occupation,
                School = p.School,
                CourseLevel = p.CourseLevel,
                ClassId = p.ClassId,
            }).Where(i => i.Human != null && humans.Contains(i.Human.Id)).ToListAsync(cancellationToken);

            methodResult.Result = students;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
