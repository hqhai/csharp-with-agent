// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.Training.Application.Services.SystemServices;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.CommandModels.Classes;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ChangeStatusClassCommand : ChangeStatusClassCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class ChangeStatusClassCommandHandler : IRequestHandler<ChangeStatusClassCommand, MethodResult<bool>>
    {
        private readonly IClassRepository _classRepository;
        private readonly ISystemService _systemService;
        private readonly IMapper _mapper;

        public ChangeStatusClassCommandHandler(IClassRepository classRepository, ISystemService systemService, IMapper mapper = null)
        {
            _classRepository = classRepository;
            _systemService = systemService;
            _mapper = mapper;
        }

        public async Task<MethodResult<bool>> Handle(ChangeStatusClassCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var classes = await _classRepository.GetByIdAsync(request.ClassId);
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
                methodResult.AddError(courseTimeConfigResult.Error?.Content, courseTimeConfigResult.StatusCode);
                return methodResult;
            }
            var courseTimeConfig = courseTimeConfigResult.Content?.Result;
            var endTime = courseTimeConfig!.FirstOrDefault(p => p.CourseId == classes.Id);
            if (endTime == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.CourseNotInstalled));
                return methodResult;
            }
            classes.LiveDays = request.LiveDays;
            classes.LiveTimeFrameId = request.LiveTimeFrameId;
            classes.StartTime = DateTime.Now;
            classes.EndTime = DateTime.Now.AddMonths(endTime.DurationMonth);
            for (DateTime date = DateTime.Now; date <= classes.EndTime; date = date.AddDays(1))
            {
                if (request.LiveDays!.Contains(date.DayOfWeek))
                {
                    ClassLiveCalendar classLiveCalendar = new ClassLiveCalendar()
                    {
                        LiveTimeFrameId = request.LiveTimeFrameId,
                        LiveDate = date,
                        Status = EnumClassLiveCalendarStatus.NotStudied,
                        ClassId = classes.Id
                    };
                    classes.ClassLiveCalendars.Add(classLiveCalendar);
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
