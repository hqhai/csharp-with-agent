// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassQuery
{
    using System.Collections.Generic;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetClassByStatusNewQuery : IRequest<MethodResult<IList<CourseClassModel>>>
    {
        public IList<Guid>? CourseIds { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
    }

    public class GetClassByStatusNewQueryHandler : IRequestHandler<GetClassByStatusNewQuery, MethodResult<IList<CourseClassModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IClassRepository _classRepository;
        private readonly IMediator _mediator;

        public GetClassByStatusNewQueryHandler(IMapper mapper, IClassRepository classRepository, IMediator mediator)
        {
            _mapper = mapper;
            _classRepository = classRepository;
            _mediator = mediator;
        }

        public async Task<MethodResult<IList<CourseClassModel>>> Handle(GetClassByStatusNewQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.CourseIds);
            MethodResult<IList<CourseClassModel>> methodResult = new MethodResult<IList<CourseClassModel>>();

            List<Class> classes = await _classRepository.Queryable.Where(e => e.Status == EnumClassType.New && request.CourseIds.Contains(e.CourseId))
                                                            .ToListAsync(cancellationToken: cancellationToken);

            IList<CourseClassModel>? courseClassModels;
            if (classes == null || classes.Count == 0)
            {
                courseClassModels = new List<CourseClassModel>();
                foreach (var item in request.CourseIds)
                {
                    var code = await _mediator.Send(new GetNewClassCodeQuery { CourseLevel = request.CourseLevel }, cancellationToken).ConfigureAwait(false);
                    var courseClass = new CourseClassModel { CourseId = item, Code = code.Result };
                    courseClassModels.Add(courseClass);
                }
                methodResult.Result = courseClassModels;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            methodResult.Result = _mapper.Map<IList<CourseClassModel>>(classes);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
