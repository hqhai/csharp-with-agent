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

    public class GetStudentByFullNamesQuery : IRequest<MethodResult<IList<StudentModel>>>
    {
        public IList<string>? FullNames { get; set; }
    }

    public class GetStudentByFullNamesQueryHandler : IRequestHandler<GetStudentByFullNamesQuery, MethodResult<IList<StudentModel>>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IMapper _mapper;

        public GetStudentByFullNamesQueryHandler(IStudentRepository studentRepository, IMapper mapper)
        {
            _studentRepository = studentRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<StudentModel>>> Handle(GetStudentByFullNamesQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<StudentModel>> methodResult = new MethodResult<IList<StudentModel>>();
            if (request.FullNames == null || !request.FullNames.Any())
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var students = await _studentRepository.Queryable
                                    .Include(x => x.User)
                                    .Where(x => x.User != null && x.User.FullName != null && request.FullNames.Contains(x.User.FullName))
                                    .ToListAsync(cancellationToken: cancellationToken);

            methodResult.Result = _mapper.Map<IList<StudentModel>>(students);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
