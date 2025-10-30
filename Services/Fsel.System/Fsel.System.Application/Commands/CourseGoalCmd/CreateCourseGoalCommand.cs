// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.CourseGoalCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities.CourseGoals;
    using Fsel.System.Domain.IRepositories.CourseGoals;
    using Fsel.System.Domain.Models.CommandModels.CourseGoals;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Linq;
    using global::System.Threading;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateCourseGoalCommand : CreateCourseGoalCommandModel, IRequest<MethodResult<IList<CourseGoalModel>>>
    {
    }

    public class CreateCourseGoalCommandHandler : IRequestHandler<CreateCourseGoalCommand, MethodResult<IList<CourseGoalModel>>>
    {
        private readonly ICourseGoalRepository _courseGoalRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;

        public CreateCourseGoalCommandHandler(ICourseGoalRepository courseGoalRepository,
                                              IMapper mapper,
                                              AuthContext authContext)
        {
            _courseGoalRepository = courseGoalRepository;
            _mapper = mapper;
            _authContext = authContext;
        }

        public async Task<MethodResult<IList<CourseGoalModel>>> Handle(CreateCourseGoalCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var isAdmin = _authContext.Roles != null && _authContext.Roles.Contains(EnumRole.Admin.ToString());
            var isRequiredAdmin = new List<EnumCourseGoalCategory> { EnumCourseGoalCategory.All, EnumCourseGoalCategory.All }.Contains(request.GoalCategory);
            if (!isAdmin && isRequiredAdmin)
            {
                var methodResult = new MethodResult<IList<CourseGoalModel>>();
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(_authContext.Roles), request.GoalCategory);
                return methodResult;
            }

            if (request.GoalCategory == EnumCourseGoalCategory.School)
            {
                return await AddsAsync(request, cancellationToken);
            }
            else
            {
                return await AddAsync(request, cancellationToken);
            }
        }

        private async Task<MethodResult<IList<CourseGoalModel>>> AddAsync(CreateCourseGoalCommand request, CancellationToken cancellationToken)
        {
            MethodResult<IList<CourseGoalModel>> methodResult = new MethodResult<IList<CourseGoalModel>>();
            var courseGoal = await _courseGoalRepository.Queryable.Include(x => x.CourseGoalConfigs)
                                                    .Where(x => x.CourseLevel == request.CourseLevel)
                                                    .Where(x => x.CourseType == request.CourseType)
                                                    .FirstOrDefaultAsync(x => x.GoalCategory == request.GoalCategory, cancellationToken);

            if (courseGoal == null)
            {
                courseGoal = _mapper.Map<CourseGoal>(request);
                courseGoal.Name = courseGoal.GoalCategory.GetDescription();
                if (!courseGoal.IsValid())
                {
                    methodResult.AddErrorBadRequest(courseGoal.ErrorMessages);
                    return methodResult;
                }
                foreach (var item in courseGoal.CourseGoalConfigs)
                {
                    var courseGoalConfig = _mapper.Map<CourseGoalConfig>(item);
                    if (!courseGoalConfig.IsValid())
                    {
                        methodResult.AddErrorBadRequest(courseGoalConfig.ErrorMessages);
                        return methodResult;
                    }
                    item.DisplayOrder = courseGoal.CourseGoalConfigs.ToList().IndexOf(item);
                }
                _courseGoalRepository.Add(courseGoal);
            }
            else
            {
                var courseGoalConfigs = courseGoal.CourseGoalConfigs
                                                  .ExceptBy(request.CourseGoalConfigs.Select(x => x.CourseId), x => x.CourseId)
                                                  .ToList();
                foreach (var courseGoalConfig in courseGoalConfigs)
                {
                    courseGoal.CourseGoalConfigs.Remove(courseGoalConfig);
                }

                foreach (var item in request.CourseGoalConfigs)
                {
                    var courseGoalConfig = courseGoal.CourseGoalConfigs.FirstOrDefault(x => x.CourseId == item.CourseId);
                    if (courseGoalConfig != null)
                    {
                        courseGoalConfig.LessonsPerWeek = item.LessonsPerWeek;
                    }
                    else
                    {
                        courseGoalConfig = _mapper.Map<CourseGoalConfig>(item);
                        courseGoal.CourseGoalConfigs.Add(courseGoalConfig);
                    }
                    courseGoalConfig.DisplayOrder = request.CourseGoalConfigs.IndexOf(item);
                    if (!courseGoalConfig.IsValid())
                    {
                        methodResult.AddErrorBadRequest(courseGoalConfig.ErrorMessages);
                        return methodResult;
                    }
                }

                _courseGoalRepository.Update(courseGoal);
            }

            await _courseGoalRepository.ExecuteTransactionAsync(async () =>
            {
                await _courseGoalRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.Result = _mapper.Map<IList<CourseGoalModel>>(new List<CourseGoal> { courseGoal });
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });
            if (request.GoalCategory == EnumCourseGoalCategory.All)
            {
                await UpdateAllAsync(request);
            }
            return methodResult;
        }

        private async Task<VoidMethodResult> UpdateAllAsync(CreateCourseGoalCommand request)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            var courseGoals = await _courseGoalRepository.Queryable.Include(x => x.CourseGoalConfigs)
                                                      .Where(x => x.CourseLevel == request.CourseLevel)
                                                      .Where(x => x.CourseType == request.CourseType)
                                                      .Where(x => x.GoalCategory != request.GoalCategory)
                                                      .ToListAsync();
            foreach (var courseGoal in courseGoals)
            {
                var courseGoalConfigs = courseGoal.CourseGoalConfigs
                                                  .ExceptBy(request.CourseGoalConfigs.Select(x => x.CourseId), x => x.CourseId)
                                                  .ToList();

                foreach (var courseGoalConfig in courseGoalConfigs)
                {
                    courseGoal.CourseGoalConfigs.Remove(courseGoalConfig);
                }

                foreach (var item in request.CourseGoalConfigs)
                {
                    var courseGoalConfig = courseGoal.CourseGoalConfigs.FirstOrDefault(x => x.CourseId == item.CourseId);
                    if (courseGoalConfig != null)
                    {
                        courseGoalConfig.LessonsPerWeek = item.LessonsPerWeek;
                    }
                    else
                    {
                        courseGoalConfig = _mapper.Map<CourseGoalConfig>(item);
                        courseGoal.CourseGoalConfigs.Add(courseGoalConfig);
                    }
                    courseGoalConfig.DisplayOrder = request.CourseGoalConfigs.IndexOf(item);
                }
            }
            await _courseGoalRepository.ExecuteTransactionAsync(async () =>
            {
                _courseGoalRepository.UpdateList(courseGoals);
                await _courseGoalRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

                return methodResult;
            });
            return methodResult;
        }

        private async Task<MethodResult<IList<CourseGoalModel>>> AddsAsync(CreateCourseGoalCommand request, CancellationToken cancellationToken)
        {
            MethodResult<IList<CourseGoalModel>> methodResult = new MethodResult<IList<CourseGoalModel>>();
            if (request.Classes == null || request.Classes.Count == 0)
            {
                return methodResult;
            }
            var existing = await _courseGoalRepository.Queryable.Include(x => x.CourseGoalConfigs).WhereBulkContains(request.Classes.Select(x => x.ClassId), x => x.ClassId)
                .Where(x => x.SchoolId == request.SchoolId)
                .ToListAsync(cancellationToken);

            var existingByClassId = existing.ToDictionary(x => x.ClassId ?? Guid.Empty, x => x);

            var toAdd = new List<CourseGoal>();
            var toUpdate = new List<CourseGoal>();

            foreach (var classModel in request.Classes)
            {
                if (existingByClassId.TryGetValue(classModel.ClassId, out var entity)) // UPDATE
                {
                    var courseGoalConfigs = entity.CourseGoalConfigs
                                                  .ExceptBy(request.CourseGoalConfigs.Select(x => x.CourseId), x => x.CourseId)
                                                  .ToList();

                    foreach (var courseGoalConfig in courseGoalConfigs)
                    {
                        entity.CourseGoalConfigs.Remove(courseGoalConfig);
                    }

                    foreach (var item in request.CourseGoalConfigs)
                    {
                        var courseGoalConfig = entity.CourseGoalConfigs.FirstOrDefault(x => x.CourseId == item.CourseId);
                        if (courseGoalConfig != null)
                        {
                            courseGoalConfig.LessonsPerWeek = item.LessonsPerWeek;
                        }
                        else
                        {
                            courseGoalConfig = _mapper.Map<CourseGoalConfig>(item);
                            entity.CourseGoalConfigs.Add(courseGoalConfig);
                        }
                        if (!courseGoalConfig.IsValid())
                        {
                            methodResult.AddErrorBadRequest(courseGoalConfig.ErrorMessages);
                            return methodResult;
                        }
                        courseGoalConfig.DisplayOrder = request.CourseGoalConfigs.IndexOf(item);
                    }

                    toUpdate.Add(entity);
                }
                else
                {
                    entity = _mapper.Map<CourseGoal>(request);
                    entity.ClassId = classModel.ClassId;
                    entity.ClassName = classModel.Name;
                    entity.Name = entity.ClassId.HasValue ? entity.ClassName : entity.GoalCategory.GetDescription();
                    if (!entity.IsValid())
                    {
                        methodResult.AddErrorBadRequest(entity.ErrorMessages);
                        return methodResult;
                    }

                    foreach (var item in entity.CourseGoalConfigs)
                    {
                        var courseGoalConfig = _mapper.Map<CourseGoalConfig>(item);
                        if (!courseGoalConfig.IsValid())
                        {
                            methodResult.AddErrorBadRequest(courseGoalConfig.ErrorMessages);
                            return methodResult;
                        }
                        item.DisplayOrder = entity.CourseGoalConfigs.ToList().IndexOf(item);
                    }
                    toAdd.Add(entity);
                }
            }

            await _courseGoalRepository.ExecuteTransactionAsync(async () =>
            {
                if (toAdd.Any())
                {
                    await _courseGoalRepository.AddList(toAdd);
                }
                if (toUpdate.Any())
                {
                    _courseGoalRepository.UpdateList(toUpdate);
                }
                await _courseGoalRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                toAdd.AddRange(toUpdate);
                methodResult.Result = _mapper.Map<IList<CourseGoalModel>>(toAdd);
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });
            return methodResult;
        }
    }
}
