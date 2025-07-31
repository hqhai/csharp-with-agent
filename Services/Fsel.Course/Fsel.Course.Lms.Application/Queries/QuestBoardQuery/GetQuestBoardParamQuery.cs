// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.QuestBoardQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetQuestBoardParamQuery : IRequest<MethodResult<QuestBoardParamModel>>
    {
        public EnumQuestBoardCategory Category { get; set; }
    }

    public class GetQuestBoardParamQueryHandler : IRequestHandler<GetQuestBoardParamQuery, MethodResult<QuestBoardParamModel>>
    {
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;

        public GetQuestBoardParamQueryHandler(ICourseResultRepository courseResultRepository,
                                              AuthContext authContext,
                                              IUserService userService,
                                              IUnitResultRepository unitResultRepository,
                                              ILessonResultRepository lessonResultRepository,
                                              IHomeWorkResultRepository homeWorkResultRepository,
                                              IFinalTestResultRepository finalTestResultRepository,
                                              IMockTestResultRepository mockTestResultRepository,
                                              IClassForumRepository classForumRepository,
                                              IVideoResultRepository videoResultRepository,
                                              IClassForumResultRepository classForumResultRepository)
        {
            _courseResultRepository = courseResultRepository;
            _authContext = authContext;
            _userService = userService;
            _unitResultRepository = unitResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _classForumRepository = classForumRepository;
            _videoResultRepository = videoResultRepository;
            _classForumResultRepository = classForumResultRepository;
        }

        public async Task<MethodResult<QuestBoardParamModel>> Handle(GetQuestBoardParamQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<QuestBoardParamModel>();

            var student = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode || student.Content?.Result == null)
            {
                methodResult.AddError(student.Error);
                return methodResult;
            }

            switch (request.Category)
            {
                case EnumQuestBoardCategory.CompleteTheFirstVideoLesson:

                    var result = await CompleteTheFirstVideoLessonHandler(student.Content.Result.Id);
                    if (!result.IsOK)
                    {
                        methodResult.AddErrorBadRequest(result.ErrorMessages.ToList());
                        return methodResult;
                    }

                    methodResult.Result = result.Result;

                    break;

                case EnumQuestBoardCategory.CompleteTheFirstClassForum:

                    var completeTheFirstClassForum = await CompleteTheFirstClassForumHandler(student.Content.Result.Id);
                    if (!completeTheFirstClassForum.IsOK)
                    {
                        methodResult.AddErrorBadRequest(completeTheFirstClassForum.ErrorMessages.ToList());
                        return methodResult;
                    }

                    methodResult.Result = completeTheFirstClassForum.Result;

                    break;

                case EnumQuestBoardCategory.CompleteHomeworkFirst:

                    var completeHomeworkFirst = await CompleteHomeworkFirstHandler(student.Content.Result.Id);
                    if (!completeHomeworkFirst.IsOK)
                    {
                        methodResult.AddErrorBadRequest(completeHomeworkFirst.ErrorMessages.ToList());
                        return methodResult;
                    }

                    methodResult.Result = completeHomeworkFirst.Result;

                    break;

                case EnumQuestBoardCategory.CompleteTheFirstUnit:

                    var completeTheFirstUnit = await CompleteTheFirstUnitHandler(student.Content.Result.Id);
                    if (!completeTheFirstUnit.IsOK)
                    {
                        methodResult.AddErrorBadRequest(completeTheFirstUnit.ErrorMessages.ToList());
                        return methodResult;
                    }

                    methodResult.Result = completeTheFirstUnit.Result;

                    break;

                case EnumQuestBoardCategory.ExploreTheLearningGalaxy:

                    var exploreTheLearningGalaxy = await ExploreTheLearningGalaxyHandler(student.Content.Result.Id);
                    if (!exploreTheLearningGalaxy.IsOK)
                    {
                        methodResult.AddErrorBadRequest(exploreTheLearningGalaxy.ErrorMessages.ToList());
                        return methodResult;
                    }

                    methodResult.Result = exploreTheLearningGalaxy.Result;

                    break;

                case EnumQuestBoardCategory.GalaxyNotes:

                    var galaxyNotes = await GalaxyNotesHandler(student.Content.Result.Id);
                    if (!galaxyNotes.IsOK)
                    {
                        methodResult.AddErrorBadRequest(galaxyNotes.ErrorMessages.ToList());
                        return methodResult;
                    }

                    methodResult.Result = galaxyNotes.Result;

                    break;

                case EnumQuestBoardCategory.JourneyOfKnowledge:

                    var journeyOfKnowledge = await ExploreTheLearningGalaxyHandler(student.Content.Result.Id);
                    if (!journeyOfKnowledge.IsOK)
                    {
                        methodResult.AddErrorBadRequest(journeyOfKnowledge.ErrorMessages.ToList());
                        return methodResult;
                    }

                    methodResult.Result = journeyOfKnowledge.Result;

                    break;

                case EnumQuestBoardCategory.MessagesFromAI:

                    var messagesFromAI = await MessagesFromAIHandler(student.Content.Result.Id);
                    if (!messagesFromAI.IsOK)
                    {
                        methodResult.AddErrorBadRequest(messagesFromAI.ErrorMessages.ToList());
                        return methodResult;
                    }

                    methodResult.Result = messagesFromAI.Result;

                    break;

                case EnumQuestBoardCategory.TheMysteryOfTheStars:

                    var theMysteryOfTheStars = await ExploreTheLearningGalaxyHandler(student.Content.Result.Id);
                    if (!theMysteryOfTheStars.IsOK)
                    {
                        methodResult.AddErrorBadRequest(theMysteryOfTheStars.ErrorMessages.ToList());
                        return methodResult;
                    }

                    methodResult.Result = theMysteryOfTheStars.Result;

                    break;

                case EnumQuestBoardCategory.LearningSpaceship:

                    var learningSpaceship = await ExploreTheLearningGalaxyHandler(student.Content.Result.Id);
                    if (!learningSpaceship.IsOK)
                    {
                        methodResult.AddErrorBadRequest(learningSpaceship.ErrorMessages.ToList());
                        return methodResult;
                    }

                    methodResult.Result = learningSpaceship.Result;

                    break;
            }

            return methodResult;
        }

        private async Task<MethodResult<QuestBoardParamModel>> CompleteTheFirstVideoLessonHandler(Guid studentId)
        {
            MethodResult<QuestBoardParamModel> methodResult = new MethodResult<QuestBoardParamModel>();

            var learn = await CurrenLearn(studentId);

            var newQuestBoardParamModel = new QuestBoardParamModel
            {
                CourseId = learn.CourseResult?.CourseId,
                UnitId = learn.UnitResult?.UnitId,
                NameUnit = learn.UnitResult?.Unit?.Name,
                DisplayOrder = learn.UnitResult?.Unit?.CourseUnitMockTests.FirstOrDefault(x => x.CourseId == learn.CourseResult?.CourseId && x.UnitId == learn.UnitResult.UnitId)?.DisplayOrder,
                LessonId = learn.LessonResult?.LessonId,
                LessonResultId = learn.LessonResult?.Id,
                ClassForumId = learn.ClassForum?.Id
            };

            if (learn.LessonResult != null && learn.LessonResult.Status == EnumResultStatus.Process)
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.Video;
            }
            else if (learn.CourseResult != null && learn.LessonResult != null && learn.CourseResult.Status == EnumResultStatus.Process)
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.Lesson;
            }
            else
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.Course;
            }

            methodResult.Result = newQuestBoardParamModel;
            return methodResult;
        }

        private async Task<MethodResult<QuestBoardParamModel>> CompleteTheFirstClassForumHandler(Guid studentId)
        {
            MethodResult<QuestBoardParamModel> methodResult = new MethodResult<QuestBoardParamModel>();

            var learn = await CurrenLearn(studentId);

            var newQuestBoardParamModel = new QuestBoardParamModel
            {
                CourseId = learn.CourseResult?.CourseId,
                UnitId = learn.UnitResult?.UnitId,
                NameUnit = learn.UnitResult?.Unit?.Name,
                DisplayOrder = learn.UnitResult?.Unit?.CourseUnitMockTests.FirstOrDefault(x => x.CourseId == learn.CourseResult?.CourseId && x.UnitId == learn.UnitResult.UnitId)?.DisplayOrder,
                LessonId = learn.LessonResult?.LessonId,
                LessonResultId = learn.LessonResult?.Id,
                ClassForumId = learn.ClassForum?.Id
            };

            if (learn.ClassForum != null)
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.ClassForum;
            }
            else if (learn.LessonResult != null && learn.LessonResult.Status == EnumResultStatus.Process)
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.Video;
            }
            else if (learn.CourseResult != null && learn.LessonResult != null && learn.CourseResult.Status == EnumResultStatus.Process)
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.Lesson;
            }
            else
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.Course;
            }

            methodResult.Result = newQuestBoardParamModel;
            return methodResult;
        }

        private async Task<MethodResult<QuestBoardParamModel>> CompleteHomeworkFirstHandler(Guid studentId)
        {
            MethodResult<QuestBoardParamModel> methodResult = new MethodResult<QuestBoardParamModel>();

            var learn = await CurrenLearn(studentId);

            var newQuestBoardParamModel = new QuestBoardParamModel
            {
                CourseId = learn.CourseResult?.CourseId,
                UnitId = learn.UnitResult?.UnitId,
                NameUnit = learn.UnitResult?.Unit?.Name,
                DisplayOrder = learn.UnitResult?.Unit?.CourseUnitMockTests.FirstOrDefault(x => x.CourseId == learn.CourseResult?.CourseId && x.UnitId == learn.UnitResult.UnitId)?.DisplayOrder,
                LessonId = learn.LessonResult?.LessonId,
                LessonResultId = learn.LessonResult?.Id,
                ClassForumId = learn.ClassForum?.Id,
                HomeWorkId = learn.HomeWorkResult?.HomeWorkId
            };

            if (learn.HomeWorkResult != null)
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.HomeWork;
            }
            else if (learn.LessonResult != null && learn.LessonResult.Status == EnumResultStatus.Process)
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.Video;
            }
            else if (learn.CourseResult != null && learn.LessonResult != null && learn.CourseResult.Status == EnumResultStatus.Process)
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.Lesson;
            }
            else
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.Course;
            }

            methodResult.Result = newQuestBoardParamModel;
            return methodResult;
        }

        private async Task<MethodResult<QuestBoardParamModel>> CompleteTheFirstUnitHandler(Guid studentId)
        {
            MethodResult<QuestBoardParamModel> methodResult = new MethodResult<QuestBoardParamModel>();

            var learn = await CurrenLearn(studentId);

            var newQuestBoardParamModel = new QuestBoardParamModel
            {
                CourseId = learn.CourseResult?.CourseId,
                UnitId = learn.UnitResult?.UnitId,
                NameUnit = learn.UnitResult?.Unit?.Name,
                DisplayOrder = learn.UnitResult?.Unit?.CourseUnitMockTests.FirstOrDefault(x => x.CourseId == learn.CourseResult?.CourseId && x.UnitId == learn.UnitResult.UnitId)?.DisplayOrder,
            };

            if (learn.UnitResult != null || learn.CourseResult != null)
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.Course;
            }

            methodResult.Result = newQuestBoardParamModel;
            return methodResult;
        }

        private async Task<MethodResult<QuestBoardParamModel>> ExploreTheLearningGalaxyHandler(Guid studentId)
        {
            MethodResult<QuestBoardParamModel> methodResult = new MethodResult<QuestBoardParamModel>();

            var learn = await CurrenLearn(studentId);

            var newQuestBoardParamModel = new QuestBoardParamModel
            {
                CourseId = learn.CourseResult?.CourseId,
                UnitId = learn.UnitResult?.UnitId,
                NameUnit = learn.UnitResult?.Unit?.Name,
                DisplayOrder = learn.UnitResult?.Unit?.CourseUnitMockTests.FirstOrDefault(x => x.CourseId == learn.CourseResult?.CourseId && x.UnitId == learn.UnitResult.UnitId)?.DisplayOrder,
                LessonId = learn.LessonResult?.LessonId,
                LessonResultId = learn.LessonResult?.Id,
                ClassForumId = learn.ClassForum?.Id,
                HomeWorkId = learn.HomeWorkResult?.HomeWorkId,
                MockTestId = learn.SkillMockTestResult?.MockTestId
            };

            var type = learn.CourseResult?.Course?.CourseLevel.GetEnumCourseType();

            if (learn.FinalTestResult != null || learn.FullMockTestResult != null)
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.Course;
            }
            else if (learn.HomeWorkResult != null)
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.HomeWork;
            }
            else if (learn.ClassForum != null)
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.ClassForum;
            }
            else if (learn.LessonResult != null && learn.LessonResult.Status == EnumResultStatus.Process)
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.Video;
            }
            else if ((learn.CourseResult != null && learn.CourseResult.Status == EnumResultStatus.Process) && (learn.LessonResult != null || learn.SkillMockTestResult != null))
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.Lesson;
            }
            else
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.Course;
            }

            methodResult.Result = newQuestBoardParamModel;
            return methodResult;
        }

        private async Task<MethodResult<QuestBoardParamModel>> GalaxyNotesHandler(Guid studentId)
        {
            MethodResult<QuestBoardParamModel> methodResult = new MethodResult<QuestBoardParamModel>();

            var learn = await CurrenLearn(studentId);

            var newQuestBoardParamModel = new QuestBoardParamModel
            {
                CourseId = learn.CourseResult?.CourseId,
                UnitId = learn.UnitResult?.UnitId,
                NameUnit = learn.UnitResult?.Unit?.Name,
                DisplayOrder = learn.UnitResult?.Unit?.CourseUnitMockTests.FirstOrDefault(x => x.CourseId == learn.CourseResult?.CourseId && x.UnitId == learn.UnitResult.UnitId)?.DisplayOrder,
                LessonId = learn.LessonResult?.LessonId,
                LessonResultId = learn.LessonResult?.Id
            };

            if (learn.LessonResult != null && learn.LessonResult.Status == EnumResultStatus.Process)
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.Video;
            }
            else
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.Course;
            }

            methodResult.Result = newQuestBoardParamModel;
            return methodResult;
        }

        private async Task<MethodResult<QuestBoardParamModel>> MessagesFromAIHandler(Guid studentId)
        {
            MethodResult<QuestBoardParamModel> methodResult = new MethodResult<QuestBoardParamModel>();

            var classForumResult = await _classForumResultRepository.Queryable
                                                                    .Include(x => x.ClassForumDetailResults)
                                                                    .Include(x => x.LessonResult)
                                                                    .Where(x => x.StudentId == studentId && x.ClassForumDetailResults.Any(c => !string.IsNullOrEmpty(c.GradingAlFeedback)))
                                                                    .OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                                                                    .FirstOrDefaultAsync();

            if (classForumResult != null)
            {
                var newQuestBoardParamModel = new QuestBoardParamModel
                {
                    CourseId = classForumResult.LessonResult?.CourseId,
                    UnitId = classForumResult.LessonResult?.UnitId,
                    LessonId = classForumResult.LessonResult?.LessonId,
                    LessonResultId = classForumResult.LessonResult?.Id,
                    ClassForumId = classForumResult.ClassForumId,
                    FeatureModule = EnumFeatureModule.ClassForum
                };

                methodResult.Result = newQuestBoardParamModel;
            }
            else
            {
                var messagesFromAI = await ExploreTheLearningGalaxyHandler(studentId);
                if (!messagesFromAI.IsOK)
                {
                    methodResult.AddErrorBadRequest(messagesFromAI.ErrorMessages.ToList());
                    return methodResult;
                }

                methodResult.Result = messagesFromAI.Result;
            }

            return methodResult;
        }

        private async Task<(CourseResult? CourseResult, UnitResult? UnitResult, LessonResult? LessonResult, MockTestResult? SkillMockTestResult, ClassForum? ClassForum, HomeWorkResult? HomeWorkResult, FinalTestResult? FinalTestResult, MockTestResult? FullMockTestResult)> CurrenLearn(Guid studentId)
        {
            CourseResult? courseResult = null;
            UnitResult? unitResult = null;
            LessonResult? lessonResult = null;
            ClassForum? classForum = null;
            HomeWorkResult? homeWorkResult = null;
            FinalTestResult? finalTestResult = null;
            MockTestResult? fullMockTestResult = null;
            MockTestResult? skillMockTestResult = null;

            var currentCourse = await _courseResultRepository.Queryable
                                                             .Include(x => x.Course)
                                                             .Where(x => x.StudentId == studentId && (x.Status == EnumResultStatus.Process || x.Status == EnumResultStatus.New))
                                                             .OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                                                             .FirstOrDefaultAsync();
            if (currentCourse == null)
            {
                return (null, null, null, null, null, null, null, null);
            }

            var currentUnit = await _unitResultRepository.Queryable
                                                         .Include(x => x.Unit)
                                                         .ThenInclude(x => x.CourseUnitMockTests)
                                                         .Where(x => x.StudentId == studentId && x.CourseId == currentCourse.CourseId && (x.Status == EnumResultStatus.Process || x.Status == EnumResultStatus.New))
                                                         .OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                                                         .FirstOrDefaultAsync();

            if (currentUnit != null)
            {
                var currentLesson = await _lessonResultRepository.Queryable
                                                                 .Where(x => x.StudentId == studentId && x.CourseId == currentCourse.CourseId && x.UnitId == currentUnit.UnitId && (x.Status == EnumResultStatus.Process || x.Status == EnumResultStatus.New))
                                                                 .OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                                                                 .FirstOrDefaultAsync();

                if (currentLesson != null)
                {
                    if (await _videoResultRepository.Queryable.AnyAsync(x => x.LessonResultId == currentLesson.Id && x.Status == EnumResultStatus.Done))
                    {
                        var currentCLassForum = await _classForumRepository.Queryable
                                                                           .FirstOrDefaultAsync(x => x.LessonId == currentLesson.LessonId);

                        classForum = currentCLassForum;
                    }

                    var currentHomeWork = await _homeWorkResultRepository.Queryable
                                                                         .Where(x => x.StudentId == studentId && x.LessonResultId == currentLesson.Id && (x.Status == EnumResultStatus.Process || x.Status == EnumResultStatus.New))
                                                                         .OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                                                                         .FirstOrDefaultAsync();

                    homeWorkResult = currentHomeWork;
                }

                var currentSkillMockTest = await _mockTestResultRepository.Queryable
                                                                          .Where(x => x.StudentId == studentId && x.CourseId == currentCourse.CourseId && x.UnitId.HasValue && x.UnitId == currentUnit.UnitId && (x.Status == EnumResultStatus.Process || x.Status == EnumResultStatus.New))
                                                                          .OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                                                                          .FirstOrDefaultAsync();

                lessonResult = currentLesson;
                skillMockTestResult = currentSkillMockTest;
            }

            var currentFinalTest = await _finalTestResultRepository.Queryable
                                                                   .Where(x => x.StudentId == studentId && x.CourseId == currentCourse.CourseId && (x.Status == EnumResultStatus.Process || x.Status == EnumResultStatus.New))
                                                                   .OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                                                                   .FirstOrDefaultAsync();

            var currentFullMockTest = await _mockTestResultRepository.Queryable
                                                                     .Where(x => x.StudentId == studentId && x.CourseId == currentCourse.CourseId && !x.UnitId.HasValue && (x.Status == EnumResultStatus.Process || x.Status == EnumResultStatus.New))
                                                                     .OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                                                                     .FirstOrDefaultAsync();

            courseResult = currentCourse;
            unitResult = currentUnit;
            finalTestResult = currentFinalTest;
            fullMockTestResult = currentFullMockTest;

            return (courseResult, unitResult, lessonResult, skillMockTestResult, classForum, homeWorkResult, finalTestResult, fullMockTestResult);
        }
    }
}
