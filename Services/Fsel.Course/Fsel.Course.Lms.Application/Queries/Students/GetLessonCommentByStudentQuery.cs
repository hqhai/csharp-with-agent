// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.Students
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.UnitQuery;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetLessonCommentByStudentQuery : IRequest<MethodResult<IList<LessonCommentByStudentModel>>>
    {
        public Guid StudentId { get; set; }
    }

    public class GetLessonCommentByStudentQueryHandler : IRequestHandler<GetLessonCommentByStudentQuery, MethodResult<IList<LessonCommentByStudentModel>>>
    {
        private readonly IUnitResultRepository _unitResultRepository;

        public GetLessonCommentByStudentQueryHandler(IUnitResultRepository unitResultRepository)
        {
            _unitResultRepository = unitResultRepository;
        }

        public async Task<MethodResult<IList<LessonCommentByStudentModel>>> Handle(GetLessonCommentByStudentQuery request, CancellationToken cancellationToken)
        {
            MethodResult<IList<LessonCommentByStudentModel>> methodResult = new MethodResult<IList<LessonCommentByStudentModel>>();

            var unitQuery = _unitResultRepository.Queryable.Where(x => x.CourseId == request.StudentId)
                                .Select(x => new LessonCommentByStudentModel
                                {
                                    Id = x.Id,
                                    Percent = x.Percent,
                                    Status = x.Status,
                                    SkillScores = x.SkillScores,
                                    CorrectCount = x.CorrectCount,
                                    CorrectTotal = x.CorrectTotal,
                                    StudentId = studentId,
                                    UnitId = x.UnitId,
                                    CourseId = request.CourseId,
                                    CreatedDate = x.CreatedDate,
                                });

            methodResult.Result = await unitQuery.ToListAsync(cancellationToken: cancellationToken);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
