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
        private readonly IStudentRepository _studentRepository;

        public GetStudentsByIdsQueryHandler(IStudentRepository studentRepository)
        {
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
            var students = await _studentRepository.Queryable.Where(i => request.Ids.Contains(i.UserId)).Select(p => new StudentModel
            {
                Id = p.Id,
                PackageId = p.PackageId,
                Occupation = p.Occupation,
                School = p.School,
                CourseLevel = p.CourseLevel,
                ClassId = p.ClassId,
            }).ToListAsync(cancellationToken);

            methodResult.Result = students;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
