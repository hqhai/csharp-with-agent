// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AdminCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
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
            foreach (var student in students)
            {
                var paramBeginnerGuideResult = await _lmsCourseService.GetParamBegginnerGuide(student.Id);
                if (!paramBeginnerGuideResult.IsSuccessStatusCode)
                {
                    listStudent.Add(student);
                    continue;
                }
                var paramBeginnerGuide = paramBeginnerGuideResult.Content?.Result;
                if (paramBeginnerGuide == null)
                {
                    listStudent.Add(student);
                    continue;
                }

                if (paramBeginnerGuide.IsDoneOnePT)
                {
                }
                if (paramBeginnerGuide.IsDonePT)
                {
                }
                if (paramBeginnerGuide.IsDoneVideo)
                {
                }
                if (paramBeginnerGuide.IsDoneClassForum)
                {
                }
                if (paramBeginnerGuide.IsDoneHomeWork)
                {
                }
            }
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

        private Dictionary<EnumCheckPoint, string> datas = new Dictionary<EnumCheckPoint, string>
        {
            { EnumCheckPoint.DoneOnePlacementTest, "placement-test-module"},
            { EnumCheckPoint.DoneOnePlacementTest, "placement-test-module_2"},
            { EnumCheckPoint.DoneOnePlacementTest, "placement-test-exercise-0"},
            { EnumCheckPoint.DoneOnePlacementTest, "placement-test-exercise-1"},
            { EnumCheckPoint.DoneOnePlacementTest, "placement-test-exercise-2"},
            { EnumCheckPoint.DoneOnePlacementTest, "placement-test-exercise-3"},
            { EnumCheckPoint.DoneOnePlacementTest, "placement-test-exercise-4"},
            { EnumCheckPoint.DoneOnePlacementTest, "placement-test-exercise-5"},
            { EnumCheckPoint.DoneOnePlacementTest, "placement-test-exercise-6"},
            { EnumCheckPoint.DoneOnePlacementTest, "placement-test-exercise-7"},
            { EnumCheckPoint.DoneOnePlacementTest, "placement-test-exercise-8"},
            { EnumCheckPoint.DoneOnePlacementTest, "placement-test-module-submit"},
            { EnumCheckPoint.LevelSelection, "placement-test-select-level"},
            { EnumCheckPoint.LevelSelection, "placement-test-select-level-two"},
            { EnumCheckPoint.DoneVideo, "techie_welcome_planet"},
            { EnumCheckPoint.DoneVideo, "i18n_techie_lesson_overview_two"},
            { EnumCheckPoint.DoneVideo, "i18n_techie_lesson_content_step1"},
            { EnumCheckPoint.DoneVideo, "techie_lesson_content_step2"},
            { EnumCheckPoint.DoneVideo, "techie_lesson_content_step3"},
            { EnumCheckPoint.DoneVideo, "techie_lesson_content_video"},
            { EnumCheckPoint.DoneVideo, "techie_lesson_content_video_step2"},
            { EnumCheckPoint.DoneVideo, "techie_lesson_content_video3"},
            { EnumCheckPoint.DoneVideo, "techie_video_exercises_step1"},
            { EnumCheckPoint.DoneVideo, "techie_video_exercises_step2"},
            { EnumCheckPoint.DoneVideo, "techie_video_exercises_step5"},
            { EnumCheckPoint.DoneVideo, "i18n_techie_video_exercises_step6"},
            { EnumCheckPoint.DoneVideo, "techie_video_exercises_done_step2"},
            { EnumCheckPoint.DoneVideo, "techie_video_exercises_step4"},
            { EnumCheckPoint.DoneVideo, "i18n_techie_lesson_ClassForumLock_step1"},
            { EnumCheckPoint.DoneClassForum, "i18n_techie_lesson_page_classForum_step2"},
            { EnumCheckPoint.DoneClassForum, "techie_class_forum_welcome"},
            { EnumCheckPoint.DoneClassForum, "i18n_beginnerGuide_classForum_step2"},
            { EnumCheckPoint.DoneClassForum, "i18n_beginnerGuide_classForum_step3"},
            { EnumCheckPoint.DoneClassForum, "i18n_beginnerGuide_classForum_step4"},
            { EnumCheckPoint.DoneHomeWork, "i18n_beginnerGuide_classForum_step5"},
            { EnumCheckPoint.DoneHomeWork, "i18n_beginnerGuide_homeWork_step1"}
        };
    }
}
