// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.CourseGoalCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.Entities.CourseGoals;
    using Fsel.System.Domain.IRepositories.CourseGoals;
    using Fsel.System.Domain.Models.CommandModels.CourseGoals;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateCourseGoalCommand : UpdateCourseGoalCommandModel, IRequest<MethodResult<CourseGoalModel>>
    {
    }

    public class UpdateCourseGoalCommandHandler : IRequestHandler<UpdateCourseGoalCommand, MethodResult<CourseGoalModel>>
    {
        private readonly ICourseGoalRepository _courseGoalRepository;
        private readonly IMapper _mapper;

        public UpdateCourseGoalCommandHandler(ICourseGoalRepository courseGoalRepository,
                                              IMapper mapper)
        {
            _courseGoalRepository = courseGoalRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<CourseGoalModel>> Handle(UpdateCourseGoalCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CourseGoalModel> methodResult = new MethodResult<CourseGoalModel>();
            var courseGoal = await _courseGoalRepository.Queryable.Include(x => x.CourseGoalConfigs)
                                                        .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (courseGoal == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }
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
                if (!courseGoalConfig.IsValid())
                {
                    methodResult.AddErrorBadRequest(courseGoalConfig.ErrorMessages);
                    return methodResult;
                }
            }

            await _courseGoalRepository.ExecuteTransactionAsync(async () =>
            {
                _courseGoalRepository.Update(courseGoal);
                await _courseGoalRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.Result = _mapper.Map<CourseGoalModel>(courseGoal);
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });
            return methodResult;
        }
    }
}
