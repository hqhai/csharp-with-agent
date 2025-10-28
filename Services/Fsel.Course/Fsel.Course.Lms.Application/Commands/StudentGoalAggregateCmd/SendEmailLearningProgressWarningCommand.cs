// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.StudentGoalAggregateCmd
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.SenderService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.SenderTemplates;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.Shared.Models.ShareModels.QueryModels;
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
        private readonly IMediator _mediator;

        private const string VideoScore = "Điểm trong video tương tác:";
        private const string ClassForumScore = "Điểm trong Diễn đàn lớp hợp:";
        private const string HomeworkScore = "Điểm bài tập về nhà:";
        private const string SkillTestScore = "Điểm bài kiểm tra Skill Test:";
        private const string UnitTestScore = "Điểm bài kiểm tra Unit Test:";
        private const string SkillMockTestScore = "Điểm bài tập về nhà:";

        private const string Subject = "[FSEL] Hãy quay lại nhịp học nhé – bạn có thể làm tốt hơn nhiều!";
        private const string Success = "https://s3-sgn10.fptcloud.com/fsel/Files/priority_24dp_75FB4C_FILL1_wght400_GRAD0_opsz24_3424_1761098926527.png";
        private const string Warning = "https://s3-sgn10.fptcloud.com/fsel/Files/warning_26dp_F7B27A_FILL1_wght400_GRAD0_opsz24_2950_1761039817809.png";

        public SendEmailLearningProgressWarningCommandHandler(IUserService userService, IUnitResultRepository unitResultRepository, IClassForumResultRepository classForumResultRepository, IHomeWorkResultRepository homeWorkResultRepository, IVideoResultRepository videoResultRepository, ILessonResultRepository lessonResultRepository, IMockTestResultRepository mockTestResultRepository, ICourseRepository courseRepository, ISenderService senderService, IMediator mediator)
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
            _mediator = mediator;
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

            var userIds = students.Where(p => p.Human != null && p.Human.UserId.HasValue).Select(p => p.Human?.UserId ?? default).ToList();

            var historiesSendMailResult = await _senderService.GetHistoriesSendMailLearningProgress(new GetHistoriesSendMailLearningProgressModel()
            {
                UserIds = userIds
            });

            var historiesSendMail = historiesSendMailResult.Content?.Result;

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            var deleteStudents = new List<StudentModel>();

            students.ForEach(p =>
            {
                var historySendMail = historiesSendMail?.Where(x => x.ReceiverId.HasValue && p.Human != null && p.Human.UserId.HasValue && p.Human.UserId == x.ReceiverId).OrderByDescending(p => p.CreatedDate).FirstOrDefault();
                if (historySendMail != null && historySendMail.CreatedDate.HasValue)
                {
                    TimeSpan timeDifference = currentDate - historySendMail.CreatedDate.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

                    int hours = (int)timeDifference.TotalHours;

                    if (hours < 24)
                    {
                        deleteStudents.Add(p);
                    }
                }
            });

            deleteStudents.ForEach(p => students.Remove(p));

            var studentIds = students.Select(p => p.Id).ToList();
            var courseIds = students.Select(p => p.CourseId ?? default).ToList();

            var courses = await _courseRepository.Queryable.WhereBulkContains(courseIds, p => p.Id).ToListAsync(cancellationToken);

            var lessonResultEntities = await _lessonResultRepository.Queryable.WhereBulkContains(studentIds, p => p.StudentId).ToListAsync(cancellationToken);

            var lessonResultEntityIds = lessonResultEntities.Select(p => p.Id).ToList();

            var videoResultEntities = await _videoResultRepository.Queryable.WhereBulkContains(lessonResultEntityIds, p => p.LessonResultId).Where(p => p.Status == EnumResultStatus.Done).ToListAsync(cancellationToken);

            var classForumResultEntities = await _classForumResultRepository.Queryable.WhereBulkContains(lessonResultEntityIds, p => p.LessonResultId).Where(p => p.Status.HasValue).ToListAsync(cancellationToken);

            var homeworkResultEntities = await _homeWorkResultRepository.Queryable.WhereBulkContains(lessonResultEntityIds, p => p.LessonResultId).Where(p => p.Status == EnumResultStatus.Done).ToListAsync(cancellationToken);

            var mockTestResultEntities = await _mockTestResultRepository.Queryable.WhereBulkContains(studentIds, p => p.StudentId).Where(p => p.UnitId.HasValue && p.Status == EnumResultStatus.Done).ToListAsync(cancellationToken);

            var unitResultEntities = await _unitResultRepository.Queryable
                                                            .Where(x => studentIds.Contains(x.StudentId) && x.Status == EnumResultStatus.Done)
                                                            .ToListAsync(cancellationToken);

            var learningProgressWarningModels = new List<LearningProgressWarningModel>();

            foreach (var student in students)
            {
                var course = courses.FirstOrDefault(p => p.Id == student.CourseId);
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
                    sections = GetSection(sections, videoResultSkillScores, skillPercentTemplate, sectionTemplate, VideoScore);
                }

                if (classForumResultSkillScores.Any())
                {
                    sections = GetSection(sections, classForumResultSkillScores, skillPercentTemplate, sectionTemplate, ClassForumScore);
                }

                if (homeworkResultSkillScores.Any())
                {
                    sections = GetSection(sections, homeworkResultSkillScores, skillPercentTemplate, sectionTemplate, HomeworkScore);
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
                        sections = GetSection(sections, unitTestSkillScores, skillPercentTemplate, sectionTemplate, UnitTestScore);
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
                        sections = GetSection(sections, skillTestSkillScores, skillPercentTemplate, sectionTemplate, SkillTestScore);
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
                        sections = GetSection(sections, skillMockTestResultSkillScores, skillPercentTemplate, sectionTemplate, SkillMockTestScore);
                    }
                }

                var unitResults = unitResultEntities.Where(p => p.StudentId == student.Id && p.CourseId == course.Id);

                double totalPercent = 0;

                if (unitResults != null && unitResults.Any())
                {
                    totalPercent = NumberHelper.ConvertRound(unitResults.Any() ? unitResults.Average(x => x.Percent) : default);
                }

                learningProgressWarningModels.Add(new LearningProgressWarningModel()
                {
                    Email = student.Human.Email,
                    FullName = student.Human.FullName,
                    SkillScore = sections,
                    TotalPercent = (int)totalPercent,
                    UserId = student.Human.UserId,
                });

                if (learningProgressWarningModels.Any())
                {
                    var tasks = learningProgressWarningModels
                             .Select(p => _senderService.SendEmailAsync(new SendEmailByTemplateCommandModel()
                             {
                                 ToEmails = new List<string> { p.Email ?? string.Empty },
                                 Subject = Subject,
                                 Template = EnumSenderTemplate.LearningProgressWarning,
                                 Params = p,
                                 Receivers = new List<SendReceiverCommandModel>()
                                 {
                                     new SendReceiverCommandModel()
                                     {
                                         Email = p.Email,
                                         ReceiverId = p.UserId
                                     }
                                 }
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

                var image = p.Percent >= 70 ? Success : Warning;

                var html = string.Format(CultureInfo.InvariantCulture, skillPercentTemplate, icon, skillName, p.Percent, image, p.Percent, color);
                skillScoreHtml += html;
            });

            var html = string.Format(CultureInfo.InvariantCulture, sectionTemplate, sectionName, skillScoreHtml);

            return section += html;
        }
    }
}
