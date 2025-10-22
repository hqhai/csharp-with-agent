// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.StudentGoalAggregateCmd
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.SenderService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SendEmailLearningProgressWarningCommand : IRequest<MethodResult<bool>>
    {
        public IList<Guid>? StudentIds { get; set; }
    }

    public class SendEmailLearningProgressWarningCommandHandler : IRequestHandler<SendEmailLearningProgressWarningCommand, MethodResult<bool>>
    {
        private readonly IUserService _userService;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ISenderService _senderService;

        private const string VideoScore = "Điểm trong video tương tác:";
        private const string ClassForumScore = "Điểm trong Diễn đàn lớp hợp:";
        private const string HomeworkScore = "Điểm bài tập về nhà:";
        private const string SkillTestScore = "Điểm bài kiểm tra Skill Test:";
        private const string UnitTestScore = "Điểm bài kiểm tra Unit Test:";
        private const string SkillMockTestScore = "Điểm bài tập về nhà:";

        private const string Subject = "[FSEL] Hãy quay lại nhịp học nhé – bạn có thể làm tốt hơn nhiều!";

        public SendEmailLearningProgressWarningCommandHandler(IUserService userService, IUnitResultRepository unitResultRepository, IClassForumResultRepository classForumResultRepository, IHomeWorkResultRepository homeWorkResultRepository, IVideoResultRepository videoResultRepository, ILessonResultRepository lessonResultRepository, IMockTestResultRepository mockTestResultRepository, ICourseRepository courseRepository, ISenderService senderService)
        {
            _userService = userService;
            _unitResultRepository = unitResultRepository;
            _classForumResultRepository = classForumResultRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
            _videoResultRepository = videoResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _courseRepository = courseRepository;
            _senderService = senderService;
        }

        public async Task<MethodResult<bool>> Handle(SendEmailLearningProgressWarningCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (request.StudentIds == null || !request.StudentIds.Any())
            {
                return methodResult;
            }

            var studentResults = await _userService.GetStudentsByStudentIdsAsync(request.StudentIds);
            var students = studentResults.Content?.Result?.Where(p => p.CourseId.HasValue).ToList();

            if (students == null || !students.Any())
            {
                return methodResult;
            }

            var studentIds = students.Select(p => p.Id).ToList();
            var courseIds = students.Select(p => p.CourseId ?? default).ToList();

            var courses = await _courseRepository.Queryable.WhereBulkContains(courseIds, p => p.Id).ToListAsync(cancellationToken);

            var lessonResultEntities = await _lessonResultRepository.Queryable.WhereBulkContains(studentIds, p => p.StudentId).ToListAsync(cancellationToken);

            var lessonResultEntityIds = lessonResultEntities.Select(p => p.Id).ToList();

            var videoResultEntities = await _videoResultRepository.Queryable.WhereBulkContains(lessonResultEntityIds, p => p.LessonResultId).Where(p => p.Status == EnumResultStatus.Done).ToListAsync(cancellationToken);

            var classForumResultEntities = await _classForumResultRepository.Queryable.WhereBulkContains(lessonResultEntityIds, p => p.LessonResultId).Where(p => p.Status.HasValue).ToListAsync(cancellationToken);

            var homeworkResultEntities = await _homeWorkResultRepository.Queryable.WhereBulkContains(lessonResultEntityIds, p => p.LessonResultId).Where(p => p.Status == EnumResultStatus.Done).ToListAsync(cancellationToken);

            var mockTestResultEntities = await _mockTestResultRepository.Queryable.WhereBulkContains(studentIds, p => p.StudentId).Where(p => p.UnitId.HasValue && p.Status == EnumResultStatus.Done).ToListAsync(cancellationToken);

            var learningProgressWarningModels = new List<LearningProgressWarningModel>();

            foreach (var student in students)
            {
                var course = courses.FirstOrDefault(p => p.Id == student.Id);
                if (course == null || string.IsNullOrEmpty(student.Human?.Email))
                {
                    continue;
                }

                var lessonResults = lessonResultEntities.Where(p => p.StudentId == student.Id && p.CourseId == student.CourseId).ToList();
                var lessonResultIds = lessonResults.Select(p => p.Id).ToList();

                var videoResults = videoResultEntities.Where(p => p.StudentId == student.Id && lessonResultIds.Contains(p.LessonResultId)).ToList();

                var classForumResults = classForumResultEntities.Where(p => p.StudentId == student.Id && lessonResultIds.Contains(p.LessonResultId)).ToList();

                var homeworkResults = homeworkResultEntities.Where(p => p.StudentId == student.Id && lessonResultIds.Contains(p.LessonResultId)).ToList();

                var videoResultScores = videoResults
                                       .Where(p => p.VideoSkillScores != null && p.VideoSkillScores.Any())
                                       .SelectMany(p => p.VideoSkillScores ?? new List<VideoSkillScores>()).ToList()
                                       .Where(p => p.Type == EnumTimeCodeType.Standalone && p.SkillScores != null && p.SkillScores.Any())
                                       .SelectMany(p => p.SkillScores ?? new List<SkillScores>()).ToList();

                var videoResultSkillScores = videoResultScores.GroupBy(p => p.Skill).Select(p => new SkillPercentModel
                {
                    Skill = p.Key,
                    Percent = (int)p.Average(x => x.Percent)
                }).ToList();

                var classForumResultScores = classForumResultEntities
                                            .Where(p => p.SkillScores != null && p.SkillScores.Any())
                                            .SelectMany(p => p.SkillScores ?? new List<SkillScores>()).ToList();

                var classForumResultSkillScores = classForumResultScores.GroupBy(p => p.Skill).Select(p => new SkillPercentModel
                {
                    Skill = p.Key,
                    Percent = (int)p.Average(x => x.Percent)
                }).ToList();

                var homeworkResultScores = homeworkResults
                                          .Where(p => p.SkillScores != null && p.SkillScores.Any())
                                          .SelectMany(p => p.SkillScores ?? new List<SkillScores>()).ToList();

                var homeworkResultSkillScores = homeworkResultScores.GroupBy(p => p.Skill).Select(p => new SkillPercentModel
                {
                    Skill = p.Key,
                    Percent = (int)p.Average(x => x.Percent)
                }).ToList();

                var sectionTemplate = await SendMailHelper.GetTemplateFromPath(AppDomain.CurrentDomain.BaseDirectory, SendMailSetting.Section, cancellationToken);

                var skillPercentTemplate = await SendMailHelper.GetTemplateFromPath(AppDomain.CurrentDomain.BaseDirectory, SendMailSetting.SkillPercent, cancellationToken);

                var sections = string.Empty;

                if (videoResultSkillScores.Any())
                {
                    GetSection(sections, videoResultSkillScores, skillPercentTemplate, sectionTemplate, VideoScore);
                }

                if (classForumResultSkillScores.Any())
                {
                    GetSection(sections, classForumResultSkillScores, skillPercentTemplate, sectionTemplate, ClassForumScore);
                }

                if (homeworkResultSkillScores.Any())
                {
                    GetSection(sections, homeworkResultSkillScores, skillPercentTemplate, sectionTemplate, HomeworkScore);
                }

                if (course.CourseType == EnumCourseType.Academic || course.CourseType == EnumCourseType.EnglishFoundation)
                {
                    var unitTestScores = videoResults
                                       .Where(p => p.VideoSkillScores != null && p.VideoSkillScores.Any())
                                       .SelectMany(p => p.VideoSkillScores ?? new List<VideoSkillScores>()).ToList()
                                       .Where(p => p.Type == EnumTimeCodeType.UnitTest && p.SkillScores != null && p.SkillScores.Any())
                                       .SelectMany(p => p.SkillScores ?? new List<SkillScores>()).ToList();

                    var unitTestSkillScores = unitTestScores.GroupBy(p => p.Skill).Select(p => new SkillPercentModel
                    {
                        Skill = p.Key,
                        Percent = (int)p.Average(x => x.Percent)
                    }).ToList();

                    if (unitTestSkillScores.Any())
                    {
                        GetSection(sections, unitTestSkillScores, skillPercentTemplate, sectionTemplate, UnitTestScore);
                    }

                    var skillTestScores = videoResults
                                       .Where(p => p.VideoSkillScores != null && p.VideoSkillScores.Any())
                                       .SelectMany(p => p.VideoSkillScores ?? new List<VideoSkillScores>()).ToList()
                                       .Where(p => p.Type == EnumTimeCodeType.SkillTest && p.SkillScores != null && p.SkillScores.Any())
                                       .SelectMany(p => p.SkillScores ?? new List<SkillScores>()).ToList();

                    var skillTestSkillScores = skillTestScores.GroupBy(p => p.Skill).Select(p => new SkillPercentModel
                    {
                        Skill = p.Key,
                        Percent = (int)p.Average(x => x.Percent)
                    }).ToList();

                    if (skillTestSkillScores.Any())
                    {
                        GetSection(sections, skillTestSkillScores, skillPercentTemplate, sectionTemplate, SkillTestScore);
                    }
                }
                else
                {
                    var skillMockTestResults = mockTestResultEntities
                                         .Where(p => p.StudentId == student.Id)
                                         .Where(p => p.SkillScores != null && p.SkillScores.Any())
                                         .SelectMany(p => p.SkillScores ?? new List<SkillScores>()).ToList();

                    var skillMockTestResultSkillScores = skillMockTestResults.GroupBy(p => p.Skill).Select(p => new SkillPercentModel
                    {
                        Skill = p.Key,
                        Percent = (int)p.Average(x => x.Percent)
                    }).ToList();

                    if (skillMockTestResultSkillScores.Any())
                    {
                        GetSection(sections, skillMockTestResultSkillScores, skillPercentTemplate, sectionTemplate, SkillMockTestScore);
                    }
                }

                learningProgressWarningModels.Add(new LearningProgressWarningModel()
                {
                    Email = student.Human.Email,
                    FullName = student.Human.FullName,
                    SkillScore = sections,
                    TotalPercent = 70
                });

                if (learningProgressWarningModels.Any())
                {
                    var tasks = learningProgressWarningModels
                             .Select(p => _senderService.SendEmailAsync(new SendEmailByTemplateCommandModel()
                             {
                                 ToEmails = new List<string> { p.Email ?? string.Empty },
                                 Subject = Subject,
                                 Template = EnumSenderTemplate.LearningProgressWarning,
                                 Params = p
                             }))
                             .ToList();

                    await Task.WhenAll(tasks);
                }
            }

            return methodResult;
        }

        private static string GetSection(string section, IList<SkillPercentModel> skills, string skillPercentTemplate, string sectionTemplate, string sectionName)
        {
            var skillScoreHtml = string.Empty;

            skills.ForEach(p =>
            {
                var (color, skillName, icon) = SendMailHelper.ConvertEnum(p.Skill);
                var html = string.Format(CultureInfo.InvariantCulture, skillPercentTemplate, icon, skillName, p.Percent, p.Percent);
                skillScoreHtml += html;
            });

            var html = string.Format(CultureInfo.InvariantCulture, sectionTemplate, sectionName, skillScoreHtml);

            return section += html;
        }
    }
}
