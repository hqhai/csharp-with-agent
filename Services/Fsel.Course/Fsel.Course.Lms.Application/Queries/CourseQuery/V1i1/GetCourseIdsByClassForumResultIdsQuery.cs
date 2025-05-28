using Fsel.Common.ActionResults;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using MediatR;

namespace Fsel.Course.Lms.Application.Queries.CourseQuery.V1i1
{
    public class GetCourseIdsByClassForumResultIdsQuery : IRequest<MethodResult<IList<CourseClassForumResultModel>>>
    {
        public IList<Guid>? ClassForumResultIds { get; set; }
    }

    public class GetCourseIdsByClassForumResultIdsQueryHandler : IRequestHandler<GetCourseIdsByClassForumResultIdsQuery, MethodResult<IList<CourseClassForumResultModel>>>
    {
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;

        public GetCourseIdsByClassForumResultIdsQueryHandler(IClassForumResultRepository classForumResultRepository, ILessonResultRepository lessonResultRepository)
        {
            _classForumResultRepository = classForumResultRepository;
            _lessonResultRepository = lessonResultRepository;
        }

        public async Task<MethodResult<IList<CourseClassForumResultModel>>> Handle(GetCourseIdsByClassForumResultIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<CourseClassForumResultModel>>();

            var query = from cfr in _classForumResultRepository.Queryable.WhereBulkContains(request.ClassForumResultIds, p => p.Id)
                        join lr in _lessonResultRepository.Queryable on cfr.LessonResultId equals lr.Id
                        select new CourseClassForumResultModel
                        {
                            ClassForumResultId = cfr.Id,
                            CourseId = lr.CourseId
                        };

            methodResult.Result = query.ToList();
            return methodResult;
        }
    }
}
