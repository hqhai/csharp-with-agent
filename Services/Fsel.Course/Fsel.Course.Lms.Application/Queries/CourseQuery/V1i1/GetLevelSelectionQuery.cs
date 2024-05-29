// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery.V1i1
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetLevelSelectionQuery : IRequest<MethodResult<IList<LevelDtoModel>>>
    {
        public EnumCourseType CourseType { get; set; }
    }

    public class GetLevelSelectionQueryHandler : IRequestHandler<GetLevelSelectionQuery, MethodResult<IList<LevelDtoModel>>>
    {
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IUnitResultRepository _unitResultRepository;
        private const int MaxLearnAgain = 3;
        private const int MaxUnitDoneToStop = 3;

        public GetLevelSelectionQueryHandler(ICourseResultRepository courseResultRepository, AuthContext authContext, IUserService userService, IUnitResultRepository unitResultRepository)
        {
            _courseResultRepository = courseResultRepository;
            _authContext = authContext;
            _userService = userService;
            _unitResultRepository = unitResultRepository;
        }

        public async Task<MethodResult<IList<LevelDtoModel>>> Handle(GetLevelSelectionQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<LevelDtoModel>>();
            var userCourseSettingResults = await _userService.GetUserCourseSettingsAsync();
            if (!userCourseSettingResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(userCourseSettingResults));
                return methodResult;
            }
            var userCourseSettings = userCourseSettingResults?.Content?.Result;

            var userCourseSettingLevel = userCourseSettings?.FirstOrDefault(x => x.Type == EnumUserCourseType.ChangeLevel);
            if (userCourseSettingLevel != null && userCourseSettingLevel.Value <= 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(userCourseSettingLevel));
                return methodResult;
            }
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }

            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var courseResults = await _courseResultRepository.Queryable.Include(x => x.Course).Where(x => x.StudentId == student.Id && x.WorkingStatus != EnumWorkingStatus.NotWorking).ToListAsync(cancellationToken);
            var isDoneCourse = await _courseResultRepository.Queryable.AnyAsync(x => x.Status == EnumResultStatus.Done && x.StudentId == student.Id, cancellationToken);
            var levelDtos = ConvertHelper.Deserialize<List<LevelDtoModel>>(request.CourseType.GetListCourseLevels(student.CourseLevel ?? default, isDoneCourse));
            if (levelDtos != null && levelDtos.Any())
            {
                foreach (var item in levelDtos)
                {
                    var courseResultLevel = courseResults.FirstOrDefault(x => x.Course != null && x.Course.CourseLevel == item.CourseLevel);
                    var userCourseSetting = userCourseSettings?.FirstOrDefault(x => x.CourseLevel == item.CourseLevel && x.Type == EnumUserCourseType.ResetAndLearnAgain);
                    item.IsResetCourse = true;
                    if (userCourseSetting != null)
                    {
                        item.IsResetCourse = userCourseSetting.Value > MaxLearnAgain;
                    }
                    item.IsUsedLevel = courseResultLevel?.WorkingStatus == EnumWorkingStatus.Active;
                    item.IsHighCourseLevel = item.IsUsedLevel || await CheckChangeLevelAllCourseAsync(student);
                    item.Status = courseResultLevel?.Status;
                }
            }

            methodResult.Result = levelDtos;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<bool> CheckChangeLevelAllCourseAsync(StudentModel student)
        {
            var courseResultActive = await _courseResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == student.Id && x.WorkingStatus == EnumWorkingStatus.Active);
            if (courseResultActive != null)
            {
                var groupUnitResultStatus = await _unitResultRepository.Queryable.Where(x => x.CourseId == courseResultActive.CourseId && x.StudentId == courseResultActive.StudentId).GroupBy(x => x.Status)
                    .Select(x => new
                    {
                        StatusResult = x.Key,
                        NumberOfStatus = x.Count(),
                    }).ToListAsync();
                return (groupUnitResultStatus.Any(x => x.StatusResult == EnumResultStatus.Done && x.NumberOfStatus >= MaxUnitDoneToStop - 1) && groupUnitResultStatus.Any(x => x.StatusResult == EnumResultStatus.Process)) ||
                    groupUnitResultStatus.Any(x => x.StatusResult == EnumResultStatus.Done && x.NumberOfStatus >= MaxUnitDoneToStop);
            }
            return false;
        }
    }
}
