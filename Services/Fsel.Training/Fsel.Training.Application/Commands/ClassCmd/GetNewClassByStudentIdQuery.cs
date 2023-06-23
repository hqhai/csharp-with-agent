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

    public class GetNewClassByStudentIdQuery : IRequest<MethodResult<ClassModel>>
    {
        public Guid StudentId { get; set; }
    }

    public class GetNewClassByStudentIdQueryHandler : IRequestHandler<GetNewClassByStudentIdQuery, MethodResult<ClassModel>>
    {
        private readonly IClassRepository _classRepository;

        public GetNewClassByStudentIdQueryHandler(IClassRepository classRepository)
        {
            _classRepository = classRepository;
        }

        public async Task<MethodResult<ClassModel>> Handle(GetNewClassByStudentIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassModel> methodResult = new MethodResult<ClassModel>();
            var classes = await _classRepository.Queryable.Include(cs => cs.ClassStudents).FirstOrDefaultAsync(p => p.Status == EnumClassType.New && p.ClassStudents.Any(p => p.StudentId == request.StudentId), cancellationToken);
            if (classes == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.ClassNotFound));
                return methodResult;
            }
            ClassModel classModel = new ClassModel()
            {
                Id = classes.Id,
                Code = classes.Code,
                Name = classes.Name,
                Status = classes.Status,
                StartDate = classes.StartDate,
                EndDate = classes.EndDate,
                CourseId = classes.CourseId,
            };
            methodResult.Result = classModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
