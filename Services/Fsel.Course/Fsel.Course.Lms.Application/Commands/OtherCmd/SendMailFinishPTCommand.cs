// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.OtherCmd
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Commands.SenderCmd;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.CommandModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.SenderTemplates;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SendMailFinishPTModel
    {
        public Guid TestGroupResultId { get; set; }
    }

    public class SendMailFinishPTCommand : SendMailFinishPTModel, IRequest<MethodResult<bool>>
    {
    }

    public class SendMailFinishPTCommandHandler : IRequestHandler<SendMailFinishPTCommand, MethodResult<bool>>
    {
        private readonly IUserService _userService;
        private readonly ITestGroupResultRepository _testGroupResultRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly ILevelRepository _levelRepository;
        private readonly AppSetting _appSetting;
        private readonly IMediator _mediator;

        private const string NoneProgress = "none-progress";
        private const string ColorDefault = "#566CD6";
        private const string BorderRadius = " border-radius: 10px;";
        private const string Display = "display: none";
        private const string SubjectEnglish = "English";

        public SendMailFinishPTCommandHandler(IUserService userService, ITestGroupResultRepository testGroupResultRepository, ISkillRepository skillRepository, ILevelRepository levelRepository, AppSetting appSetting, IMediator mediator)
        {
            _userService = userService;
            _testGroupResultRepository = testGroupResultRepository;
            _skillRepository = skillRepository;
            _levelRepository = levelRepository;
            _appSetting = appSetting;
            _mediator = mediator;
        }

        public async Task<MethodResult<bool>> Handle(SendMailFinishPTCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var testGroupResult = await _testGroupResultRepository.ReadQueryable.Include(p => p.TestResults).Include(p => p.CurrentLevel).Include(p => p.EmailLevel).ThenInclude(p => p.Category).ThenInclude(p => p.CategoryParent).FirstOrDefaultAsync(p => p.Id == request.TestGroupResultId, cancellationToken);
            if (testGroupResult == null || !testGroupResult.StudentId.HasValue || !testGroupResult.CurrentLevelId.HasValue || !testGroupResult.EmailLevelId.HasValue)
            {
                return methodResult;
            }

            var studentResult = await _userService.GetUserByStudentId(testGroupResult.StudentId.Value);
            var student = studentResult.Content?.Result;
            if (student == null || student.User == null)
            {
                return methodResult;
            }

            var suggestLevel = testGroupResult.CurrentLevel;
            var currentLevel = testGroupResult.EmailLevel;
            var subject = currentLevel?.Category?.CategoryParent;

            var testResult = testGroupResult.TestResults.OrderByDescending(p => p.CreatedDate).FirstOrDefault();
            if (testResult == null || testResult.SkillScores == null || testResult.SkillScores.Count == 0)
            {
                return methodResult;
            }

            var skillHtml = await SendMailHelper.GetTemplateFromPath(AppDomain.CurrentDomain.BaseDirectory, SendMailSetting.CourseSkill, cancellationToken);
            string skillScore = string.Empty;

            var skillIds = testResult.SkillScores.Select(p => p.SkillId).ToList();

            var skills = await _skillRepository.ReadQueryable.WhereBulkContains(skillIds, p => p.Id).ToListAsync(cancellationToken);

            foreach (var item in testResult.SkillScores)
            {
                var skill = skills.FirstOrDefault(p => p.Id == item.SkillId);
                if (skill != null)
                {
                    var message = SkillMessagePool.Get(item.Percent);

                    var html = string.Format(CultureInfo.InvariantCulture, skillHtml, skill.FilePath, skill.Name, GetColorPercent(item.Percent), item.Percent, item.Percent == 0 ? NoneProgress : null, item.Percent, !string.IsNullOrEmpty(skill.ColorCode) ? skill.ColorCode : ColorDefault, item.Percent == 100 ? BorderRadius : null, 100 - item.Percent, item.Percent == 0 ? BorderRadius : null, message);

                    skillScore += html;
                }
            }

            var token = await _userService.SenderSettingGenerateToken(new UpdateSenderSettingCommandModel
            {
                UserId = student.UserId,
                Template = EnumSenderTemplate.SendStudentCompleteCourseAcademic
            });

            string accessLink = string.Format(CultureInfo.InvariantCulture, _appSetting.ConstantUrl!.UpdateSenderSettingUrl!, token?.Content?.Result ?? string.Empty);

            var display = subject?.Name == SubjectEnglish ? null : Display;

            var sendStudentPT = new SendStudentPTTemplateModel()
            {
                FullName = student.User.FullName,
                AccessLink = accessLink,
                CurrentLevel = currentLevel?.Name,
                DescriptionCurrentLevel = currentLevel?.PTDescription ?? string.Empty,
                SuggestLevel = suggestLevel?.Name,
                DescriptionSuggestLevel = suggestLevel?.PTDescription ?? string.Empty,
                Program = currentLevel?.Category?.Name,
                ProgramDescription = currentLevel?.Category?.Description,
                SkillScores = skillScore,
                Display = display,
                ContinueLearn = _appSetting.ResourceContent?.LmsWebsiteUrl
            };

            if (!string.IsNullOrEmpty(student.User.Email))
            {
                var sendResult = await _mediator.Send(new SenderCommand
                {
                    Email = student.User.Email,
                    Subject = SenderSettings.SendPTResultSubject,
                    Params = sendStudentPT,
                    CcEmail = student.ParentEmail,
                    Template = EnumSenderTemplate.StudentCompletePT
                }, cancellationToken).ConfigureAwait(false);
            }

            return methodResult;
        }

        private static string GetColorPercent(double percent)
        {
            if (percent < 50)
            {
                return "#F04438";
            }
            else if (percent < 80)
            {
                return "#FF9A47";
            }
            else
            {
                return "#5CC159";
            }
        }

        private static class SkillMessagePool
        {
            private static readonly Random _random = new();

            private static List<string> _under50Pool = new(Under50Messages);
            private static List<string> _under80Pool = new(Under80Messages);
            private static List<string> _under100Pool = new(Under100Messages);

            public static string Get(double percent)
            {
                if (percent < 50)
                {
                    return GetFromPool(ref _under50Pool, Under50Messages);
                }
                if (percent < 80)
                {
                    return GetFromPool(ref _under80Pool, Under80Messages);
                }
                return GetFromPool(ref _under100Pool, Under100Messages);
            }

            private static string GetFromPool(ref List<string> pool, string[] source)
            {
                if (pool.Count == 0)
                {
                    pool = new List<string>(source);
                }

                var index = _random.Next(pool.Count);
                var value = pool[index];

                pool.RemoveAt(index);
                return value;
            }
        }

        private static readonly string[] Under50Messages =
        {
            SendMailSetting.SkillUnder50i1, SendMailSetting.SkillUnder50i2, SendMailSetting.SkillUnder50i3, SendMailSetting.SkillUnder50i4, SendMailSetting.SkillUnder50i5,
            SendMailSetting.SkillUnder50i6, SendMailSetting.SkillUnder50i7, SendMailSetting.SkillUnder50i8, SendMailSetting.SkillUnder50i9, SendMailSetting.SkillUnder50i10
        };

        private static readonly string[] Under80Messages =
        {
            SendMailSetting.SkillUnder80i1, SendMailSetting.SkillUnder80i2, SendMailSetting.SkillUnder80i3, SendMailSetting.SkillUnder80i4, SendMailSetting.SkillUnder80i5,
            SendMailSetting.SkillUnder80i6, SendMailSetting.SkillUnder80i7, SendMailSetting.SkillUnder80i8, SendMailSetting.SkillUnder80i9, SendMailSetting.SkillUnder80i10
        };

        private static readonly string[] Under100Messages =
        {
            SendMailSetting.SkillUnder100i1, SendMailSetting.SkillUnder100i2, SendMailSetting.SkillUnder100i3, SendMailSetting.SkillUnder100i4, SendMailSetting.SkillUnder100i5,
            SendMailSetting.SkillUnder100i6, SendMailSetting.SkillUnder100i7, SendMailSetting.SkillUnder100i8, SendMailSetting.SkillUnder100i9, SendMailSetting.SkillUnder100i10
        };
    }
}
