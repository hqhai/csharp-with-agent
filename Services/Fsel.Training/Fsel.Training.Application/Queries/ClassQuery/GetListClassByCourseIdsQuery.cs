// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Helpers;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListClassByCourseIdsQuery : IRequest<MethodResult<IList<ClassModel>>>
    {
        public string? CourseIdStr { get; set; }

        public IList<Guid>? CourseIds
        {
            get { return CourseIdStr.ToList<Guid>(); }
        }
    }

    public class GetListClassByCourseIdsQueryHandler : IRequestHandler<GetListClassByCourseIdsQuery, MethodResult<IList<ClassModel>>>
    {
        private readonly IClassRepository _classRepository;
        private readonly IMapper _mapper;

        public GetListClassByCourseIdsQueryHandler(IClassRepository classRepository, IMapper mapper)
        {
            _classRepository = classRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<ClassModel>>> Handle(GetListClassByCourseIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<IList<ClassModel>>();
            var classes = await _classRepository.Queryable.Where(x => request.CourseIds != null && request.CourseIds.Contains(x.CourseId)).ToListAsync(cancellationToken);
            methodResult.Result = _mapper.Map<IList<ClassModel>>(classes);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
