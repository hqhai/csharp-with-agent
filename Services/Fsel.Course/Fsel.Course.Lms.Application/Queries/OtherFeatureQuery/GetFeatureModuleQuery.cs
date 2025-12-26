// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.OtherFeatureQuery
{
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetFeatureModuleQuery : IRequest<MethodResult<FeatureModuleModel>>
    {
        public EnumFeatureModule FeatureModule { get; set; }
        public Guid ObjectId { get; set; }

        public Guid? UserId { get; set; }
    }

    public class GetFeatureModuleQueryHandler : IRequestHandler<GetFeatureModuleQuery, MethodResult<FeatureModuleModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly AuthContext _authContext;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IFinalTestRepository _finalTestRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IClassForumDetailResultRepository _classForumDetailResultRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IUnitLessonRepository _unitLessonRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        private readonly ILessonVideoRepository _lessonVideoRepository;
        private readonly ILessonHomeWorkRepository _lessonHomeWorkRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly IUserService _userService;

        public GetFeatureModuleQueryHandler(ICourseRepository courseRepository,
            AuthContext authContext,
            ICourseResultRepository courseResultRepository,
            IUnitRepository unitRepository,
            IUnitResultRepository unitResultRepository,
            IMockTestResultRepository mockTestResultRepository,
            IMockTestRepository mockTestRepository,
            IFinalTestResultRepository finalTestResultRepository,
            IFinalTestRepository finalTestRepository,
            ILessonRepository lessonRepository,
            ILessonResultRepository lessonResultRepository,
            IVideoRepository videoRepository,
            IVideoResultRepository videoResultRepository,
            IClassForumDetailResultRepository classForumDetailResultRepository,
            IClassForumRepository classForumRepository,
            IClassForumResultRepository classForumResultRepository,
            IHomeWorkRepository homeWorkRepository,
            IHomeWorkResultRepository homeWorkResultRepository,
            IUnitLessonRepository unitLessonRepository,
            ICourseUnitMockTestRepository courseUnitMockTestRepository,
            ILessonVideoRepository lessonVideoRepository,
            ILessonHomeWorkRepository lessonHomeWorkRepository,
            ISectionGroupRepository sectionGroupRepository,
            ISectionGroupResultRepository sectionGroupResultRepository,
            IUserService userService)
        {
            _courseRepository = courseRepository;
            _authContext = authContext;
            _courseResultRepository = courseResultRepository;
            _unitRepository = unitRepository;
            _unitResultRepository = unitResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _mockTestRepository = mockTestRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _finalTestRepository = finalTestRepository;
            _lessonRepository = lessonRepository;
            _lessonResultRepository = lessonResultRepository;
            _videoRepository = videoRepository;
            _videoResultRepository = videoResultRepository;
            _classForumDetailResultRepository = classForumDetailResultRepository;
            _classForumRepository = classForumRepository;
            _classForumResultRepository = classForumResultRepository;
            _homeWorkRepository = homeWorkRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
            _unitLessonRepository = unitLessonRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
            _lessonVideoRepository = lessonVideoRepository;
            _lessonHomeWorkRepository = lessonHomeWorkRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _userService = userService;
        }

        public async Task<MethodResult<FeatureModuleModel>> Handle(GetFeatureModuleQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<FeatureModuleModel> methodResult = new MethodResult<FeatureModuleModel>();
            var userId = !request.UserId.HasValue ? _authContext.CurrentUserId : request.UserId.Value;
            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(userId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var courseResult = await _courseResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == student.Id && x.WorkingStatus == EnumWorkingStatus.Active, cancellationToken);
            if (courseResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(courseResult));
                return methodResult;
            }
            var featureModule = new FeatureModuleModel
            {
                CourseId = courseResult.CourseId,
                CourseResultId = courseResult.Id,
                StudentId = courseResult.StudentId
            };

            switch (request.FeatureModule)
            {
                case EnumFeatureModule.Course:
                    var course = await _courseRepository.GetByIdAsync(request.ObjectId);
                    if (course == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                        return methodResult;
                    }
                    if (course.Id != courseResult.CourseId)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseRequestNotActive), nameof(course));
                        return methodResult;
                    }
                    break;

                case EnumFeatureModule.CourseResult:
                    courseResult = await _courseResultRepository.GetByIdAsync(request.ObjectId);
                    if (courseResult == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(courseResult));
                        return methodResult;
                    }
                    break;

                case EnumFeatureModule.Unit:
                    var unit = await _unitRepository.GetByIdAsync(request.ObjectId);
                    if (unit == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(unit));
                        return methodResult;
                    }
                    featureModule = await GetFeatureModuleToUnit(featureModule, unit.Id);
                    break;

                case EnumFeatureModule.UnitResult:
                    var unitResult = await _unitResultRepository.Queryable.Include(x => x.Unit)
                                          .FirstOrDefaultAsync(x => x.Id == request.ObjectId, cancellationToken);
                    if (unitResult == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(unitResult));
                        return methodResult;
                    }
                    featureModule = await GetFeatureModuleToUnit(featureModule, unitResult.UnitId);
                    break;

                case EnumFeatureModule.MockTest:
                    var mockTest = await _mockTestRepository.GetByIdAsync(request.ObjectId);
                    if (mockTest == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTest));
                        return methodResult;
                    }
                    featureModule = await GetFeatureModuleToMockTest(featureModule, mockTest.Id);
                    break;

                case EnumFeatureModule.MockTestResult:
                    var mockTestResult = await _mockTestResultRepository.GetByIdAsync(request.ObjectId);
                    if (mockTestResult == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTestResult));
                        return methodResult;
                    }
                    featureModule.MockTestResultId = mockTestResult.Id;
                    featureModule = await GetFeatureModuleToMockTest(featureModule, mockTestResult.MockTestId);
                    break;

                case EnumFeatureModule.FinalTest:
                    var finalTest = await _finalTestRepository.GetByIdAsync(request.ObjectId);
                    if (finalTest == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(finalTest));
                        return methodResult;
                    }
                    featureModule = await GetFeatureModuleToFinalTest(featureModule, finalTest.Id);
                    break;

                case EnumFeatureModule.FinalTestResult:
                    var finalTestResult = await _finalTestResultRepository.GetByIdAsync(request.ObjectId);
                    if (finalTestResult == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(finalTestResult));
                        return methodResult;
                    }
                    featureModule.FinalTestResultId = finalTestResult.Id;
                    featureModule = await GetFeatureModuleToFinalTest(featureModule, finalTestResult.FinalTestId);
                    break;

                case EnumFeatureModule.SectionGroup:
                    var sectionGroup = await _sectionGroupRepository.GetByIdAsync(request.ObjectId);
                    if (sectionGroup == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroup));
                        return methodResult;
                    }
                    featureModule = await GetFeatureModuleToSectionGroup(featureModule, sectionGroup.Id);
                    break;

                case EnumFeatureModule.SectionGroupResult:
                    var sectionGroupResult = await _sectionGroupResultRepository.GetByIdAsync(request.ObjectId);
                    if (sectionGroupResult == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroupResult));
                        return methodResult;
                    }
                    featureModule.FinalTestResultId = sectionGroupResult.FinalTestResultId;
                    featureModule.MockTestResultId = sectionGroupResult.MockTestResultId;
                    featureModule = await GetFeatureModuleToSectionGroup(featureModule, sectionGroupResult.SectionGroupId);
                    break;

                case EnumFeatureModule.Lesson:
                    var lesson = await _lessonRepository.GetByIdAsync(request.ObjectId);
                    if (lesson == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lesson));
                        return methodResult;
                    }
                    featureModule = await GetFeatureModuleToLesson(featureModule, lesson.Id);
                    break;

                case EnumFeatureModule.LessonResult:
                    var lessonResult = await _lessonResultRepository.GetByIdAsync(request.ObjectId);
                    if (lessonResult == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lessonResult));
                        return methodResult;
                    }
                    featureModule.UnitId = lessonResult.UnitId;
                    featureModule = await GetFeatureModuleToLesson(featureModule, lessonResult.LessonId);
                    break;

                case EnumFeatureModule.Video:
                    var video = await _videoRepository.GetByIdAsync(request.ObjectId);
                    if (video == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(video));
                        return methodResult;
                    }
                    featureModule = await GetFeatureModuleToVideo(featureModule, video.Id);
                    break;

                case EnumFeatureModule.VideoResult:
                    var videoResult = await _videoResultRepository.GetByIdAsync(request.ObjectId);
                    if (videoResult == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult));
                        return methodResult;
                    }
                    featureModule.LessonResultId = videoResult.LessonResultId;
                    featureModule = await GetFeatureModuleToVideo(featureModule, videoResult.VideoId);
                    break;

                case EnumFeatureModule.HomeWork:
                    var homeWork = await _homeWorkRepository.GetByIdAsync(request.ObjectId);
                    if (homeWork == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWork));
                        return methodResult;
                    }
                    featureModule = await GetFeatureModuleToHomeWork(featureModule, homeWork.Id);
                    break;

                case EnumFeatureModule.HomeWorkResult:
                    var homeWorkResult = await _homeWorkResultRepository.GetByIdAsync(request.ObjectId);
                    if (homeWorkResult == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWorkResult));
                        return methodResult;
                    }
                    featureModule.LessonResultId = homeWorkResult.LessonResultId;
                    featureModule = await GetFeatureModuleToVideo(featureModule, homeWorkResult.HomeWorkId);
                    break;

                case EnumFeatureModule.ClassForum:
                    var classForum = await _classForumRepository.GetByIdAsync(request.ObjectId);
                    if (classForum == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForum));
                        return methodResult;
                    }
                    featureModule = await GetFeatureModuleToClassForum(featureModule, classForum.Id);
                    break;

                case EnumFeatureModule.ClassForumResult:
                    var classForumResult = await _classForumResultRepository.GetByIdAsync(request.ObjectId);
                    if (classForumResult == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForumResult));
                        return methodResult;
                    }
                    featureModule.LessonResultId = classForumResult.LessonResultId;
                    featureModule = await GetFeatureModuleToClassForum(featureModule, classForumResult.ClassForumId);
                    break;

                case EnumFeatureModule.ClassForumDetailResult:
                    var classForumDetailResult = await _classForumDetailResultRepository.GetByIdAsync(request.ObjectId);
                    if (classForumDetailResult == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForumDetailResult));
                        return methodResult;
                    }
                    var classForumResultDto = await _classForumResultRepository.GetByIdAsync(classForumDetailResult.ClassForumResultId);
                    if (classForumResultDto == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForumResult));
                        return methodResult;
                    }
                    featureModule.ClassForumDetailResultId = classForumDetailResult.Id;
                    featureModule.LessonResultId = classForumResultDto.LessonResultId;
                    featureModule = await GetFeatureModuleToClassForum(featureModule, classForumResultDto.ClassForumId);
                    break;

                default:
                    break;
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = featureModule;
            return methodResult;
        }

        private async Task<FeatureModuleModel> GetFeatureModuleToUnit(FeatureModuleModel featureModule, Guid unitId)
        {
            var unit = await _unitRepository.Queryable.Include(x => x.UnitResults.Where(x => x.CourseId == featureModule.CourseId && x.StudentId == featureModule.StudentId))
                                                   .FirstOrDefaultAsync(x => x.Id == unitId);
            if (unit == null)
            {
                return featureModule;
            }
            featureModule.UnitId = unit.Id;
            featureModule.UnitResultId = unit.UnitResults.FirstOrDefault()?.Id;
            return featureModule;
        }

        private async Task<FeatureModuleModel> GetFeatureModuleToLesson(FeatureModuleModel featureModule, Guid lessonId)
        {
            var lesson = await _lessonRepository.Queryable.Include(x => x.LessonResults
                                                                        .Where(x => x.CourseId == featureModule.CourseId && x.StudentId == featureModule.StudentId)
                                                                        .Where(x => !featureModule.UnitId.HasValue || x.UnitId == featureModule.UnitId)
                                                                        .OrderBy(x => x.CreatedDate)
                                                                  )
                                                          .FirstOrDefaultAsync(x => x.Id == lessonId);
            if (lesson == null)
            {
                return featureModule;
            }
            var lessonResult = lesson.LessonResults.FirstOrDefault();
            featureModule.LessonId = lesson.Id;
            featureModule.LessonResultId = lesson.LessonResults.FirstOrDefault()?.Id;
            if (lessonResult != null)
            {
                featureModule = await GetFeatureModuleToUnit(featureModule, lessonResult.UnitId);
            }
            return featureModule;
        }

        private async Task<FeatureModuleModel> GetFeatureModuleToSectionGroup(FeatureModuleModel featureModule, Guid sectionGroupId)
        {
            var sectionGroup = await _sectionGroupRepository.Queryable.Include(x => x.MockTestSections)
                                                                      .Include(x => x.FinalTestSections)
                                                                      .FirstOrDefaultAsync(x => x.Id == sectionGroupId);
            if (sectionGroup == null)
            {
                return featureModule;
            }
            var mockTestId = sectionGroup.MockTestSections.FirstOrDefault()?.MockTestId;
            var finalTestId = sectionGroup.FinalTestSections.FirstOrDefault()?.FinalTestId;
            if (sectionGroup.MockTestSections.Any() && mockTestId.HasValue)
            {
                featureModule = await GetFeatureModuleToMockTest(featureModule, mockTestId.Value);
            }
            else if (sectionGroup.FinalTestSections.Any() && finalTestId.HasValue)
            {
                featureModule = await GetFeatureModuleToFinalTest(featureModule, finalTestId.Value);
            }
            var sectionGroupResult = await _sectionGroupResultRepository.Queryable.Where(x => !featureModule.MockTestResultId.HasValue || x.MockTestResultId == featureModule.MockTestResultId.Value)
                                                .Where(x => !featureModule.FinalTestResultId.HasValue || x.FinalTestResultId == featureModule.FinalTestResultId.Value)
                                                .FirstOrDefaultAsync(x => x.SectionGroupId == sectionGroup.Id && x.StudentId == featureModule.StudentId);
            featureModule.SectionGroupId = sectionGroup.Id;
            featureModule.SectionGroupResultId = sectionGroupResult?.Id;
            return featureModule;
        }

        private async Task<FeatureModuleModel> GetFeatureModuleToVideo(FeatureModuleModel featureModule, Guid videoId)
        {
            Guid lessonId;
            if (featureModule.LessonResultId.HasValue)
            {
                var lessonResult = await _lessonResultRepository.GetByIdAsync(featureModule.LessonResultId.Value);
                if (lessonResult == null)
                {
                    return featureModule;
                }
                featureModule.UnitId = lessonResult.UnitId;
                lessonId = lessonResult.LessonId;
            }
            else
            {
                lessonId = await (from baseQ in _lessonRepository.Queryable
                                  join lv in _lessonVideoRepository.Queryable on baseQ.Id equals lv.LessonId
                                  join ul in _unitLessonRepository.Queryable on baseQ.Id equals ul.LessonId
                                  join u in _unitRepository.Queryable on ul.UnitId equals u.Id
                                  join cum in _courseUnitMockTestRepository.Queryable on u.Id equals cum.UnitId
                                  where cum.CourseId == featureModule.CourseId && lv.VideoId == videoId
                                  orderby lv.CreatedDate
                                  select baseQ).Select(x => x.Id).FirstOrDefaultAsync();
            }
            featureModule = await GetFeatureModuleToLesson(featureModule, lessonId);
            var video = await _videoRepository.Queryable.Include(x => x.VideoResults.Where(x => x.LessonResultId == featureModule.LessonResultId))
                                                        .FirstOrDefaultAsync(x => x.Id == videoId);
            if (video == null)
            {
                return featureModule;
            }
            featureModule.VideoId = video.Id;
            featureModule.VideoResultId = video.VideoResults.FirstOrDefault()?.Id;
            return featureModule;
        }

        private async Task<FeatureModuleModel> GetFeatureModuleToHomeWork(FeatureModuleModel featureModule, Guid homeWorkId)
        {
            Guid lessonId;
            if (featureModule.LessonResultId.HasValue)
            {
                var lessonResult = await _lessonResultRepository.GetByIdAsync(featureModule.LessonResultId.Value);
                if (lessonResult == null)
                {
                    return featureModule;
                }
                featureModule.UnitId = lessonResult.UnitId;
                lessonId = lessonResult.LessonId;
            }
            else
            {
                lessonId = await (from baseQ in _lessonRepository.Queryable
                                  join lh in _lessonHomeWorkRepository.Queryable on baseQ.Id equals lh.LessonId
                                  join ul in _unitLessonRepository.Queryable on baseQ.Id equals ul.LessonId
                                  join u in _unitRepository.Queryable on ul.UnitId equals u.Id
                                  join cum in _courseUnitMockTestRepository.Queryable on u.Id equals cum.UnitId
                                  where cum.CourseId == featureModule.CourseId && lh.HomeWorkId == homeWorkId
                                  orderby lh.CreatedDate
                                  select baseQ).Select(x => x.Id).FirstOrDefaultAsync();
            }
            featureModule = await GetFeatureModuleToLesson(featureModule, lessonId);
            var homeWork = await _homeWorkRepository.Queryable.Include(x => x.HomeWorkResults.Where(x => x.LessonResultId == featureModule.LessonResultId && x.HomeWorkId == homeWorkId))
                                                              .FirstOrDefaultAsync(x => x.Id == homeWorkId);
            if (homeWork == null)
            {
                return featureModule;
            }
            featureModule.HomeWorkId = homeWork.Id;
            featureModule.HomeWorkResultId = homeWork.HomeWorkResults.FirstOrDefault()?.Id;
            return featureModule;
        }

        private async Task<FeatureModuleModel> GetFeatureModuleToClassForum(FeatureModuleModel featureModule, Guid classForumId)
        {
            Guid lessonId;
            if (featureModule.LessonResultId.HasValue)
            {
                var lessonResult = await _lessonResultRepository.GetByIdAsync(featureModule.LessonResultId.Value);
                if (lessonResult == null)
                {
                    return featureModule;
                }
                featureModule.UnitId = lessonResult.UnitId;
                lessonId = lessonResult.LessonId;
            }
            else
            {
                lessonId = await _classForumRepository.Queryable.Where(x => x.Id == classForumId && x.LessonId.HasValue).Select(x => x.LessonId!.Value).FirstOrDefaultAsync();
            }
            featureModule = await GetFeatureModuleToLesson(featureModule, lessonId);
            var classForum = await _classForumRepository.Queryable.Include(x => x.ClassForumResults.Where(x => x.LessonResultId == featureModule.LessonResultId))
                .FirstOrDefaultAsync(x => x.Id == classForumId);
            if (classForum == null)
            {
                return featureModule;
            }
            featureModule.ClassForumId = classForum.Id;
            featureModule.ClassForumResultId = classForum.ClassForumResults.FirstOrDefault()?.Id;
            if (featureModule.ClassForumResultId.HasValue)
            {
                featureModule.ClassForumDetailResultId = await GetClassFourmDetailResultIdAsync(featureModule.ClassForumResultId.Value);
            }
            return featureModule;
        }

        private async Task<Guid?> GetClassFourmDetailResultIdAsync(Guid classForumResultId)
        {
            var classForumDetailResult = await _classForumDetailResultRepository.Queryable.Where(x => x.ClassForumResultId == classForumResultId && x.Status == EnumClassForumResultStatus.Graded)
                                                                   .FirstOrDefaultAsync();
            return classForumDetailResult?.Id;
        }

        private async Task<FeatureModuleModel> GetFeatureModuleToFinalTest(FeatureModuleModel featureModule, Guid finalTestId)
        {
            var finalTest = await _finalTestRepository.Queryable.Include(x => x.FinalTestResults.Where(x => x.CourseId == featureModule.CourseId && x.StudentId == featureModule.StudentId)
                                                                                                .Where(x => !featureModule.FinalTestResultId.HasValue || x.Id == featureModule.FinalTestResultId))
                                                   .FirstOrDefaultAsync(x => x.Id == finalTestId);
            if (finalTest == null)
            {
                return featureModule;
            }
            featureModule.FinalTestId = finalTest.Id;
            featureModule.FinalTestResultId = finalTest.FinalTestResults.FirstOrDefault()?.Id;
            return featureModule;
        }

        private async Task<FeatureModuleModel> GetFeatureModuleToMockTest(FeatureModuleModel featureModule, Guid mockTestId)
        {
            var mockTest = await _mockTestRepository.Queryable.Include(x => x.MockTestResults.Where(x => x.CourseId == featureModule.CourseId && x.StudentId == featureModule.StudentId)
                                                                                             .Where(x => !featureModule.MockTestResultId.HasValue || x.Id == featureModule.MockTestResultId))
                                                            .FirstOrDefaultAsync(x => x.Id == mockTestId);
            if (mockTest == null)
            {
                return featureModule;
            }
            var mockTestResult = mockTest.MockTestResults.FirstOrDefault();
            featureModule.MockTestId = mockTest.Id;
            featureModule.MockTestResultId = mockTestResult?.Id;
            if (mockTest.MockTestType == EnumMockTestType.SkillMockTest && mockTestResult != null && mockTestResult.UnitId.HasValue)
            {
                return await GetFeatureModuleToMockTest(featureModule, mockTestResult.UnitId.Value);
            }
            return featureModule;
        }
    }
}
