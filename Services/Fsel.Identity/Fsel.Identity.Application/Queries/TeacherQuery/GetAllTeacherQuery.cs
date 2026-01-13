// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.TeacherQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetAllTeacherQuery : IRequest<MethodResult<IList<TeacherModel>>>
    {
    }

    public class GetAllTeacherQueryHandler : IRequestHandler<GetAllTeacherQuery, MethodResult<IList<TeacherModel>>>
    {
        private readonly ITeacherRepository _teacherRepository;
        private readonly IMapper _mapper;

        public GetAllTeacherQueryHandler(ITeacherRepository teacherRepository, IMapper mapper)
        {
            _teacherRepository = teacherRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<TeacherModel>>> Handle(GetAllTeacherQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<TeacherModel>>();
            var allTeacher = await _teacherRepository.Queryable.Include(p => p.User).ToListAsync(cancellationToken);
            methodResult.Result = _mapper.Map<IList<TeacherModel>>(allTeacher);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
