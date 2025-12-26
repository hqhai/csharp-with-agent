// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.CourseResultCmd.V1i2
{
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.CourseItemServices;
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
        private readonly IMapper _mapper;
        private readonly ICourseModuleCachingService _courseModuleCachingService;
        private readonly ICourseItemInitializerFactory _courseItemInitializerFactory;

        public SaveCourseResultCommandHandler(ICourseResultRepository courseResultRepository
            , ICourseRepository courseRepository
            , IUserService userService
            , AuthContext authContext
            , SaveUserCourseSettingPublisher saveUserCourseSettingPublisher
            , ICourseModuleRepository courseModuleRepository
            , IMapper mapper
            , ICourseModuleCachingService courseModuleCachingService
            , ICourseItemInitializerFactory courseItemInitializerFactory)
        {
            _courseResultRepository = courseResultRepository;
            _courseRepository = courseRepository;
            _userService = userService;
            _authContext = authContext;
            _saveUserCourseSettingPublisher = saveUserCourseSettingPublisher;
            _courseModuleRepository = courseModuleRepository;
            _mapper = mapper;
            _courseModuleCachingService = courseModuleCachingService;
            _courseItemInitializerFactory = courseItemInitializerFactory;
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
                await SaveCourseModuleAsync(courseResult, cancellationToken);
                await SaveCourseSettingAsync(course, cancellationToken);
            }

            methodResult.Result = _mapper.Map<CourseResultModel>(courseResult);
            return methodResult;
        }

        private async Task SaveCourseModuleAsync(CourseResult courseResult, CancellationToken cancellationToken)
        {
            var minOpenOrder = await _courseModuleRepository.ReadQueryable
                                                            .Where(x => x.CourseId == courseResult.CourseId)
                                                            .MinAsync(x => x.OpenOrder, cancellationToken);

            var courseModules = await GetCourseModulesAsync(courseResult.CourseId);
            courseModules = courseModules.Where(x => x.OpenOrder == minOpenOrder).ToList();
            if (courseModules == null || !courseModules.Any())
            {
                return;
            }
            foreach (var module in courseModules)
            {
                await UpdateNewResultCourseModule(module, courseResult, cancellationToken);
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

        private async Task UpdateNewResultCourseModule(CourseModule nextModule, CourseResult courseResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(nextModule);
            var initializer = _courseItemInitializerFactory.Get(nextModule.CourseConfigType);
            if (initializer != null)
            {
                await initializer.InitializeAsync(nextModule, courseResult, cancellationToken);
            }
            return;
        }

        public async Task<IList<CourseModule>> GetCourseModulesAsync(Guid id)
        {
            return await _courseModuleCachingService.GetOrSetAsync(id.ToString(), async (ctx, _) =>
            {
                var courseModules = await _courseModuleRepository.ReadQueryable
                                                             .Where(x => x.CourseId == id)
                                                             .ToListAsync(_);

                return courseModules.OrderBy(x => x.DisplayOrder).ToList();
            });
        }
    }
}
