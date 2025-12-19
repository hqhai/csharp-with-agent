// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.TeacherQuery
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetTeachersByKeywordQuery : IRequest<MethodResult<IList<TeacherModel>>>
    {
        public string? Keyword { get; set; }
    }

    public class GetTeachersByKeywordQueryHandler : IRequestHandler<GetTeachersByKeywordQuery, MethodResult<IList<TeacherModel>>>
    {
        private readonly ITeacherRepository _teacherRepository;
        private readonly IMapper _mapper;

        public GetTeachersByKeywordQueryHandler(ITeacherRepository teacherRepository, IMapper mapper)
        {
            _teacherRepository = teacherRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<TeacherModel>>> Handle(GetTeachersByKeywordQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<TeacherModel>> methodResult = new MethodResult<IList<TeacherModel>>();
            if (string.IsNullOrEmpty(request.Keyword))
            {
                methodResult.Result = null;
                return methodResult;
            }
            var teachers = await _teacherRepository.Queryable.Include(x => x.User).ToListAsync(cancellationToken);
            teachers = teachers.Where(x => !string.IsNullOrEmpty(x.User?.FullName) && x.User.FullName.Contains(request.Keyword!, StringComparison.OrdinalIgnoreCase)).ToList();
            methodResult.Result = _mapper.Map<IList<TeacherModel>>(teachers);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
