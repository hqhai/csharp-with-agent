// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CurriculumQuery
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetCurriculumsByStudentIdQuery : IRequest<MethodResult<IList<CurriculumModel>>>
    {
        public Guid StudentId { get; set; }
        public EnumCurriculumStatus? Status { get; set; }
        public bool? IsDone { get; set; }
    }

    public class GetCurriculumsByStudentIdQueryHandler : IRequestHandler<GetCurriculumsByStudentIdQuery, MethodResult<IList<CurriculumModel>>>
    {
        private readonly ICurriculumStudentRepository _curriculumStudentRepository;
        private readonly ICurriculumRepository _curriculumRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ICourseRepository _courseRepository;

        public GetCurriculumsByStudentIdQueryHandler(ICurriculumStudentRepository curriculumStudentRepository, ICurriculumRepository curriculumRepository, ILessonResultRepository lessonResultRepository, ICourseResultRepository courseResultRepository, ICourseRepository courseRepository)
        {
            _curriculumStudentRepository = curriculumStudentRepository;
            _curriculumRepository = curriculumRepository;
            _lessonResultRepository = lessonResultRepository;
            _courseResultRepository = courseResultRepository;
            _courseRepository = courseRepository;
        }

        public async Task<MethodResult<IList<CurriculumModel>>> Handle(GetCurriculumsByStudentIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<CurriculumModel>>();

            var query = await (from baseQuery in _curriculumStudentRepository.Queryable
                               join cu in _curriculumRepository.Queryable on baseQuery.CurriculumId equals cu.Id
                               join c in _courseRepository.Queryable on cu.CourseId equals c.Id
                               join cc in _courseRepository.Queryable on cu.CourseCloneId equals cc.Id
                               where baseQuery.StudentId == request.StudentId
                               select new
                               {
                                   CurriculumStudent = baseQuery,
                                   Curriculum = cu,
                                   Course = c,
                                   CourseClone = cc
                               }).ToListAsync(cancellationToken);

            if (request.Status.HasValue)
            {
                query = query.Where(p => p.Curriculum.CurriculumStatus == request.Status).ToList();
            }

            var courseIds = query.Select(p => p.CourseClone.Id).ToList();

            var courseResults = await _courseResultRepository.Queryable.WhereBulkContains(courseIds, p => p.CourseId).Where(x => x.StudentId == request.StudentId).ToListAsync(cancellationToken);

            var curriculums = query.Select(p => new CurriculumModel()
            {
                Id = p.Curriculum.Id,
                CurriculumName = p.Curriculum.CurriculumName,
                CourseId = p.Curriculum.CourseId,
                CourseCloneId = p.Curriculum.CourseCloneId,
                CreatedDate = p.CurriculumStudent.CreatedDate,
                CourseName = p.Course.Name,
                StartDate = p.Curriculum.StartDate,
                EndDate = p.Curriculum.EndDate,
                CourseLevel = p.Course.CourseLevel,
                CourseType = p.Course.CourseType,
                Subject = "Tiếng Anh"
            }).OrderByDescending(x => x.CreatedDate).ToList();

            curriculums.ForEach(p =>
            {
                var courseResult = courseResults.FirstOrDefault(x => x.CourseId == p.CourseCloneId);
                p.IsDone = courseResult != null && courseResult.Status == Domain.Enums.EnumResultStatus.Done;
            });

            if (request.IsDone.HasValue)
            {
                curriculums = curriculums.Where(p => p.IsDone == request.IsDone).ToList();
            }

            methodResult.Result = curriculums;
            return methodResult;
        }
    }
}
