// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentByUserIdQuery : IRequest<MethodResult<StudentModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetStudentByUserIdQueryHandler : IRequestHandler<GetStudentByUserIdQuery, MethodResult<StudentModel>>
    {
        private readonly IMapper _mapper;
        private readonly IStudentRepository _studentRepository;

        public GetStudentByUserIdQueryHandler(IMapper mapper, IStudentRepository studentRepository)
        {
            _mapper = mapper;
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<StudentModel>> Handle(GetStudentByUserIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<StudentModel>();
            var student = await _studentRepository.Queryable
                                        .Include(i => i.Human)
                                        .FirstOrDefaultAsync(i => i.Human != null && i.Human.UserId == request.Id.ToString(), cancellationToken);

            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            methodResult.Result = _mapper.Map<StudentModel>(student);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
