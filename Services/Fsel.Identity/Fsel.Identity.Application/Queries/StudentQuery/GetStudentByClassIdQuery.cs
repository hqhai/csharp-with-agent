// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentByClassIdQuery : IRequest<MethodResult<IList<StudentModel>>>
    {
        public string? Id { get; set; }
    }

    public class GetStudentByClassIdQueryHandler : IRequestHandler<GetStudentByClassIdQuery, MethodResult<IList<StudentModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IStudentRepository _studentRepository;

        public GetStudentByClassIdQueryHandler(IMapper mapper, IStudentRepository studentRepository)
        {
            _mapper = mapper;
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<IList<StudentModel>>> Handle(GetStudentByClassIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<IList<StudentModel>> methodResult = new MethodResult<IList<StudentModel>>();

            var student = await _studentRepository.Queryable.Where(x => x.ClassId.ToString() == request.Id).ToListAsync(cancellationToken: cancellationToken);
            if (student == null || student.Count == 0)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(nameof(EnumStudentErrorCode.StudentNull));
                return methodResult;
            }
            methodResult.Result = _mapper.Map<IList<StudentModel>>(student);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
