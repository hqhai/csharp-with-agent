// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetLessonCommentByStudentQuery : IRequest<MethodResult<IList<LessonCommentByStudentModel>>>
    {
        public Guid StudentId { get; set; }
    }

    public class GetLessonCommentByStudentQueryHandler : IRequestHandler<GetLessonCommentByStudentQuery, MethodResult<IList<LessonCommentByStudentModel>>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;

        public GetLessonCommentByStudentQueryHandler(ILessonResultRepository lessonResultRepository)
        {
            _lessonResultRepository = lessonResultRepository;
        }

        public async Task<MethodResult<IList<LessonCommentByStudentModel>>> Handle(GetLessonCommentByStudentQuery request, CancellationToken cancellationToken)
        {
            MethodResult<IList<LessonCommentByStudentModel>> methodResult = new MethodResult<IList<LessonCommentByStudentModel>>();

            var lessonComments = await _lessonResultRepository.Queryable
                                .Include(x => x.Unit)
                                .Include(x => x.Lesson)
                                .Include(x => x.VideoResults)
                                .Where(x => x.VideoResult != null && x.StudentId == request.StudentId && x.VideoResult.StudentId == request.StudentId && x.VideoResult.Status == EnumResultStatus.Done)
                                .AsNoTracking()
                                .Select(x => new LessonCommentByStudentModel
                                {
                                    Id = x.Id,
                                    CreatedDate = x.CreatedDate,
                                    Feedback = x.VideoResult!.Feedback,
                                    LessonName = x.Lesson!.Name,
                                    NumberOfStars = x.VideoResult.NumberOfStars,
                                    UnitName = x.Unit!.Name,
                                }).ToListAsync(cancellationToken: cancellationToken);
            methodResult.Result = lessonComments;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
