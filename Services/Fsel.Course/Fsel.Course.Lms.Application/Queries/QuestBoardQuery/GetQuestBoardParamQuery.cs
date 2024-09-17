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
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;

        public GetQuestBoardParamQueryHandler(ICourseResultRepository courseResultRepository,
                                              AuthContext authContext,
                                              IUserService userService,
                                              IUnitResultRepository unitResultRepository,
                                              ILessonResultRepository lessonResultRepository,
                                              IClassForumResultRepository classForumResultRepository,
                                              IHomeWorkResultRepository homeWorkResultRepository,
                                              IFinalTestResultRepository finalTestResultRepository,
                                              IMockTestResultRepository mockTestResultRepository)
        {
            _courseResultRepository = courseResultRepository;
            _authContext = authContext;
            _userService = userService;
            _unitResultRepository = unitResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _classForumResultRepository = classForumResultRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
        }

        public async Task<MethodResult<QuestBoardParamModel>> Handle(GetQuestBoardParamQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<QuestBoardParamModel>();

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
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

                    var galaxyNotes = await CompleteTheFirstVideoLessonHandler(student.Content.Result.Id);
                    if (!galaxyNotes.IsOK)
                    {
                        methodResult.AddErrorBadRequest(galaxyNotes.ErrorMessages.ToList());
                        return methodResult;
                    }

                    methodResult.Result = galaxyNotes.Result;

                    break;

                case EnumQuestBoardCategory.JourneyOfKnowledge:

                    var journeyOfKnowledge = await CompleteTheFirstVideoLessonHandler(student.Content.Result.Id);
                    if (!journeyOfKnowledge.IsOK)
                    {
                        methodResult.AddErrorBadRequest(journeyOfKnowledge.ErrorMessages.ToList());
                        return methodResult;
                    }

                    methodResult.Result = journeyOfKnowledge.Result;

                    break;
                case EnumQuestBoardCategory.MessagesFromAI:

                    var messagesFromAI = await CompleteTheFirstClassForumHandler(student.Content.Result.Id);
                    if (!messagesFromAI.IsOK)
                    {
                        methodResult.AddErrorBadRequest(messagesFromAI.ErrorMessages.ToList());
                        return methodResult;
                    }

                    methodResult.Result = messagesFromAI.Result;

                    break;

                case EnumQuestBoardCategory.TheMysteryOfTheStars:

                    var theMysteryOfTheStars = await CompleteHomeworkFirstHandler(student.Content.Result.Id);
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
                LessonId = learn.LessonResult?.LessonId,
                LessonResulId = learn.LessonResult?.Id
            };

            if (learn.LessonResult != null)
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.Video;
            }
            else if (learn.UnitResult != null)
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.Unit;
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
                LessonId = learn.LessonResult?.LessonId,
                LessonResulId = learn.LessonResult?.Id,
                ClassForumId = learn.ClassForumResult?.ClassForumId,
            };

            if (learn.ClassForumResult != null)
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.ClassForum;
            }

            else if (learn.LessonResult != null)
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.Video;
            }
            else if (learn.UnitResult != null)
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.Unit;
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
                LessonId = learn.LessonResult?.LessonId,
                LessonResulId = learn.LessonResult?.Id,
                ClassForumId = learn.ClassForumResult?.ClassForumId
            };

            if (learn.ClassForumResult != null)
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.HomeWork;
            }

            else if (learn.LessonResult != null)
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.Video;
            }
            else if (learn.UnitResult != null)
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.Unit;
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
                UnitId = learn.UnitResult?.UnitId
            };

            if (learn.UnitResult != null)
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.Unit;
            }
            else
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
                LessonId = learn.LessonResult?.LessonId,
                LessonResulId = learn.LessonResult?.Id,
                ClassForumId = learn.ClassForumResult?.ClassForumId,
                HomeWorkId = learn.HomeWorkResult?.HomeWorkId
            };

            var type = learn.CourseResult?.Course?.CourseLevel.GetEnumCourseType();

            if (learn.FinalTestResult != null || learn.MockTestResult != null)
            {
                if (type == EnumCourseType.Academic && learn.FinalTestResult != null)
                {
                    newQuestBoardParamModel.FinalTestId = learn.FinalTestResult.FinalTestId;
                    newQuestBoardParamModel.FeatureModule = EnumFeatureModule.FinalTest;
                }

                if (type == EnumCourseType.Ielts && learn.MockTestResult != null)
                {
                    newQuestBoardParamModel.MockTestId = learn.MockTestResult.MockTestId;
                    newQuestBoardParamModel.FeatureModule = EnumFeatureModule.MockTest;
                }
            }

            else if (learn.HomeWorkResult != null)
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.HomeWork;
            }

            else if (learn.ClassForumResult != null)
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.ClassForum;
            }

            else if (learn.LessonResult != null)
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.Video;
            }

            else if (learn.UnitResult != null)
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.Unit;
            }

            else
            {
                newQuestBoardParamModel.FeatureModule = EnumFeatureModule.Course;
            }

            methodResult.Result = newQuestBoardParamModel;
            return methodResult;
        }

        private async Task<(CourseResult? CourseResult, UnitResult? UnitResult, LessonResult? LessonResult, ClassForumResult? ClassForumResult, HomeWorkResult? HomeWorkResult, FinalTestResult? FinalTestResult, MockTestResult? MockTestResult)> CurrenLearn(Guid studentId)
        {
            CourseResult? courseResult = null;
            UnitResult? unitResult = null;
            LessonResult? lessonResult = null;
            ClassForumResult? classForumResult = null;
            HomeWorkResult? homeWorkResult = null;
            FinalTestResult? finalTestResult = null;
            MockTestResult? mockTestResult = null;

            var currentCourse = await _courseResultRepository.Queryable
                                                             .Include(x => x.Course)
                                                             .Where(x => x.StudentId == studentId && (x.Status == EnumResultStatus.Process || x.Status == EnumResultStatus.New))
                                                             .OrderByDescending(x => x.CreatedDate)
                                                             .ThenByDescending(x => x.UpdatedDate)
                                                             .FirstOrDefaultAsync();
            if (currentCourse == null)
            {
                return (null, null, null, null, null, null, null);
            }

            var currentUnit = await _unitResultRepository.Queryable
                                                         .Where(x => x.StudentId == studentId && x.CourseId == currentCourse.CourseId && (x.Status == EnumResultStatus.Process || x.Status == EnumResultStatus.New))
                                                         .OrderByDescending(x => x.CreatedDate)
                                                         .ThenByDescending(x => x.UpdatedDate)
                                                         .FirstOrDefaultAsync();

            if (currentUnit != null)
            {
                var currentLesson = await _lessonResultRepository.Queryable
                                                                 .Where(x => x.StudentId == studentId && x.CourseId == currentCourse.CourseId && x.UnitId == currentUnit.UnitId && (x.Status == EnumResultStatus.Process || x.Status == EnumResultStatus.New))
                                                                 .OrderByDescending(x => x.CreatedDate)
                                                                 .ThenByDescending(x => x.UpdatedDate)
                                                                 .FirstOrDefaultAsync();

                if (currentLesson != null)
                {
                    var currentCLassForum = await _classForumResultRepository.Queryable
                                                                             .Where(x => x.StudentId == studentId && x.LessonResultId == currentLesson.Id)
                                                                             .OrderByDescending(x => x.CreatedDate)
                                                                             .ThenByDescending(x => x.UpdatedDate)
                                                                             .FirstOrDefaultAsync();

                    var currentHomeWork = await _homeWorkResultRepository.Queryable
                                                                         .Where(x => x.StudentId == studentId && x.LessonResultId == currentLesson.Id && (x.Status == EnumResultStatus.Process || x.Status == EnumResultStatus.New))
                                                                         .OrderByDescending(x => x.CreatedDate)
                                                                         .ThenByDescending(x => x.UpdatedDate)
                                                                         .FirstOrDefaultAsync();

                    classForumResult = currentCLassForum;
                    homeWorkResult = currentHomeWork;
                }

                lessonResult = currentLesson;
            }

            var currentFinalTest = await _finalTestResultRepository.Queryable
                                                                   .Where(x => x.StudentId == studentId && x.CourseId == currentCourse.CourseId && (x.Status == EnumResultStatus.Process || x.Status == EnumResultStatus.New))
                                                                   .OrderByDescending(x => x.CreatedDate)
                                                                   .ThenByDescending(x => x.UpdatedDate)
                                                                   .FirstOrDefaultAsync();

            var currentMockTest = await _mockTestResultRepository.Queryable
                                                                 .Where(x => x.StudentId == studentId && x.CourseId == currentCourse.CourseId && (x.Status == EnumResultStatus.Process || x.Status == EnumResultStatus.New))
                                                                 .OrderByDescending(x => x.CreatedDate)
                                                                 .ThenByDescending(x => x.UpdatedDate)
                                                                 .FirstOrDefaultAsync();

            courseResult = currentCourse;
            unitResult = currentUnit;
            finalTestResult = currentFinalTest;
            mockTestResult = currentMockTest;

            return (courseResult, unitResult, lessonResult, classForumResult, homeWorkResult, finalTestResult, mockTestResult);
        }
    }
}
