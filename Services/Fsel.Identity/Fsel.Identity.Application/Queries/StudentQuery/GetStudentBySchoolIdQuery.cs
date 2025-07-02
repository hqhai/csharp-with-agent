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

    public class GetStudentBySchoolIdQuery : IRequest<MethodResult<IList<StudentModel>>>
    {
    }

    public class GetStudentBySchoolIdQueryHandler : IRequestHandler<GetStudentBySchoolIdQuery, MethodResult<IList<StudentModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IStudentRepository _studentRepository;
        private readonly IUserSchoolRepository _userSchoolRepository;

        public GetStudentBySchoolIdQueryHandler(IMapper mapper, IStudentRepository studentRepository, IUserSchoolRepository userSchoolRepository)
        {
            _mapper = mapper;
            _studentRepository = studentRepository;
            _userSchoolRepository = userSchoolRepository;
        }

        public async Task<MethodResult<IList<StudentModel>>> Handle(GetStudentBySchoolIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<StudentModel>>();
            var schoolId = await _userSchoolRepository.GetSchoolIdAsync();
            //var students = _studentRepository.Queryable.Where(x => x.SchoolId == schoolId).ToList();

            var students = await _studentRepository.Queryable
                .Include(s => s.User)
                .Where(x => x.SchoolId == schoolId)
                .ToListAsync(cancellationToken);

            methodResult.Result = _mapper.Map<IList<StudentModel>>(students);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
