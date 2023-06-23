// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetClassByStudentIdQuery : IRequest<MethodResult<ClassModel>>
    {
        public Guid StudentId { get; set; }
    }

    public class GetClassByStudentIdQueryHandler : IRequestHandler<GetClassByStudentIdQuery, MethodResult<ClassModel>>
    {
        private readonly IMapper _mapper;
        private readonly IClassRepository _classRepository;

        public GetClassByStudentIdQueryHandler(IMapper mapper, IClassRepository classRepository)
        {
            _mapper = mapper;
            _classRepository = classRepository;
        }

        public async Task<MethodResult<ClassModel>> Handle(GetClassByStudentIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<ClassModel> methodResult = new MethodResult<ClassModel>();

            var @class = await _classRepository.Queryable
                                            .Include(x => x.ClassStudents.Where(n => !n.IsDeleted))
                                            .Where(e => e.Status != EnumClassType.Done && e.ClassStudents.Select(n => n.StudentId).Contains(request.StudentId))
                                            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            methodResult.Result = _mapper.Map<ClassModel>(@class);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
