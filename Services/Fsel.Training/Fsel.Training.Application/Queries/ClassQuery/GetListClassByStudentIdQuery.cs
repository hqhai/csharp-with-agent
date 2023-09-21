// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListClassByStudentIdQuery : IRequest<MethodResult<IList<ClassModel>>>
    {
        public Guid StudentId { get; set; }
    }

    public class GetListClassByStudentIdQueryHandler : IRequestHandler<GetListClassByStudentIdQuery, MethodResult<IList<ClassModel>>>
    {
        private readonly IClassStudentRepository _classStudentRepository;
        private readonly IMapper _mapper;

        public GetListClassByStudentIdQueryHandler(IClassStudentRepository classStudentRepository, IMapper mapper)
        {
            _classStudentRepository = classStudentRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<ClassModel>>> Handle(GetListClassByStudentIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<ClassModel>> methodResult = new MethodResult<IList<ClassModel>>();
            var @classStudents = await _classStudentRepository.Queryable.Include(x => x.Class).Where(x => x.StudentId == request.StudentId).ToListAsync(cancellationToken);
            var @class = classStudents.Select(x => x.Class).ToList();
            methodResult.Result = _mapper.Map<IList<ClassModel>>(@class);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
