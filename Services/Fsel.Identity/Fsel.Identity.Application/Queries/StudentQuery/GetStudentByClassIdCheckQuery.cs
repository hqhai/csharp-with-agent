// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentByClassIdCheckQuery : IRequest<MethodResult<bool>>
    {
        public string? Id { get; set; }
    }

    public class GetStudentByClassIdCheckQueryHandler : IRequestHandler<GetStudentByClassIdCheckQuery, MethodResult<bool>>
    {
        private readonly IStudentRepository _studentRepository;

        public GetStudentByClassIdCheckQueryHandler(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<bool>> Handle(GetStudentByClassIdCheckQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<bool> methodResult = new MethodResult<bool>();

            var student = await _studentRepository.Queryable.Where(x => x.ClassId.ToString() == request.Id).ToListAsync(cancellationToken: cancellationToken);
            if (student == null)
            {
                methodResult.Result = false;
                methodResult.AddErrorBadRequest(nameof(EnumStudentErrorCode.StudentsNotExist));
                return methodResult;
            }
            else if (student.Count >= 12)
            {
                methodResult.Result = false;
                methodResult.AddErrorBadRequest(nameof(EnumStudentErrorCode.ClassMoreThan12Students));
            }
            else if (student.Count < 12)
            {
                methodResult.Result = true;
                methodResult.StatusCode = StatusCodes.Status200OK;
            }
            return methodResult;
        }
    }
}
