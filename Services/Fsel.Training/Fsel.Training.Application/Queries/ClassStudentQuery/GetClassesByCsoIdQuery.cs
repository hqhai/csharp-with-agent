// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassStudentQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetClassesByCsoIdQuery : IRequest<MethodResult<IList<ClassModel>>>
    {
        public Guid? CsoId { get; set; }
    }

    public class GetClassesByCsoIdQueryHandler : IRequestHandler<GetClassesByCsoIdQuery, MethodResult<IList<ClassModel>>>
    {
        private readonly IClassRepository _classRepository;

        public GetClassesByCsoIdQueryHandler(IClassRepository classRepository)
        {
            _classRepository = classRepository;
        }

        public async Task<MethodResult<IList<ClassModel>>> Handle(GetClassesByCsoIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<ClassModel>> methodResult = new MethodResult<IList<ClassModel>>();

            var classStudent = await _classRepository.Queryable
                                .Include(x => x.ClassStudents)
                                .Where(x => x.CsoId == request.CsoId)
                                .Select(x => new ClassModel
                                {
                                    Id = x.Id,
                                    CsoId = x.CsoId,
                                    CreatedDate = x.CreatedDate,
                                    CourseId = x.CourseId,
                                    Code = x.Code,
                                    Name = x.Name,
                                    TeacherId = x.TeacherId,
                                    Status = x.Status,
                                    ClassStudents = x.ClassStudents.Select(x => new ClassStudentModel
                                    {
                                        StudentId = x.StudentId,
                                    }).ToList(),
                                }).ToListAsync(cancellationToken);

            methodResult.Result = classStudent;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
