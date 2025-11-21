// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.CourseResultCmd.V1i2
{
    using System.Linq.Dynamic.Core;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SaveCourseResultCommand : IRequest<MethodResult<CourseResultModel>>
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
    }

    public class SaveCourseResultCommandHandler : IRequestHandler<SaveCourseResultCommand, MethodResult<CourseResultModel>>
    {
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly SaveUserCourseSettingPublisher _saveUserCourseSettingPublisher;
        private readonly ICourseModuleRepository _courseModuleRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly ITestRepository _testRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IRepository<TestGroupResult> _testGroupResultRepository;
        private readonly IRepository<TestResult> _testResultRepository;
        private readonly IMapper _mapper;

        public SaveCourseResultCommandHandler(ICourseResultRepository courseResultRepository
            , ICourseRepository courseRepository
            , IUserService userService
            , AuthContext authContext
            , SaveUserCourseSettingPublisher saveUserCourseSettingPublisher
            , ICourseModuleRepository courseModuleRepository
            , IUnitRepository unitRepository
            , ITestRepository testRepository
            , IUnitResultRepository unitResultRepository
            , IRepository<TestGroupResult> testGroupResultRepository
            , IRepository<TestResult> testResultRepository
            , IMapper mapper)
        {
            _courseResultRepository = courseResultRepository;
            _courseRepository = courseRepository;
            _userService = userService;
            _authContext = authContext;
            _saveUserCourseSettingPublisher = saveUserCourseSettingPublisher;
            _courseModuleRepository = courseModuleRepository;
            _unitRepository = unitRepository;
            _testRepository = testRepository;
            _unitResultRepository = unitResultRepository;
            _testGroupResultRepository = testGroupResultRepository;
            _testResultRepository = testResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<CourseResultModel>> Handle(SaveCourseResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CourseResultModel>();
            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(Course), request.CourseId);
                return methodResult;
            }

            var courseResult = await _courseResultRepository.ReadQueryable.Where(x => x.CourseId == request.CourseId)
                                                            .Where(x => x.StudentId == request.StudentId && x.WorkingStatus == EnumWorkingStatus.Active)
                                                            .FirstOrDefaultAsync(cancellationToken);

            if (courseResult == null)
            {
                courseResult = new CourseResult
                {
                    CourseId = request.CourseId,
                    StudentId = request.StudentId,
                    Status = EnumResultStatus.New,
                    WorkingStatus = EnumWorkingStatus.Active
                };
                try
                {
                    await _courseResultRepository.BulkMergeAsync(new List<CourseResult> { courseResult }, bulk =>
                    {
                        bulk.ColumnPrimaryKeyExpression = c => new { c.CourseId, c.StudentId, c.IsDeleted };
                    });
                }
                catch
                {
                }
                await SaveCourseModuleAsync(courseResult);
                await SaveCourseSettingAsync(course, cancellationToken);
            }

            methodResult.Result = _mapper.Map<CourseResultModel>(courseResult);
            return methodResult;
        }

        private async Task SaveCourseModuleAsync(CourseResult courseResult)
        {
            var courseModule = await _courseModuleRepository.ReadQueryable.Where(x => x.CourseId == courseResult.CourseId)
                                                                      .OrderBy(x => x.OpenOrder)
                                                                      .FirstOrDefaultAsync();
            if (courseModule == null)
            {
                return;
            }

            if (courseModule.CourseConfigType == EnumCourseConfigType.Unit)
            {
                var unit = await _unitRepository.ReadQueryable.Where(x => x.OriginalId == courseModule.OriginalId)
                                                .FirstOrDefaultAsync(x => x.VersionStatus == EnumVersionStatus.LastVersion);
                var unitResult = await _unitResultRepository.ReadQueryable.Where(x => x.CourseModuleId == courseModule.Id)
                                                            .Where(x => x.CourseResultId == courseResult.Id)
                                                            .FirstOrDefaultAsync();
                if (unitResult == null && unit != null)
                {
                    unitResult = new UnitResult
                    {
                        CourseModuleId = courseModule.Id,
                        CourseResultId = courseResult.Id,
                        CourseId = courseResult.CourseId,
                        UnitId = unit.Id,
                        StudentId = courseResult.StudentId,
                        Status = EnumResultStatus.New,
                    };
                    try
                    {
                        await _unitResultRepository.BulkMergeAsync(new List<UnitResult> { unitResult }, bulk =>
                        {
                            bulk.ColumnPrimaryKeyExpression = c => new { c.CourseModuleId, c.CourseResultId, c.IsDeleted };
                        });
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                var test = await _testRepository.Queryable.Where(x => x.OriginalId == courseModule.OriginalId)
                                                .FirstOrDefaultAsync(x => x.VersionStatus == EnumVersionStatus.LastVersion);

                var testGroupResult = await _testGroupResultRepository.Queryable.Where(x => x.CourseModuleId == courseModule.Id)
                                                                  .Where(x => x.CourseResultId == courseResult.Id)
                                                                  .FirstOrDefaultAsync();
                if (testGroupResult == null && test != null)
                {
                    testGroupResult = new TestGroupResult
                    {
                        CourseModuleId = courseModule.Id,
                        CourseResultId = courseResult.Id,
                        CourseId = courseResult.CourseId,
                        StudentId = courseResult.StudentId,
                        Status = EnumResultStatus.New,
                    };
                    try
                    {
                        await _testGroupResultRepository.BulkMergeAsync(new List<TestGroupResult> { testGroupResult }, bulk =>
                        {
                            bulk.ColumnPrimaryKeyExpression = c => new { c.CourseModuleId, c.CourseResultId, c.IsDeleted };
                        });
                    }
                    catch
                    {
                    }

                    var testResult = new TestResult
                    {
                        TestGroupResultId = testGroupResult.Id,
                        TestId = test.Id,
                        StudentId = courseResult.StudentId,
                        Status = EnumResultStatus.New,
                    };
                    try
                    {
                        await _testResultRepository.BulkMergeAsync(new List<TestResult> { testResult }, bulk =>
                        {
                            bulk.ColumnPrimaryKeyExpression = c => new { c.TestGroupResultId, c.TestId, c.StudentId, c.IsDeleted };
                        });
                    }
                    catch { }
                }
            }
        }

        private async Task SaveCourseSettingAsync(Course course, CancellationToken cancellationToken)
        {
            var userCourseSettingResults = await _userService.GetUserCourseSettingsAsync(_authContext.CurrentUserId);
            if (!userCourseSettingResults.IsSuccessStatusCode)
            {
                return;
            }
            var userCourseSetting = userCourseSettingResults.Content?.Result?.FirstOrDefault(x => x.CourseLevel == course.CourseLevel);
            if (userCourseSetting != null)
            {
                return;
            }
            await _saveUserCourseSettingPublisher.Publish(new SaveUserCourseSettingQueueModel
            {
                CourseLevel = course.CourseLevel,
                Type = EnumUserCourseType.ResetAndLearnAgain,
                UserId = _authContext.CurrentUserId
            }, cancellationToken).ConfigureAwait(false);
        }
    }
}
