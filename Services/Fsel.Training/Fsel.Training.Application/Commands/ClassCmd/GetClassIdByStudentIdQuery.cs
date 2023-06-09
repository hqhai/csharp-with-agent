// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.Training.Domain.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetClassIdByStudentIdQuery : IRequest<MethodResult<Guid>>
    {
        public Guid StudentId { get; set; }
    }

    public class GetClassIdByStudentIdQueryHandler : IRequestHandler<GetClassIdByStudentIdQuery, MethodResult<Guid>>
    {
        private readonly IClassRepository _classRepository;

        public GetClassIdByStudentIdQueryHandler(IClassRepository classRepository)
        {
            _classRepository = classRepository;
        }

        public async Task<MethodResult<Guid>> Handle(GetClassIdByStudentIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<Guid> methodResult = new MethodResult<Guid>();
            var classes = await _classRepository.Queryable.Include(cs => cs.ClassStudents).FirstOrDefaultAsync(p => p.Status == EnumClassType.New && p.ClassStudents.Any(p => p.StudentId == request.StudentId), cancellationToken);
            if (classes == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.ClassNotFound));
                return methodResult;
            }
            methodResult.Result = classes.Id;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
