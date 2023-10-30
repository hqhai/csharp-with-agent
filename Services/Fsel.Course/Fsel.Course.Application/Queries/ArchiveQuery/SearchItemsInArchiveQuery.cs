// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.ArchiveQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Application.Services.UserServices;
    using Fsel.Course.Application.Services.UserServices.Models;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.ArchiveModels;
    using Fsel.Course.Domain.Models.QueryModels.Archives;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SearchItemsInArchiveQuery : SearchItemsInArchiveQueryModel, IRequest<MethodResult<PagingItemsModel<object>>>
    {
    }

    public class SearchItemsInArchiveQueryHandler : IRequestHandler<SearchItemsInArchiveQuery, MethodResult<PagingItemsModel<object>>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly ILessonVideoRepository _lessonVideoRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IExtraPracticeRepository _extraPracticeRepository;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly IFinalTestRepository _finalTestRepository;
        private readonly IPlacementTestRepository _placementTestRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IVideoRepository _videoRepository;
        public SearchItemsInArchiveQueryHandler(ICourseRepository courseRepository, ILessonRepository lessonRepository, IUnitRepository unitRepository, ILessonVideoRepository lessonVideoRepository, IHomeWorkRepository homeWorkRepository, IExtraPracticeRepository extraPracticeRepository, IMockTestRepository mockTestRepository, IFinalTestRepository finalTestRepository, IPlacementTestRepository placementTestRepository, IMapper mapper, IUserService userService, IVideoRepository videoRepository)
        {
            _courseRepository = courseRepository;
            _lessonRepository = lessonRepository;
            _unitRepository = unitRepository;
            _lessonVideoRepository = lessonVideoRepository;
            _homeWorkRepository = homeWorkRepository;
            _extraPracticeRepository = extraPracticeRepository;
            _mockTestRepository = mockTestRepository;
            _finalTestRepository = finalTestRepository;
            _placementTestRepository = placementTestRepository;
            _mapper = mapper;
            _userService = userService;
            _videoRepository = videoRepository;
        }

        public async Task<MethodResult<PagingItemsModel<object>>> Handle(SearchItemsInArchiveQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<PagingItemsModel<object>>();
            ArgumentNullException.ThrowIfNull(request);

            if (request.ObjectName == nameof(Course))
            {
                var course = await _courseRepository.GetListByPageAsync<ArchiveCourseModel>(_courseRepository.Queryable.IgnoreQueryFilters().Where(x => x.IsDeleted).Where(n => request.Level == null || n.CourseLevel == request.Level).Where(m => request.TeacherIds == null || m.CourseTeachers.Any(e => request.TeacherIds.Contains(e.TeacherId))).Include(p => p.CourseTeachers), request, cancellationToken);
                var teacherIds = course.Items?.Where(n => n.CourseTeachers != null).SelectMany(p => p.CourseTeachers!).Select(x => x.TeacherId).ToList();
                var teachersResult = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = teacherIds });
                var teachers = teachersResult.Content?.Result;
                course.Items.ForEach(x =>
                {
                    x.CourseTeachers.ForEach(p =>
                    {
                        p.FullName = teachers?.FirstOrDefault(n => n.Id == p.TeacherId)?.Human?.FullName;
                    });
                });
                methodResult.Result = new PagingItemsModel<object> { Items = _mapper.Map<IList<object>>(course.Items), PagingInfo = course.PagingInfo };
            }
            else if (request.ObjectName == nameof(Lesson))
            {
                var lesson = await _lessonRepository.GetListByPageAsync<ArchiveLessonModel>(_lessonRepository.Queryable.IgnoreQueryFilters().Where(x => x.IsDeleted).Include(lv => lv.LessonVideos).ThenInclude(v => v.Video).ThenInclude(vt => vt!.VideoTimeCodes).Where(n => request.Level == null || n.CourseLevel == request.Level).Where(p => request.TeacherIds == null || request.TeacherIds.Any(c => p.LessonVideos.Select(x => x.Video!.TeacherId).Contains(c))), request, cancellationToken);

                var teacherIds = lesson.Items?.Select(x => x.TeacherId).ToList();
                var teachersResult = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = teacherIds });
                var teachers = teachersResult.Content?.Result;
                lesson.Items.ForEach(x =>
                {
                    x.TeacherName = teachers?.FirstOrDefault(n => n.Id == x.TeacherId)?.Human?.FullName;
                });
                methodResult.Result = new PagingItemsModel<object> { Items = _mapper.Map<IList<object>>(lesson.Items), PagingInfo = lesson.PagingInfo };
            }
            else if (request.ObjectName == nameof(Domain.Entities.Unit))
            {
                var unit = await _unitRepository.GetListByPageAsync<ArchiveUnitModel>(_unitRepository.Queryable.IgnoreQueryFilters().Where(x => x.IsDeleted).Include(cum => cum.CourseUnitMockTests).ThenInclude(c => c.Course).ThenInclude(ct => ct!.CourseTeachers).Where(n => request.Level == null || n.CourseLevel == request.Level).Where(p => request.TeacherIds == null || request.TeacherIds.Any(a => p.CourseUnitMockTests.Select(s => s.Course).SelectMany(f => f!.CourseTeachers).Select(g => g.TeacherId).Contains(a))), request, cancellationToken);
                var teacherIds = unit.Items?.Where(n => n.Teachers != null).SelectMany(p => p.Teachers!).Select(x => x.TeacherId).ToList();
                var teachersResult = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = teacherIds });
                var teachers = teachersResult.Content?.Result;
                unit.Items.ForEach(x =>
                {
                    x.Teachers.ForEach(p =>
                    {
                        p.TeacherName = teachers?.FirstOrDefault(n => n.Id == p.TeacherId)?.Human?.FullName;
                    });
                });
                methodResult.Result = new PagingItemsModel<object> { Items = _mapper.Map<IList<object>>(unit.Items), PagingInfo = unit.PagingInfo };
            }
            else if (request.ObjectName == nameof(LessonVideo))
            {
                var unit = await _lessonVideoRepository.GetListByPageAsync<ArchiveVideoLessonModel>(_lessonVideoRepository.Queryable.IgnoreQueryFilters().Where(x => x.IsDeleted).Include(v => v.Video).ThenInclude(vt => vt!.VideoTimeCodes).ThenInclude(te => te.TimeCodeExercises).ThenInclude(e => e.Exercise).Where(p => request.TeacherIds == null || request.TeacherIds.Any(h => h == p.Video!.TeacherId)).Where(j => request.Level == null || request.Level == j.Video!.CourseLevel).Where(k => request.TimeCodeType == null || k.Video!.VideoTimeCodes.Select(l => l.TimeCodeType).Any(c => c == request.TimeCodeType)), request, cancellationToken);
                methodResult.Result = new PagingItemsModel<object> { Items = _mapper.Map<IList<object>>(unit.Items), PagingInfo = unit.PagingInfo };
            }
            return methodResult;
        }
    }
}
