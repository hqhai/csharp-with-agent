// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.PlacementTestCmd.V1i1
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.SenderCmd;
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

        public SendPTCommandHandler(IMediator mediator)
        {
            _mediator = mediator;
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
            var suggestLevel = GetPreviousEnumValue(courseLevel);

            var currentCourseHtml = await SendMailHelper.GetTemplateFromPath(AppDomain.CurrentDomain.BaseDirectory, EnumCourseLevelHelper.GetCourseInfo(suggestLevel), cancellationToken);

            var courseInfoHtml = await SendMailHelper.GetTemplateFromPath(AppDomain.CurrentDomain.BaseDirectory, SendMailSetting.CourseInfo, cancellationToken);

            var suggestLevels = GetSuggestLevels(courseLevel, age);

            var teachersHtml = await SendMailHelper.GetTemplateFromPath(AppDomain.CurrentDomain.BaseDirectory, SendMailSetting.TeachersInFo, cancellationToken);
            var pathTeachersBios = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SendMailSetting.TeacherBios);
            var listTeachersBios = ConvertHelper.DeserializeFromFilePath<IList<CourseTeacherModel>>(pathTeachersBios);

            string coursesInfo = string.Empty;

            for (int i = 0; i < suggestLevels.Count; i++)
            {
                var teachers = listTeachersBios?.Where(p => p.TeacherLevels != null && p.TeacherLevels.Any(x => x == suggestLevels[i])).ToList();
                var teacherInfo = string.Empty;

                foreach (var item in teachers)
                {
                    var teacherInfoHtml = string.Format(CultureInfo.InvariantCulture, teachersHtml, suggestLevels[i], item.AvatarPath, item.FullName, item.Nationality, item.Certifications, item.Experience, item.Strength);
                    teacherInfo += teacherInfoHtml;
                }

                var courseInfo = string.Format(CultureInfo.InvariantCulture, courseInfoHtml, i + 1, suggestLevels[i], suggestLevels[i].GetDescription(), EnumCourseLevelHelper.GetCourseTitle(suggestLevels[i]), EnumCourseLevelHelper.GetLevelPhoto(suggestLevels[i]), EnumCourseLevelHelper.GetEnumCourseType(suggestLevels[i]), GetInfoCourse(EnumCourseLevelHelper.GetEnumCourseType(suggestLevels[i])), teacherInfo);

                coursesInfo += courseInfo;
            }

            var param = new SendStudentPTTemplateModel
            {
                CurrentCourse = currentCourseHtml,
                CourseInfos = coursesInfo,
            };

            var subject = string.Format(CultureInfo.InvariantCulture, SenderSettings.SendPTResultSubject);
            var sendResult = new MethodResult<bool>();
            if (!string.IsNullOrEmpty(email))
            {
                sendResult = await _mediator.Send(new SenderCommand { Email = email, Subject = subject, Params = param, Template = EnumSenderTemplate.StudentCompletePT }, cancellationToken).ConfigureAwait(false);
            }
        }

        public static string GetInfoCourse(EnumCourseType currentValue)
        {
            if (currentValue == EnumCourseType.Academic)
            {
                return SendMailSetting.AcademicInfo;
            }
            else
            {
                return SendMailSetting.IELTInfo;
            }
        }

        public static EnumCourseLevel GetPreviousEnumValue(EnumCourseLevel currentValue)
        {
            int enumCount = Enum.GetValues(typeof(EnumCourseLevel)).Length;
            int currentValueIndex = (int)currentValue;
            if (currentValueIndex == 0)
            {
                return currentValue;
            }
            int previousValueIndex = (currentValueIndex - 1) % enumCount;
            EnumCourseLevel previousValue = (EnumCourseLevel)previousValueIndex;
            return previousValue;
        }

        public static IList<EnumCourseLevel> GetSuggestLevels(EnumCourseLevel currentValue, int age)
        {
            var suggestLevels = new List<EnumCourseLevel>();
            switch (currentValue)
            {
                case EnumCourseLevel.A1:
                    suggestLevels.Add(EnumCourseLevel.A2);
                    break;

                case EnumCourseLevel.A2:
                    suggestLevels.Add(EnumCourseLevel.B1);
                    break;

                case EnumCourseLevel.B1:
                    if (age >= 14)
                    {
                        suggestLevels.Add(EnumCourseLevel.MS1);
                    }
                    suggestLevels.Add(EnumCourseLevel.B1Plus);
                    break;

                case EnumCourseLevel.B1Plus:
                    if (age >= 14)
                    {
                        suggestLevels.Add(EnumCourseLevel.MS2);
                    }
                    suggestLevels.Add(EnumCourseLevel.B2);
                    break;

                case EnumCourseLevel.B2:
                    if (age >= 14)
                    {
                        suggestLevels.Add(EnumCourseLevel.MS3);
                    }
                    suggestLevels.Add(EnumCourseLevel.C1);
                    break;

                case EnumCourseLevel.C1:
                    if (age >= 14)
                    {
                        suggestLevels.Add(EnumCourseLevel.MS3);
                    }
                    suggestLevels.Add(EnumCourseLevel.C1);
                    break;
            }
            return suggestLevels;
        }
    }
}
