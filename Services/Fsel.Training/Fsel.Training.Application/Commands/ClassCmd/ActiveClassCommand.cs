// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Training.Application.Services.SystemServices;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ActiveClassCommand : BaseCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class ActiveClassCommandHandler : IRequestHandler<ActiveClassCommand, MethodResult<bool>>
    {
        private readonly IClassRepository _classRepository;
        private readonly ISystemService _systemService;

        public ActiveClassCommandHandler(IClassRepository classRepository, ISystemService systemService)
        {
            _classRepository = classRepository;
            _systemService = systemService;
        }

        public async Task<MethodResult<bool>> Handle(ActiveClassCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var classes = await _classRepository.GetByIdAsync(request.Id);
            if (classes == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.ClassesNotExits));
                return methodResult;
            }
            if (classes.Status != EnumClassType.New)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.StatusOfClassIsNotNew));
                return methodResult;
            }
            List<Guid> courseIds = new List<Guid>();
            courseIds.Add(classes.CourseId);
            var courseTimeConfigResult = await _systemService.GetCourseTimeConfigByCourseId(courseIds);
            if (!courseTimeConfigResult.IsSuccessStatusCode)
            {
                methodResult.AddError(courseTimeConfigResult.Error);
                return methodResult;
            }
            var courseTimeConfig = courseTimeConfigResult.Content?.Result;
            var endTime = courseTimeConfig!.FirstOrDefault(p => p.CourseId == classes.Id);
            if (endTime == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.CourseTimeNotInstalled));
                return methodResult;
            }
            classes.StartDate = DateTime.Now;
            classes.EndDate = DateTime.Now.AddMonths(endTime.DurationMonth);
            classes.Status = EnumClassType.Active;

            if (classes.LiveTimeFrameId.HasValue && classes.LiveDays != null)
            {
                for (DateTime date = DateTime.Now; date <= classes.EndDate; date = date.AddDays(1))
                {
                    if (classes.LiveDays!.Contains(date.DayOfWeek))
                    {
                        ClassLiveCalendar classLiveCalendar = new ClassLiveCalendar()
                        {
                            LiveTimeFrameId = classes.LiveTimeFrameId ?? default,
                            LiveDate = date,
                            Status = EnumClassLiveCalendarStatus.NotStudied,
                            ClassId = classes.Id
                        };
                        classes.ClassLiveCalendars.Add(classLiveCalendar);
                    }
                }
            }

            await _classRepository.ExecuteTransactionAsync(async () =>
            {
                _classRepository.Update(classes);
                await _classRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
