// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AdminCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Entities.BeginnerGuideConfigs;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ToolUpdateBeginnerGuideStudentCommand : IRequest<MethodResult<bool>>
    {
    }

    public class ToolUpdateBeginnerGuideStudentCommandHandler : IRequestHandler<ToolUpdateBeginnerGuideStudentCommand, MethodResult<bool>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly ILmsCourseService _lmsCourseService;

        public ToolUpdateBeginnerGuideStudentCommandHandler(IStudentRepository studentRepository, ILmsCourseService lmsCourseService)
        {
            _studentRepository = studentRepository;
            _lmsCourseService = lmsCourseService;
        }

        public async Task<MethodResult<bool>> Handle(ToolUpdateBeginnerGuideStudentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            var listStudent = new List<Student>();

            var students = await _studentRepository.Queryable.ToListAsync(cancellationToken);
            var paramBeginnerGuidesResult = await _lmsCourseService.GetParamBegginnerGuide(string.Join(",", students.Select(x => x.Id)));
            if (!paramBeginnerGuidesResult.IsSuccessStatusCode)
            {
                methodResult.AddError(paramBeginnerGuidesResult.Error);
                return methodResult;
            }

            var paramBeginnerGuides = paramBeginnerGuidesResult.Content?.Result;
            foreach (var student in students)
            {
                var paramBeginnerGuide = paramBeginnerGuides?.Where(x => x.StudentId == student.Id).FirstOrDefault();
                if (paramBeginnerGuide == null)
                {
                    continue;
                }
                var other = student.BeginnerGuide?.Other;

                if (paramBeginnerGuide.IsDoneOnePT)
                {
                    other = GetBeginnerGuideOther(EnumCheckPoint.DoneOnePlacementTest, other);
                }
                if (paramBeginnerGuide.IsDonePT)
                {
                    other = GetBeginnerGuideOther(EnumCheckPoint.LevelSelection, other);
                }
                if (paramBeginnerGuide.IsDoneVideo)
                {
                    other = GetBeginnerGuideOther(EnumCheckPoint.DoneVideo, other);
                }
                if (paramBeginnerGuide.IsDoneClassForum)
                {
                    other = GetBeginnerGuideOther(EnumCheckPoint.DoneClassForum, other);
                }
                if (paramBeginnerGuide.IsDoneHomeWork)
                {
                    other = GetBeginnerGuideOther(EnumCheckPoint.DoneHomeWork, other);
                }
                if (student.BeginnerGuide != null)
                {
                    student.BeginnerGuide = new StudentBeginnerGuide
                    {
                        Other = other,
                        BeginnerGuides = student.BeginnerGuide.BeginnerGuides?.ToList(),
                        QuestionTypes = student.BeginnerGuide.QuestionTypes?.ToList()
                    };
                }
                else
                {
                    student.BeginnerGuide = new StudentBeginnerGuide
                    {
                        Other = other,
                        BeginnerGuides = new List<EnumBeginnerGuide>(),
                        QuestionTypes = new List<EnumQuestionType>()
                    };
                }
            }
            _studentRepository.UpdateList(students);
            await _studentRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private enum EnumCheckPoint
        {
            DoneOnePlacementTest,
            LevelSelection,
            DoneVideo,
            DoneClassForum,
            DoneHomeWork
        }

        private string? GetBeginnerGuideOther(EnumCheckPoint checkPoint, string? other)
        {
            var data = datas.Where(x => x.Key == checkPoint).Select(x => x.Value).ToList();
            var dataOthers = other?.Split(",").ToList();
            var others = data.Where(x => dataOthers == null || !dataOthers.Any(y => y == x)).ToList();
            other = string.Concat(other, ",", string.Join(",", others));
            return other;
        }

        private IList<KeyValuePair<EnumCheckPoint, string>> datas = new List<KeyValuePair<EnumCheckPoint, string>>
        {
              // web
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneOnePlacementTest, "placement-test-module"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneOnePlacementTest, "placement-test-module_2"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneOnePlacementTest, "placement-test-exercise-0"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneOnePlacementTest, "placement-test-exercise-1"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneOnePlacementTest, "placement-test-exercise-2"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneOnePlacementTest, "placement-test-exercise-3"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneOnePlacementTest, "placement-test-exercise-4"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneOnePlacementTest, "placement-test-exercise-5"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneOnePlacementTest, "placement-test-exercise-6"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneOnePlacementTest, "placement-test-exercise-7"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneOnePlacementTest, "placement-test-exercise-8"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneOnePlacementTest, "placement-test-module-submit"),

              // mobile
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneOnePlacementTest, "keyTestProgressModule"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneOnePlacementTest, "keyPlacementTest"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneOnePlacementTest, "keyTestProgressModule"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneOnePlacementTest, "keyTabMediaPort"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneOnePlacementTest, "showCaseTabQuestions"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneOnePlacementTest, "showCaseQuestionMark"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneOnePlacementTest, "showCaseQuestionTime"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneOnePlacementTest, "showCaseQuestionList"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneOnePlacementTest, "showCaseBodyQuestionList"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneOnePlacementTest, "showCaseQuestionBottomBar"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneOnePlacementTest, "showCaseQuestionBottomBar2"),

              // web
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.LevelSelection, "placement-test-select-level"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.LevelSelection, "placement-test-select-level-two"),

              // web
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneVideo, "techie_welcome_planet"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneVideo, "i18n_techie_lesson_overview_two"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneVideo, "i18n_techie_lesson_content_step1"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneVideo, "techie_lesson_content_step2"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneVideo, "techie_lesson_content_step3"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneVideo, "techie_lesson_content_video"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneVideo, "techie_lesson_content_video_step2"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneVideo, "techie_lesson_content_video3"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneVideo, "techie_video_exercises_step1"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneVideo, "techie_video_exercises_step2"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneVideo, "techie_video_exercises_step5"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneVideo, "i18n_techie_video_exercises_step6"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneVideo, "techie_video_exercises_done_step2"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneVideo, "techie_video_exercises_step4"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneVideo, "i18n_techie_lesson_ClassForumLock_step1"),

              //mobile
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneVideo, "showCasePlanet"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneVideo, "showCaseVideo"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneVideo, "showCaseControl"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneVideo, "showCaseListEx"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneVideo, "showCaseIconNoteFist"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneVideo, "showCaseReviewLesson"),

              // web
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneClassForum, "i18n_techie_lesson_page_classForum_step2"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneClassForum, "techie_class_forum_welcome"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneClassForum, "i18n_beginnerGuide_classForum_step2"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneClassForum, "i18n_beginnerGuide_classForum_step3"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneClassForum, "i18n_beginnerGuide_classForum_step4"),

              //mobile
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneClassForum, "showCaseClassForumFist"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneClassForum, "showCaseFormClassForumLast"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneClassForum, "keyPromptChatGPT1"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneClassForum, "keyPromptChatGPT2"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneClassForum, "showCaseClassMate"),

              //web
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneHomeWork, "i18n_beginnerGuide_classForum_step5"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneHomeWork, "i18n_beginnerGuide_homeWork_step1"),

              //mmobile
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneHomeWork, "showCaseHomeWorkFist"),
              new KeyValuePair<EnumCheckPoint, string>( EnumCheckPoint.DoneHomeWork, "showCaseHomeWorkItem")
        };
    }
}
