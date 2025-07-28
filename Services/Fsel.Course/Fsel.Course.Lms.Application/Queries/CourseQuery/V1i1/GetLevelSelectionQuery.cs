// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery.V1i1
{
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using static Fsel.Shared.Constants.ValueSettings;

    public class GetLevelSelectionQuery : IRequest<MethodResult<IList<LevelDtoModel>>>
    {
        public EnumCourseType CourseType { get; set; }

        public Guid? UserId { get; set; }
    }

    public class GetLevelSelectionQueryHandler : IRequestHandler<GetLevelSelectionQuery, MethodResult<IList<LevelDtoModel>>>
    {
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ChangeCourseHelper _changeCourseHelper;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private const int MaxAgeIELST = 14;
        private const int MaxAgeEnglishFoundation = 16;

        public GetLevelSelectionQueryHandler(ICourseResultRepository courseResultRepository, ChangeCourseHelper changeCourseHelper, AuthContext authContext, IUserService userService)
        {
            _courseResultRepository = courseResultRepository;
            _changeCourseHelper = changeCourseHelper;
            _authContext = authContext;
            _userService = userService;
        }

        public async Task<MethodResult<IList<LevelDtoModel>>> Handle(GetLevelSelectionQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<LevelDtoModel>>();
            var userCourseSettingResults = await _userService.GetUserCourseSettingsAsync(_authContext.CurrentUserId);
            if (!userCourseSettingResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(userCourseSettingResults));
                return methodResult;
            }
            var userCourseSettings = userCourseSettingResults?.Content?.Result;

            var studentResult = await _userService.GetStudentByUserIdAsync(request.UserId ?? _authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            if (!student.BaseCourseLevel.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student.BaseCourseLevel));
                return methodResult;
            }

            if (!student.ExpiredDate.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student.ExpiredDate));
                return methodResult;
            }

            int age = Shared.Helpers.DateTimeHelper.GetYearOld(student.Human?.Birthday);
            if ((request.CourseType == EnumCourseType.Ielts && age < AgeMilestone.StudentAge) || (request.CourseType == EnumCourseType.EnglishFoundation && age < AgeMilestone.TeenagersAge))
            {
                methodResult.Result = new List<LevelDtoModel>();
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var isChangeLevelStudent = await _changeCourseHelper.CheckChangeLevelAllCourseAsync(student.Id);
            if (isChangeLevelStudent)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(isChangeLevelStudent));
                return methodResult;
            }
            var courseResults = await _courseResultRepository.Queryable.Include(x => x.Course).Where(x => x.StudentId == student.Id && x.WorkingStatus != EnumWorkingStatus.NotWorking).ToListAsync(cancellationToken);

            var isStudentsAchieveScore = await _changeCourseHelper.IsStudentsAchieveScoresAsync(student.Id, student.BaseCourseLevel);
            var levelDtos = ConvertHelper.Deserialize<List<LevelDtoModel>>(request.CourseType.GetListCourseLevels(student.BaseCourseLevel.Value, isStudentsAchieveScore));

            if (levelDtos != null && levelDtos.Any())
            {
                var skillLevels = ConvertHelper.EnumToList<EnumSkillLevel>();

                foreach (var item in levelDtos)
                {
                    if (courseResults.Any(x => x.Status == EnumResultStatus.Done && x.WorkingStatus == EnumWorkingStatus.Active))
                    {
                        item.SkillLevel = EnumCourseLevelHelper.GetSkillLevel(student.BaseCourseLevel ?? default, item.CourseLevel, isStudentsAchieveScore);
                    }

                    var courseResultLevel = courseResults.FirstOrDefault(x => x.Course != null && x.Course.CourseLevel == item.CourseLevel);
                    var userCourseSetting = userCourseSettings?.FirstOrDefault(x => x.CourseLevel == item.CourseLevel && x.Type == EnumUserCourseType.ResetAndLearnAgain);
                    if (userCourseSetting != null)
                    {
                        item.IsResetCourse = !isChangeLevelStudent && userCourseSetting.HasRemainingAttempts();
                    }
                    else
                    {
                        item.IsResetCourse = courseResultLevel != null ? !isChangeLevelStudent : null;
                    }
                    item.IsUsedLevel = courseResultLevel?.WorkingStatus == EnumWorkingStatus.Active;
                    item.IsHiddenCourseLevel = isChangeLevelStudent;
                    item.Status = courseResultLevel?.Status;
                }
            }

            methodResult.Result = levelDtos ?? new();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
