// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery.V1i2.Unit
{
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
    using Fsel.Course.Lms.Application.Services.ApplicationServices;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetUnitByUnitQuery : IRequest<MethodResult<IList<UnitModel>>>
    {
        public Guid CourseId { get; set; }
        public EnumLearnProcessType Type { get; set; }
    }

    public class GetUnitByUnitVideoQueryHandler : IRequestHandler<GetUnitByUnitQuery, MethodResult<IList<UnitModel>>>
    {
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;
        private readonly ICourseModuleCachingService _courseModuleCachingService;
        private readonly ICourseModuleRepository _courseModuleRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IProgressService _progressService;

        public GetUnitByUnitVideoQueryHandler(AuthContext authContext
            , IMapper mapper
            , ICourseRepository courseRepository
            , IUnitRepository unitRepository
            , IUserService userService
            , ICourseModuleCachingService courseModuleCachingService
            , ICourseModuleRepository courseModuleRepository
            , ICourseResultRepository courseResultRepository
            , IProgressService progressService
            )
        {
            _authContext = authContext;
            _mapper = mapper;
            _courseRepository = courseRepository;
            _unitRepository = unitRepository;
            _userService = userService;
            _courseModuleCachingService = courseModuleCachingService;
            _courseModuleRepository = courseModuleRepository;
            _courseResultRepository = courseResultRepository;
            _progressService = progressService;
        }

        public async Task<MethodResult<IList<UnitModel>>> Handle(GetUnitByUnitQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<UnitModel>>();
            var method = await GetStudentAsync();
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }

            var studentId = method.Result!.Id;

            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }

            methodResult.Result = await GetUnitsAsync(request, studentId, cancellationToken);
            return methodResult;
        }

        private async Task<MethodResult<StudentModel>> GetStudentAsync()
        {
            var methodResult = new MethodResult<StudentModel>();
            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            methodResult.Result = student;
            return methodResult;
        }

        private async Task<IList<UnitModel>?> GetUnitsAsync(GetUnitByUnitQuery request, Guid? studentId, CancellationToken cancellationToken)
        {
            var courseResult = await _courseResultRepository.ReadQueryable
                                    .Where(x => x.StudentId == studentId)
                                    .FirstOrDefaultAsync(x => x.CourseId == request.CourseId, cancellationToken) ?? new CourseResult();

            var courseModules = await GetCourseModulesAsync(request.CourseId);

            var (unitResultsByOriginalId, unitDics) = await _unitRepository.BuildUnitLookupsAsync(courseResult, courseModules);

            var units = courseModules.OrderBy(x => x.DisplayOrder)
                                     .Select(module => ProcessModule(module, unitDics, unitResultsByOriginalId))
                                     .Where(dto => dto != null)
                                     .Select(dto => dto!)
                                     .ToList();

            var unitIds = units.Select(x => x.Id).ToList();

            var list = new List<(Guid UnitId, double Percent)>();
            switch (request.Type)
            {
                case EnumLearnProcessType.LessonVideo:
                    list = await _progressService.GetUnitVideoPercentsAsync(courseResult.Id, unitIds, cancellationToken);
                    break;

                case EnumLearnProcessType.HomeWork:
                    list = await _progressService.GetUnitHomeWorkPercentsAsync(courseResult.Id, unitIds, cancellationToken);
                    break;

                case EnumLearnProcessType.ClassForum:
                    list = await _progressService.GetUnitClassForumPercentsAsync(courseResult.Id, unitIds, cancellationToken);
                    break;

                case EnumLearnProcessType.UnitTest:
                    list = await _progressService.GetUnitVideoUnitTestPercentsAsync(courseResult.Id, unitIds, cancellationToken);
                    break;

                default:
                    throw new NotImplementedException();
            }
            foreach (var item in units)
            {
                if (item.UnitResult != null)
                {
                    item.Percent = list.FirstOrDefault(x => x.UnitId == item.Id).Percent;
                    item.UnitResult.ProgressPercent = list.FirstOrDefault(x => x.UnitId == item.Id).Percent;
                }
            }

            return units;
        }

        private UnitModel? ProcessModule(
        CourseModule module,
        IDictionary<Guid, Domain.Entities.Unit> unitDics,
        IDictionary<Guid, (Domain.Entities.Unit, UnitResult)> unitResultsByOriginalId)
        {
            return module.CourseConfigType switch
            {
                EnumCourseConfigType.Unit when unitResultsByOriginalId.TryGetValue(module.OriginalId, out var unit) =>
                    CreateUnitModuleWithResult(module, unit),

                EnumCourseConfigType.Unit when unitDics.TryGetValue(module.OriginalId, out var unit) =>
                    CreateUnitModuleWithoutResult(module, unit),

                _ => null
            };
        }

        private UnitModel CreateUnitModuleWithResult(CourseModule module, (Domain.Entities.Unit Unit, UnitResult UnitResult) unit)
        {
            var dto = _mapper.Map<UnitModel>(unit.Unit);
            dto.DisplayOrder = module.DisplayNumber;
            dto.UnitResult = _mapper.Map<UnitResultModel>(unit.UnitResult);
            return dto;
        }

        private UnitModel CreateUnitModuleWithoutResult(CourseModule module, Domain.Entities.Unit unit)
        {
            var dto = _mapper.Map<UnitModel>(unit);
            dto.DisplayOrder = module.DisplayNumber;
            return dto;
        }

        private async Task<IList<CourseModule>> GetCourseModulesAsync(Guid id)
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
