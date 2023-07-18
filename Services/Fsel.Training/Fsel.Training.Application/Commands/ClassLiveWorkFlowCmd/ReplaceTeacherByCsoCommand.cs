// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassLiveWorkFlowCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.CommandModels.ClassLiveWorkFlows;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ReplaceTeacherByCsoCommand : AssignNewTeacherToLessonCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class ReplaceTeacherByCsoCommandHandler : IRequestHandler<ReplaceTeacherByCsoCommand, MethodResult<bool>>
    {
        private readonly IClassLiveCalendarRepository _classLiveCalendarRepository;
        private readonly IClassLiveWorkFlowRepository _classLiveWorkFlowRepository;
        private readonly AuthContext _authContext;

        public ReplaceTeacherByCsoCommandHandler(IClassLiveCalendarRepository classLiveCalendarRepository,
                                                 IClassLiveWorkFlowRepository classLiveWorkFlowRepository,
                                                 AuthContext authContext)
        {
            _classLiveCalendarRepository = classLiveCalendarRepository;
            _classLiveWorkFlowRepository = classLiveWorkFlowRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<bool>> Handle(ReplaceTeacherByCsoCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var lessonNeedToChange = await _classLiveWorkFlowRepository.GetByIdAsync(request.ClassLiveWorkId);

            await _classLiveCalendarRepository.ExecuteTransactionAsync(async () =>
            {
                if (lessonNeedToChange != null
                    && lessonNeedToChange.Type == EnumWorkFlowType.ChangeTeacher
                    && lessonNeedToChange.WorkFlowParentId == null)
                {
                    var lessonNeedToAssign = new ClassLiveWorkFlow()
                    {
                        ClassLiveCalendarId = lessonNeedToChange.ClassLiveCalendarId,
                        Status = EnumWorkFlowAssignTeacherStatus.Pending.ToString(),
                        Type = EnumWorkFlowType.AssignTeacher,
                        Description = lessonNeedToChange.Description,
                        TeacherId = request.TeacherId,
                        CsoId = _authContext.CurrentUserId,
                        WorkFlowParentId = request.ClassLiveWorkId
                    };
                    lessonNeedToChange.Status = EnumWorkFlowChangeTeacherStatus.WaitConfirm.ToString();

                    _classLiveWorkFlowRepository.Add(lessonNeedToAssign);
                    await _classLiveWorkFlowRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                    _classLiveWorkFlowRepository.Update(lessonNeedToChange);
                    await _classLiveWorkFlowRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
                else if (lessonNeedToChange != null
                        && lessonNeedToChange.WorkFlowParentId != null
                        && lessonNeedToChange.Type == EnumWorkFlowType.AssignTeacher)
                {
                    var assignTeacher = await _classLiveWorkFlowRepository.Queryable.Where(x => x.WorkFlowParentId == lessonNeedToChange.Id)
                                                                                    .FirstOrDefaultAsync(cancellationToken);
                    if (assignTeacher == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumClassLiveWorkFlowErrorCode.ClassLiveWorkFlowNotExits));
                        return methodResult;
                    }

                    assignTeacher.TeacherId = request?.TeacherId;
                    _classLiveWorkFlowRepository.Update(assignTeacher);
                    await _classLiveWorkFlowRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    methodResult.AddErrorBadRequest(nameof(EnumClassLiveWorkFlowErrorCode.ClassLiveWorkFlowNotExits));
                    return methodResult;
                }

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
