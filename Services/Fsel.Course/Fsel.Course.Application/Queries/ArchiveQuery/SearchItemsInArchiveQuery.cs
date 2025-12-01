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
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IExtraPracticeRepository _extraPracticeRepository;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly IFinalTestRepository _finalTestRepository;
        private readonly IPlacementTestRepository _placementTestRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IVideoRepository _videoRepository;

        public SearchItemsInArchiveQueryHandler(ICourseRepository courseRepository, ILessonRepository lessonRepository, IUnitRepository unitRepository, IHomeWorkRepository homeWorkRepository, IExtraPracticeRepository extraPracticeRepository, IMockTestRepository mockTestRepository, IFinalTestRepository finalTestRepository, IPlacementTestRepository placementTestRepository, IMapper mapper, IUserService userService, IVideoRepository videoRepository)
        {
            _courseRepository = courseRepository;
            _lessonRepository = lessonRepository;
            _unitRepository = unitRepository;
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
                await SearchCourseArchive(request, methodResult, cancellationToken);
            }
            else if (request.ObjectName == nameof(Lesson))
            {
                await SearchLessonArchive(request, methodResult, cancellationToken);
            }
            else if (request.ObjectName == nameof(Domain.Entities.Unit))
            {
                await SearchUnitArchive(request, methodResult, cancellationToken);
            }
            else if (request.ObjectName == nameof(Video))
            {
                await SearchVideoArchive(request, methodResult, cancellationToken);
            }
            else if (request.ObjectName == nameof(HomeWork))
            {
                await SearchHomeworkArchive(request, methodResult, cancellationToken);
            }
            else if (request.ObjectName == nameof(ExtraPractice))
            {
                await SearchExtraPracticeArchive(request, methodResult, cancellationToken);
            }
            else if (request.ObjectName == nameof(MockTest))
            {
                await SearchMockTestArchive(request, methodResult, cancellationToken);
            }
            else if (request.ObjectName == nameof(FinalTest))
            {
                await SearchFinalTestArchive(request, methodResult, cancellationToken);
            }
            else if (request.ObjectName == nameof(PlacementTest))
            {
                await SearchPlacementTestArchive(request, methodResult, cancellationToken);
            }
            return methodResult;
        }

        private async Task SearchLessonArchive(SearchItemsInArchiveQueryModel request, MethodResult<PagingItemsModel<object>> methodResult, CancellationToken cancellationToken)
        {
            var lesson = await _lessonRepository.GetListByPageAsync<LessonArchiveModel>(_lessonRepository.Queryable.Where(x => x.IsArchive).Include(lv => lv.LessonVideos).ThenInclude(v => v.Video).ThenInclude(vtc => vtc!.VideoTimeCodes)
                .Where(ft => string.IsNullOrEmpty(request.Keyword) || (ft.Name ?? string.Empty).Contains(request.Keyword ?? string.Empty))
                .Where(lev => !request.Level.HasValue || lev.CourseLevel == request.Level)
                .Where(ti => request.TeacherIds == null || ti.LessonVideos.Where(vd => vd.Video != null).Select(x => x.Video!.TeacherId).Any(rq => request.TeacherIds.Contains(rq))), request, cancellationToken);

            var teacherIds = lesson.Items?.Select(x => x.TeacherId).ToList();
            var teachersResult = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = teacherIds });
            var teachers = teachersResult.Content?.Result;
            lesson.Items.ForEach(x =>
            {
                x.TeacherName = teachers?.FirstOrDefault(n => n.Id == x.TeacherId)?.User?.FullName;
            });
            methodResult.Result = new PagingItemsModel<object> { Items = _mapper.Map<IList<object>>(lesson.Items), PagingInfo = lesson.PagingInfo };
        }

        private async Task SearchCourseArchive(SearchItemsInArchiveQueryModel request, MethodResult<PagingItemsModel<object>> methodResult, CancellationToken cancellationToken)
        {
            var course = await _courseRepository.GetListByPageAsync<CourseArchiveModel>(_courseRepository.Queryable.Where(x => x.IsArchive)
                .Where(k => string.IsNullOrEmpty(request.Keyword) || (k.Code ?? string.Empty).Contains(request.Keyword ?? string.Empty))
                .Where(n => !request.Level.HasValue || n.CourseLevel == request.Level)
                .Where(m => request.TeacherIds == null || m.CourseTeachers.Any(e => request.TeacherIds.Contains(e.TeacherId))).Include(p => p.CourseTeachers), request, cancellationToken);
            var teacherIds = course.Items?.Where(n => n.CourseTeachers != null).SelectMany(p => p.CourseTeachers!).Select(x => x.TeacherId).ToList();
            var teachersResult = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = teacherIds });
            var teachers = teachersResult.Content?.Result;
            course.Items.ForEach(x =>
            {
                x.CourseTeachers.ForEach(p =>
                {
                    p.FullName = teachers?.FirstOrDefault(n => n.Id == p.TeacherId)?.User?.FullName;
                });
            });
            methodResult.Result = new PagingItemsModel<object> { Items = _mapper.Map<IList<object>>(course.Items), PagingInfo = course.PagingInfo };
        }

        private async Task SearchUnitArchive(SearchItemsInArchiveQueryModel request, MethodResult<PagingItemsModel<object>> methodResult, CancellationToken cancellationToken)
        {
            var unit = await _unitRepository.GetListByPageAsync<UnitArchiveModel>(_unitRepository.Queryable.Where(x => x.IsArchive).Include(cum => cum.CourseUnitMockTests).ThenInclude(c => c.Course).ThenInclude(ct => ct!.CourseTeachers)
                .Where(k => string.IsNullOrEmpty(request.Keyword) || (k.Code ?? string.Empty).Contains(request.Keyword ?? string.Empty))
                .Where(lv => request.Level == null || lv.CourseLevel == request.Level)
                .Where(tis => request.TeacherIds == null || tis.CourseUnitMockTests.Select(c => c.Course).SelectMany(ct => ct!.CourseTeachers).Select(ti => ti.TeacherId).Any(rq => request.TeacherIds.Contains(rq))), request, cancellationToken);
            var teacherIds = unit.Items?.Where(n => n.Teachers != null).SelectMany(p => p.Teachers!).Select(x => x.TeacherId).ToList();
            var teachersResult = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = teacherIds });
            var teachers = teachersResult.Content?.Result;
            unit.Items.ForEach(x =>
            {
                x.Teachers.ForEach(p =>
                {
                    p.TeacherName = teachers?.FirstOrDefault(n => n.Id == p.TeacherId)?.User?.FullName;
                });
            });
            methodResult.Result = new PagingItemsModel<object> { Items = _mapper.Map<IList<object>>(unit.Items), PagingInfo = unit.PagingInfo };
        }

        private async Task SearchVideoArchive(SearchItemsInArchiveQueryModel request, MethodResult<PagingItemsModel<object>> methodResult, CancellationToken cancellationToken)
        {
            var video = await _videoRepository.GetListByPageAsync<VideoArchiveModel>(_videoRepository.Queryable.Where(x => x.IsArchive).Include(vt => vt!.VideoTimeCodes).ThenInclude(te => te.TimeCodeExercises).ThenInclude(e => e.Exercise)
                .Where(k => string.IsNullOrEmpty(request.Keyword) || (k.Name ?? string.Empty).Contains(request.Keyword ?? string.Empty))
                .Where(p => request.TeacherIds == null || request.TeacherIds.Any(h => h == p.TeacherId))
                .Where(j => request.Level == null || request.Level == j.CourseLevel)
                .Where(k => request.TimeCodeType == null || k.VideoTimeCodes.Select(l => l.TimeCodeType).Any(c => c == request.TimeCodeType)), request, cancellationToken);
            methodResult.Result = new PagingItemsModel<object> { Items = _mapper.Map<IList<object>>(video.Items), PagingInfo = video.PagingInfo };
        }

        private async Task SearchHomeworkArchive(SearchItemsInArchiveQueryModel request, MethodResult<PagingItemsModel<object>> methodResult, CancellationToken cancellationToken)
        {
            var homework = await _homeWorkRepository.GetListByPageAsync<HomeworkArchiveModel>(_homeWorkRepository.Queryable.Where(x => x.IsArchive)
                .Where(k => string.IsNullOrEmpty(request.Keyword) || (k.Code ?? string.Empty).Contains(request.Keyword ?? string.Empty))
                .Where(n => request.Level == null || n.CourseLevel == request.Level)
                .Where(n => request.Skill == null || n.CourseSkill == request.Skill), request, cancellationToken);
            methodResult.Result = new PagingItemsModel<object> { Items = _mapper.Map<IList<object>>(homework.Items), PagingInfo = homework.PagingInfo };
        }

        private async Task SearchExtraPracticeArchive(SearchItemsInArchiveQueryModel request, MethodResult<PagingItemsModel<object>> methodResult, CancellationToken cancellationToken)
        {
            var extraPractice = await _extraPracticeRepository.GetListByPageAsync<ExtraPracticeArchiveModel>(_extraPracticeRepository.Queryable.Where(x => x.IsArchive)
                .Where(k => string.IsNullOrEmpty(request.Keyword) || (k.Code ?? string.Empty).Contains(request.Keyword ?? string.Empty))
                .Where(l => !request.Level.HasValue || l.CourseLevel == request.Level)
                .Where(t => !request.Type.HasValue || t.Type == request.Type), request, cancellationToken);
            methodResult.Result = new PagingItemsModel<object> { Items = _mapper.Map<IList<object>>(extraPractice.Items), PagingInfo = extraPractice.PagingInfo };
        }

        private async Task SearchMockTestArchive(SearchItemsInArchiveQueryModel request, MethodResult<PagingItemsModel<object>> methodResult, CancellationToken cancellationToken)
        {
            var mockTest = await _mockTestRepository.GetListByPageAsync<MockTestArchiveModel>(_mockTestRepository.Queryable.Where(x => x.IsArchive).Include(mts => mts.MockTestSections).ThenInclude(sg => sg.SectionGroup)
                .Where(k => string.IsNullOrEmpty(request.Keyword) || (k.Name ?? string.Empty).Contains(request.Keyword ?? string.Empty))
                .Where(t => !request.MockTestType.HasValue || t.MockTestType == request.MockTestType)
                .Where(s => !request.Skill.HasValue || s.MockTestSections.Select(mt => mt.SectionGroup!.CourseSkill).Contains(request.Skill.Value)), request, cancellationToken);
            methodResult.Result = new PagingItemsModel<object> { Items = _mapper.Map<IList<object>>(mockTest.Items), PagingInfo = mockTest.PagingInfo };
        }

        private async Task SearchFinalTestArchive(SearchItemsInArchiveQueryModel request, MethodResult<PagingItemsModel<object>> methodResult, CancellationToken cancellationToken)
        {
            var finalTest = await _finalTestRepository.GetListByPageAsync<FinalTestArchiveModel>(_finalTestRepository.Queryable.Where(x => x.IsArchive).Include(fts => fts.FinalTestSections).ThenInclude(sg => sg.SectionGroup)
                .Where(k => string.IsNullOrEmpty(request.Keyword) || (k.Name ?? string.Empty).Contains(request.Keyword ?? string.Empty))
                .Where(lv => !request.FinalTestLevel.HasValue || lv.FinalTestLevel == request.FinalTestLevel)
                .Where(s => !request.Skill.HasValue || s.FinalTestSections.Select(mt => mt.SectionGroup!.CourseSkill).Contains(request.Skill.Value)), request, cancellationToken);
            methodResult.Result = new PagingItemsModel<object> { Items = _mapper.Map<IList<object>>(finalTest.Items), PagingInfo = finalTest.PagingInfo };
        }

        private async Task SearchPlacementTestArchive(SearchItemsInArchiveQueryModel request, MethodResult<PagingItemsModel<object>> methodResult, CancellationToken cancellationToken)
        {
            var placementTest = await _placementTestRepository.GetListByPageAsync<PlacementTestArchiveModel>(_placementTestRepository.Queryable.Where(x => x.IsArchive)
                .Where(k => string.IsNullOrEmpty(request.Keyword) || (k.Name ?? string.Empty).Contains(request.Keyword ?? string.Empty))
                .Where(n => request.PlacementTestLevel == null || n.Level == request.PlacementTestLevel), request, cancellationToken);
            methodResult.Result = new PagingItemsModel<object> { Items = _mapper.Map<IList<object>>(placementTest.Items), PagingInfo = placementTest.PagingInfo };
        }
    }
}
