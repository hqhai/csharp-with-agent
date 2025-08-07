using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Shared.Enums;
using Fsel.Shared.Models.ShareModels;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Lms.Application.Queries.CourseQuery.V1i1
{
    public class GetCourseForChooseLevelQuery : IRequest<MethodResult<CourseForChooseLevelModel>>
    {
        public Guid StudentId { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
    }

    public class GetCourseForChooseLevelQueryHandler : IRequestHandler<GetCourseForChooseLevelQuery, MethodResult<CourseForChooseLevelModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseResultRepository _courseResultRepository;

        public GetCourseForChooseLevelQueryHandler(ICourseRepository courseRepository, ICourseResultRepository courseResultRepository)
        {
            _courseRepository = courseRepository;
            _courseResultRepository = courseResultRepository;
        }

        public async Task<MethodResult<CourseForChooseLevelModel>> Handle(GetCourseForChooseLevelQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CourseForChooseLevelModel>();
            var course = await _courseRepository.Queryable.Where(p => p.CourseLevel == request.CourseLevel && p.Status == EnumCourseStatus.Active).OrderByDescending(p => p.CreatedDate).FirstOrDefaultAsync(cancellationToken);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }
            var isHasCourseResult = await _courseResultRepository.Queryable.AnyAsync(p => p.StudentId == request.StudentId && p.WorkingStatus == EnumWorkingStatus.Active, cancellationToken);
            methodResult.Result = new CourseForChooseLevelModel()
            {
                CourseId = course.Id,
                IsHasCourseResult = isHasCourseResult
            };
            return methodResult;
        }
    }
}
