// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CurriculumQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetCurriculumByIdQuery : IRequest<MethodResult<CurriculumModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetCurriculumByIdQueryHandler : IRequestHandler<GetCurriculumByIdQuery, MethodResult<CurriculumModel>>
    {
        private readonly ICurriculumRepository _curriculumRepository;
        private readonly IMapper _mapper;
        private readonly ICourseRepository _courseRepository;

        public GetCurriculumByIdQueryHandler(ICurriculumRepository curriculumRepository, IMapper mapper, ICourseRepository courseRepository)
        {
            _curriculumRepository = curriculumRepository;
            _mapper = mapper;
            _courseRepository = courseRepository;
        }

        public async Task<MethodResult<CurriculumModel>> Handle(GetCurriculumByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CurriculumModel>();

            var curriculum = await (from baseQuery in _curriculumRepository.Queryable
                                    join c in _courseRepository.Queryable on baseQuery.CourseId equals c.Id
                                    join cc in _courseRepository.Queryable on baseQuery.CourseCloneId equals cc.Id
                                    where baseQuery.Id == request.Id
                                    select new
                                    {
                                        Curriculum = baseQuery,
                                        Course = c,
                                        CourseClone = cc,
                                    }).FirstOrDefaultAsync(cancellationToken);
            if (curriculum == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCurriculumErrorCode.CurriculumDoesNotExist), nameof(curriculum), request.Id);
                return methodResult;
            }

            var curriculumModel = _mapper.Map<CurriculumModel>(curriculum.Curriculum);
            curriculumModel.CourseName = curriculum.Course.Name;
            curriculumModel.CourseType = curriculum.Course.CourseType;
            curriculumModel.CourseLevel = curriculum.Course.CourseLevel;
            curriculumModel.Subject = "Tiếng Anh";

            methodResult.Result = curriculumModel;
            return methodResult;
        }
    }
}
