// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentQuery
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentsByIdsQuery : IRequest<MethodResult<List<StudentModel>>>
    {
        public IList<string>? Ids { get; set; }
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
                methodResult.AddErrorBadRequest(nameof(EnumUserErrorCode.StudentIdsNull), nameof(request.Ids), request.Ids);
                return methodResult;
            }

            var humans = await _humanRepository.Queryable.Where(p => request.Ids.Contains(p.UserId!)).Select(p => p.Id).ToListAsync(cancellationToken);
            var students = await _studentRepository.Queryable.Include(h => h.Human).Select(p => new StudentModel
            {
                Id = p.Id,
                Membership = p.Membership,
                Occupation = p.Occupation,
                School = p.School,
                CourseLevel = p.CourseLevel,
                ClassId = p.ClassId,
                HumanId = p.HumanId,
                UserId = p.Human!.UserId
            }).Where(i => humans.Contains(i.HumanId)).ToListAsync(cancellationToken);

            methodResult.Result = students;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
