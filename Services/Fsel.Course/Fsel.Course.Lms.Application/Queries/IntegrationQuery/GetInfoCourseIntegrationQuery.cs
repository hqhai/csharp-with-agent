// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.IntegrationQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.IntegrationModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetInfoCourseIntegrationQuery : IRequest<MethodResult<IList<InfoCourseIntegrationModel>>>
    {
        public IList<Guid>? UserIds { get; set; }
    }

    public class GetInfoCourseIntegrationQueryHandler : IRequestHandler<GetInfoCourseIntegrationQuery, MethodResult<IList<InfoCourseIntegrationModel>>>
    {
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IUnitLessonRepository _unitLessonRepository;

        public GetInfoCourseIntegrationQueryHandler(ICourseResultRepository courseResultRepository,
                                                    ILessonResultRepository lessonResultRepository,
                                                    IUnitLessonRepository unitLessonRepository)
        {
            _courseResultRepository = courseResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _unitLessonRepository = unitLessonRepository;
        }

        public async Task<MethodResult<IList<InfoCourseIntegrationModel>>> Handle(GetInfoCourseIntegrationQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.UserIds);
            MethodResult<IList<InfoCourseIntegrationModel>> methodResult = new MethodResult<IList<InfoCourseIntegrationModel>>();

            var courseResults = await _courseResultRepository.Queryable
                                                             .Include(x => x.Course)
                                                             .WhereBulkContains(request.UserIds, x => x.CreatedUserId)
                                                             .GroupBy(x => x.CreatedUserId)
                                                             .Select(x => new InfoCourseIntegrationModel
                                                             {
                                                                 UserId = x.Key,
                                                                 InfoCourseIntegrationDetails = x.Select(c => new InfoCourseIntegrationDetailModel
                                                                 {
                                                                     CourseId = c.CourseId,
                                                                     StudentId = c.StudentId,
                                                                     CourseName = c.Course != null ? c.Course.Name : default,
                                                                     StartDate = c.ProcessDate,
                                                                     EndDate = c.CompletionDate
                                                                 }).ToList()
                                                             })
                                                             .AsNoTracking()
                                                             .ToListAsync(cancellationToken);
            if (courseResults == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            foreach (var courseResult in courseResults)
            {
                await SetData(courseResult.InfoCourseIntegrationDetails, cancellationToken);
            }

            methodResult.Result = courseResults;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task SetData(IList<InfoCourseIntegrationDetailModel>? infoCourseIntegrationDetails, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(infoCourseIntegrationDetails);

            foreach (var item in infoCourseIntegrationDetails)
            {

                var lessonResult = await _lessonResultRepository.Queryable.AsNoTracking().FirstOrDefaultAsync(p => p.CourseId == item.CourseId && p.StudentId == item.StudentId, cancellationToken);

                var currentLesson = await _lessonResultRepository.Queryable.Include(x => x.Lesson).Include(x => x.Unit).AsNoTracking()
                                                                 .FirstOrDefaultAsync(p => p.CourseId == item.CourseId && p.StudentId == item.StudentId && p.Status == EnumResultStatus.Process, cancellationToken);

                item.TotalLessonDone = await _lessonResultRepository.Queryable.AsNoTracking()
                                                                    .CountAsync(p => p.CourseId == item.CourseId && p.StudentId == item.StudentId && p.Status == EnumResultStatus.Done, cancellationToken);

                item.TotalLesson = await _unitLessonRepository.Queryable.AsNoTracking()
                                                              .CountAsync(p => p.UnitId == (lessonResult != null ? lessonResult.UnitId : Guid.Empty), cancellationToken);

                item.NameCurrentUnit = currentLesson?.Unit?.Name;

                item.NameCurrentLesson = currentLesson?.Lesson?.Name;
            }
        }
    }
}
