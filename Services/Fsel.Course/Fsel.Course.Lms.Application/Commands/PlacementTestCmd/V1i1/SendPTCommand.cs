// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.PlacementTestCmd.V1i1
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Commands.SenderCmd;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.SenderTemplates;
    using MediatR;

    public class SendPTCommand : IRequest<MethodResult<bool>>
    {
        public EnumCourseLevel CourseLevel { get; set; }
        public string? Email { get; set; }
        public int Age { get; set; }
    }

    public class SendPTCommandHandler : IRequestHandler<SendPTCommand, MethodResult<bool>>
    {
        private readonly IMediator _mediator;
        private readonly AppSetting _appSetting;
        private readonly ISystemService _systemService;

        public SendPTCommandHandler(IMediator mediator, AppSetting appSetting, ISystemService systemService)
        {
            _mediator = mediator;
            _appSetting = appSetting;
            _systemService = systemService;
        }

        public async Task<MethodResult<bool>> Handle(SendPTCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            await SendStudentPlacementTest(request.CourseLevel, request.Email, request.Age, cancellationToken);
            return methodResult;
        }

        private async Task SendStudentPlacementTest(EnumCourseLevel courseLevel, string? email, int age, CancellationToken cancellationToken)
        {
            var currentLevel = SendMailHelper.GetPreviousEnumValue(courseLevel);

            var courseSuggestResults = await _systemService.CourseSuggestConfigQuery(new BaseQueryModel()
            {
                Filters = new List<GenericFilterModel>()
                {
                    new GenericFilterModel()
                    {
                        Property = "FromAge",
                        Operator = EnumFilterOperator.LessThanOrEqual,
                        Value = age
                    },
                     new GenericFilterModel()
                    {
                        Property = "ToAge",
                        Operator = EnumFilterOperator.GreaterThanOrEqual,
                        Value = age
                    },
                     new GenericFilterModel()
                    {
                        Property = "PlacementTestLevel",
                        Operator = EnumFilterOperator.Equal,
                        Value = courseLevel
                    },
                     new GenericFilterModel()
                    {
                        Property = "Type",
                        Operator = EnumFilterOperator.Equal,
                        Value = "Balanced"
                    }
                }
            });

            var courseSuggests = courseSuggestResults.Content?.Result;
            var suggestLevels = courseSuggests?.FirstOrDefault()?.CourseLevels;

            string currentCourseHtml = string.Empty;

            if (courseLevel == EnumCourseLevel.A1)
            {
                currentCourseHtml = await SendMailHelper.GetTemplateFromPath(AppDomain.CurrentDomain.BaseDirectory, EnumCourseLevelHelper.GetCourseInfo(null), cancellationToken);
            }
            else
            {
                currentCourseHtml = await SendMailHelper.GetTemplateFromPath(AppDomain.CurrentDomain.BaseDirectory, EnumCourseLevelHelper.GetCourseInfo(currentLevel), cancellationToken);
            }

            var courseInfoHtml = await SendMailHelper.GetTemplateFromPath(AppDomain.CurrentDomain.BaseDirectory, SendMailSetting.CourseInfo, cancellationToken);

            var teachersHtml = await SendMailHelper.GetTemplateFromPath(AppDomain.CurrentDomain.BaseDirectory, SendMailSetting.TeachersInFo, cancellationToken);
            var pathTeachersBios = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SendMailSetting.TeacherBios);
            var listTeachersBios = ConvertHelper.DeserializeFromFilePath<IList<CourseTeacherModel>>(pathTeachersBios);

            string coursesInfo = string.Empty;

            for (int i = 0; i < suggestLevels?.Count; i++)
            {
                var teachers = listTeachersBios?.Where(p => p.TeacherLevels != null && p.TeacherLevels.Any(x => x == suggestLevels[i])).ToList();
                var teacherInfo = string.Empty;

                for (int j = 0; j < teachers?.Count; j++)
                {
                    if (j == 0)
                    {
                        var teacherInfoHtml = string.Format(CultureInfo.InvariantCulture, teachersHtml, null, suggestLevels[i], teachers[j].AvatarPath, teachers[j].FullName, teachers[j].Nationality, teachers[j].Certifications, teachers[j].Experience);
                        teacherInfo += teacherInfoHtml;
                    }
                    else
                    {
                        var teacherInfoHtml = string.Format(CultureInfo.InvariantCulture, teachersHtml, SendMailSetting.Display, suggestLevels[i], teachers[j].AvatarPath, teachers[j].FullName, teachers[j].Nationality, teachers[j].Certifications, teachers[j].Experience);
                        teacherInfo += teacherInfoHtml;
                    }
                }
                var courseType = EnumCourseLevelHelper.GetEnumCourseType(suggestLevels[i]);

                var courseInfo = string.Format(CultureInfo.InvariantCulture, courseInfoHtml, i + 1, suggestLevels[i], suggestLevels[i].GetDescription(), EnumCourseLevelHelper.GetCourseTitle(suggestLevels[i]), EnumCourseLevelHelper.GetLevelPhoto(suggestLevels[i]), SendMailHelper.GetCourseTitle(courseType), SendMailHelper.GetInfoCourse(EnumCourseLevelHelper.GetEnumCourseType(suggestLevels[i])), teacherInfo);

                coursesInfo += courseInfo;
            }

            var param = new SendStudentPTTemplateModel
            {
                FullName = email,
                CurrentCourse = currentCourseHtml,
                CourseInfos = coursesInfo,
                ContinueLearn = _appSetting.ResourceContent?.LmsWebsiteUrl
            };

            var subject = string.Format(CultureInfo.InvariantCulture, SenderSettings.SendPTResultSubject);
            var sendResult = new MethodResult<bool>();
            if (!string.IsNullOrEmpty(email))
            {
                sendResult = await _mediator.Send(new SenderCommand { Email = email, Subject = subject, Params = param, Template = EnumSenderTemplate.StudentCompletePT, IsCCEmail = false }, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
