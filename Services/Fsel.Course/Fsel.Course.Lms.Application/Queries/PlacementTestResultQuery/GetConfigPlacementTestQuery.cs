// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.PlacementTestResultQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using static Fsel.Shared.Constants.ValueSettings;

    public class GetConfigPlacementTestQuery : IRequest<MethodResult<PlacementTestReportOveallModel>>
    {
    }

    public class GetConfigPlacementTestQueryHandler : IRequestHandler<GetConfigPlacementTestQuery, MethodResult<PlacementTestReportOveallModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IPlacementTestGroupResultRepository _placementTestGroupResultRepository;

        public GetConfigPlacementTestQueryHandler(AuthContext authContext
            , IUserService userService
            , IPlacementTestGroupResultRepository placementTestGroupResultRepository)
        {
            _authContext = authContext;
            _userService = userService;
            _placementTestGroupResultRepository = placementTestGroupResultRepository;
        }

        public async Task<MethodResult<PlacementTestReportOveallModel>> Handle(GetConfigPlacementTestQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PlacementTestReportOveallModel> methodResult = new MethodResult<PlacementTestReportOveallModel>();
            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
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
            if (student.User == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student.User));
                return methodResult;
            }
            var placementTestGroupResult = await _placementTestGroupResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == student.Id, cancellationToken);
            if (placementTestGroupResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(placementTestGroupResult));
                return methodResult;
            }

            if (placementTestGroupResult.Status != EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusNotDone), nameof(placementTestGroupResult.Status));
                return methodResult;
            }
            int age = Shared.Helpers.DateTimeHelper.GetYearOld(student.User.Birthday);
            var pathConfigPlacementTest = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.ConfigPlacementTest);
            var configPlacementTests = ConvertHelper.DeserializeFromFilePath<IList<PlacementTestReportConfigModel>>(pathConfigPlacementTest);

            var pathConfigViewReportLevel = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.ConfigViewReportLevel);
            var configPlacementTestViews = ConvertHelper.DeserializeFromFilePath<IList<PlacementTestReportViewConfigModel>>(pathConfigViewReportLevel);

            bool isPreA1 = placementTestGroupResult.CurrentLevel == EnumCourseLevel.A1 && placementTestGroupResult.Percent < MinCompletePercent;
            var currentLevel = isPreA1 ? ValueCourseLevel.PreA1 : EnumCourseLevelHelper.GetCodeByEnumCourseLevel(placementTestGroupResult.CurrentLevel);

            var placementTestReportOveall = new PlacementTestReportOveallModel
            {
                FullName = student.User.FullName,
                SuggetLevel = placementTestGroupResult.SuggetLevel,
                CurrentLevel = currentLevel,
                IsPreA1 = isPreA1,
                PlacementTestViewReport = GetPlacementTestReportViewConfig(configPlacementTestViews, age, placementTestGroupResult.SuggetLevel, student.User.FullName),
            };
            var placementTestAgeLevels = placementTestConfigAgeLevels.Where(x => x.AgeStart <= age && (!x.AgeEnd.HasValue || age < x.AgeEnd)).ToList();
            var placementTestAgeLevel = placementTestAgeLevels.FirstOrDefault();

            if (placementTestAgeLevels.Any(x => x.CurrentLevel == placementTestGroupResult.CurrentLevel))
            {
                placementTestReportOveall.PlacementTestReportConfig = GetPlacementTestReportConfig(configPlacementTests, EnumTestResultScenario.EqualToSuggestedLevel, currentLevel);
            }
            else if (placementTestAgeLevel != null && placementTestGroupResult.CurrentLevel.HasValue)
            {
                if ((int)placementTestAgeLevel.CurrentLevel > (int)placementTestGroupResult.CurrentLevel)
                {
                    placementTestReportOveall.PlacementTestReportConfig = GetPlacementTestReportConfig(configPlacementTests, EnumTestResultScenario.BelowSuggestedLevelAndAge, currentLevel);
                }
                else
                {
                    placementTestReportOveall.PlacementTestReportConfig = GetPlacementTestReportConfig(configPlacementTests, EnumTestResultScenario.AboveSuggestedLevel, currentLevel);
                }
            }
            methodResult.Result = placementTestReportOveall;
            return methodResult;
        }

        private static PlacementTestReportConfigModel? GetPlacementTestReportConfig(IList<PlacementTestReportConfigModel>? configPlacementTests, EnumTestResultScenario enumTestResult, string? currentLevel)
        {
            var configPlacementTest = configPlacementTests?.FirstOrDefault(x => x.ResultScenario == enumTestResult);
            if (configPlacementTest != null)
            {
                if (!string.IsNullOrEmpty(configPlacementTest.Subtitle))
                {
                    configPlacementTest.Subtitle = string.Format(configPlacementTest.Subtitle, currentLevel);
                }
                foreach (var item in configPlacementTest.Translations)
                {
                    if (!string.IsNullOrEmpty(item.Subtitle))
                    {
                        item.Subtitle = string.Format(item.Subtitle, currentLevel);
                    }
                }
            }
            return configPlacementTest;
        }

        private static PlacementTestReportViewConfigModel? GetPlacementTestReportViewConfig(IList<PlacementTestReportViewConfigModel>? configPlacementTests, int age, EnumCourseLevel? suggetLevel, string? fullName)
        {
            var placementTestAgeLevelReport = configPlacementTests?.FirstOrDefault(x => x.AgeStart <= age && (!x.AgeEnd.HasValue || age < x.AgeEnd) && x.SuggestedLevel == suggetLevel);
            if (placementTestAgeLevelReport != null)
            {
                if (!string.IsNullOrEmpty(placementTestAgeLevelReport.Text))
                {
                    placementTestAgeLevelReport.Text = string.Format(placementTestAgeLevelReport.Text, fullName);
                }
                foreach (var item in placementTestAgeLevelReport.Translations)
                {
                    if (!string.IsNullOrEmpty(item.Text))
                    {
                        item.Text = string.Format(item.Text, fullName);
                    }
                }
            }
            return placementTestAgeLevelReport;
        }

        public static IList<PlacementTestConfigAgeLevelModel> placementTestConfigAgeLevels = new List<PlacementTestConfigAgeLevelModel>()
        {
            new PlacementTestConfigAgeLevelModel
            {
                AgeStart = 0,
                AgeEnd = 14,
                CurrentLevel = EnumCourseLevel.A1
            },
            new PlacementTestConfigAgeLevelModel
            {
                AgeStart = 14,
                AgeEnd = 16,
                CurrentLevel = EnumCourseLevel.A2
            },
            new PlacementTestConfigAgeLevelModel
            {
                AgeStart = 16,
                AgeEnd = 17,
                CurrentLevel = EnumCourseLevel.B1
            },
            new PlacementTestConfigAgeLevelModel
            {
                AgeStart = 16,
                AgeEnd = 17,
                CurrentLevel = EnumCourseLevel.B1Plus
            },
            new PlacementTestConfigAgeLevelModel
            {
                AgeStart = 16,
                AgeEnd = 17,
                CurrentLevel = EnumCourseLevel.B2
            },
            new PlacementTestConfigAgeLevelModel
            {
                AgeStart = 17,
                AgeEnd = null,
                CurrentLevel = EnumCourseLevel.C1
            }
        };
    }
}
