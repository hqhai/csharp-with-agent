// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Entities.BeginnerGuideConfigs;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateStudentBeginnerGuideCommand : UpdateStudentBeginnerGuideCommandModel, IRequest<MethodResult<StudentModel>>
    {
    }

    public class UpdateStudentBeginnerGuideCommandHandler : IRequestHandler<UpdateStudentBeginnerGuideCommand, MethodResult<StudentModel>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;

        public UpdateStudentBeginnerGuideCommandHandler(IStudentRepository studentRepository, IMapper mapper, AuthContext authContext)
        {
            _studentRepository = studentRepository;
            _mapper = mapper;
            _authContext = authContext;
        }

        public async Task<MethodResult<StudentModel>> Handle(UpdateStudentBeginnerGuideCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<StudentModel>();

            //Kiểm tra xem student có tồn tại không
            var student = await _studentRepository.Queryable
                                    .Include(x => x.User)
                                    .FirstOrDefaultAsync(x => x.UserId == _authContext.CurrentUserId, cancellationToken: cancellationToken);

            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            student.BeginnerGuide = ExcuteBeginnerGuide(student, request);
            await _studentRepository.ExecuteTransactionAsync(async () =>
            {
                student = _studentRepository.Update(student);
                await _studentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<StudentModel>(student);
                return methodResult;
            });

            return methodResult;
        }

        private List<string> FindKeyByValue(string value)
        {
            return datasBeginner
                .Where(x => x.Value == value)
                .Select(x => x.Key)
                .ToList();
        }

        private List<string> FindValueByKey(string key)
        {
            return datasBeginner
                .Where(x => x.Key == key)
                .Select(x => x.Value)
                .ToList();
        }

        private IList<KeyValuePair<string, string>> datasBeginner = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("placement-test-module", "keyTestProgressModule"),
            new KeyValuePair<string, string>("placement-test-module_2", "keyTestProgressModule"),
            new KeyValuePair<string, string>("placement-test-exercise-0", "keyPlacementTest"),
            new KeyValuePair<string, string>("placement-test-exercise-1", "keyTestProgressModule"),
            new KeyValuePair<string, string>("placement-test-exercise-2", "keyTabMediaPort"),
            new KeyValuePair<string, string>("placement-test-exercise-3", "keyTabMediaPort2"),
            new KeyValuePair<string, string>("", "showCaseTabQuestions"),
            new KeyValuePair<string, string>("placement-test-exercise-4", "keyQuestionMark"),
            new KeyValuePair<string, string>("placement-test-exercise-5", "keyQuestionTime"),
            new KeyValuePair<string, string>("placement-test-exercise-6", "keyIcQuestionList"),
            new KeyValuePair<string, string>("placement-test-exercise-7", "keyQuestionListBottomSheetFinalTestMockTest"),
            new KeyValuePair<string, string>("placement-test-exercise-8", "keyQuestionBottomBar"),
            new KeyValuePair<string, string>("placement-test-module-submit", "keyQuestionBottomBar2"),
            new KeyValuePair<string, string>( "placement-test-select-level", ""),
            new KeyValuePair<string, string>( "placement-test-select-level-two", ""),
            new KeyValuePair<string, string>("techie_welcome_planet", "keyPlanet"),
            new KeyValuePair<string, string>("i18n_techie_lesson_overview_two", ""),
            new KeyValuePair<string, string>("i18n_techie_lesson_content_step1", ""),
            new KeyValuePair<string, string>("techie_lesson_content_step2", ""),
            new KeyValuePair<string, string>("techie_lesson_content_step3", ""),
            new KeyValuePair<string, string>("techie_lesson_content_step3", ""),
            new KeyValuePair<string, string>("", "keyVideo"),
            new KeyValuePair<string, string>("techie_lesson_content_video", "keyVideoControls"),
            new KeyValuePair<string, string>("techie_lesson_content_video_step2", ""),
            new KeyValuePair<string, string>("techie_lesson_content_video3", "keyListVideo"),
            new KeyValuePair<string, string>("", "keyNote"),
            new KeyValuePair<string, string>("techie_video_exercises_step1", ""),
            new KeyValuePair<string, string>("techie_video_exercises_step2", ""),
            new KeyValuePair<string, string>("techie_video_exercises_step5", ""),
            new KeyValuePair<string, string>("i18n_techie_video_exercises_step6", ""),
            new KeyValuePair<string, string>("techie_video_exercises_done_step2", ""),
            new KeyValuePair<string, string>("techie_video_exercises_step4", ""),
            new KeyValuePair<string, string>("i18n_techie_lesson_ClassForumLock_step1", "keyReviewLesson"),
            new KeyValuePair<string, string>( "i18n_techie_lesson_page_classForum_step2", "keyClassForum"),
            new KeyValuePair<string, string>( "techie_class_forum_welcome", "keyPostClassForum"),
            new KeyValuePair<string, string>( "", "keyVideoControls"),
            new KeyValuePair<string, string>( "", "keyListVideo"),
            new KeyValuePair<string, string>( "", "keyNote"),
            new KeyValuePair<string, string>( "i18n_techie_lesson_ClassForumLock_step1", "keyReviewLesson"),
            new KeyValuePair<string, string>( "i18n_beginnerGuide_classForum_step2", "keyPromptChatGPT1"),
            new KeyValuePair<string, string>( "i18n_beginnerGuide_classForum_step3", "keyPromptChatGPT2"),
            new KeyValuePair<string, string>( "i18n_beginnerGuide_classForum_step4", "keyClassMate"),
            new KeyValuePair<string, string>( "i18n_beginnerGuide_classForum_step5", "keyHomeWork"),
            new KeyValuePair<string, string>( "i18n_beginnerGuide_classForum_step5", "keyHomeWork"),
            new KeyValuePair<string, string>( "i18n_beginnerGuide_homeWork_step1", "keyItemHomeWork"),
        };
        private StudentBeginnerGuide ExcuteBeginnerGuide (Student student, UpdateStudentBeginnerGuideCommandModel request)
        {
            StudentBeginnerGuide studentBeginnerGuide = (student.BeginnerGuide) != null ? student.BeginnerGuide : new StudentBeginnerGuide();
            var oldOther = student.BeginnerGuide?.Other?.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList() ?? new List<string>();
            var newOther = request.BeginnerGuide?.Other?.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList() ?? new List<string>();

            var addOther = newOther.Except(oldOther).ToList();

            foreach (var item in addOther)
            {
                var keysToAdd = FindKeyByValue(item).Where(key => !newOther.Contains(key));
                newOther.AddRange(keysToAdd);

                var valuesToAdd = FindValueByKey(item).Where(value => !newOther.Contains(value));
                newOther.AddRange(valuesToAdd);
            }
            studentBeginnerGuide.BeginnerGuides = request.BeginnerGuide?.BeginnerGuides;
            studentBeginnerGuide.Other = string.Join(",", newOther.Distinct());
            studentBeginnerGuide.QuestionTypes = request.BeginnerGuide?.QuestionTypes?.Distinct().ToList();
            return studentBeginnerGuide;
        }
    }
}
