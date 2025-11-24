// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentByEmailsQuery : IRequest<MethodResult<IList<StudentModel>>>
    {
        public IList<string>? Emails { get; set; }
    }

    public class GetStudentByEmailsQueryHandler : IRequestHandler<GetStudentByEmailsQuery, MethodResult<IList<StudentModel>>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IMapper _mapper;

        public GetStudentByEmailsQueryHandler(IStudentRepository studentRepository, IMapper mapper)
        {
            _studentRepository = studentRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<StudentModel>>> Handle(GetStudentByEmailsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<StudentModel>> methodResult = new MethodResult<IList<StudentModel>>();
            if (request.Emails == null || !request.Emails.Any())
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var students = await _studentRepository.Queryable
                                    .Include(x => x.User)
                                    .Where(x => x.User != null && x.User!.Email != null && request.Emails.Contains(x.User!.Email))
                                    .ToListAsync(cancellationToken: cancellationToken);

            methodResult.Result = _mapper.Map<IList<StudentModel>>(students);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
