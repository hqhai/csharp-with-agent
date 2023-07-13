// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassCmd
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
        private readonly IClassRepository _classRepository;
        private readonly IClassLiveCalendarRepository _classLiveCalendarRepository;
        private readonly IClassLiveWorkFlowRepository _classLiveWorkFlowRepository;
        private readonly AuthContext _authContext;

        public ReplaceTeacherByCsoCommandHandler(IClassRepository classRepository,
                                                 IClassLiveCalendarRepository classLiveCalendarRepository,
                                                 IClassLiveWorkFlowRepository classLiveWorkFlowRepository,
                                                 AuthContext authContext)
        {
            _classRepository = classRepository;
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

                    lessonNeedToChange = new ClassLiveWorkFlow()
                    {
                        ClassLiveCalendarId = lessonNeedToChange.ClassLiveCalendarId,
                        Status = EnumWorkFlowChangeTeacherStatus.WaitConfirm.ToString(),
                        Type = EnumWorkFlowType.AssignTeacher,
                        Description = lessonNeedToChange.Description,
                        TeacherId = request.TeacherId,
                        CsoId = _authContext.CurrentUserId,
                        WorkFlowParentId = request.ClassLiveWorkId
                    };
                    _classLiveWorkFlowRepository.Add(lessonNeedToChange);


                }
                else if (lessonNeedToChange != null
                        && lessonNeedToChange.WorkFlowParentId != null
                        && lessonNeedToChange.Type == EnumWorkFlowType.ChangeTeacher)
                {
                    var changeTeacher = await _classLiveWorkFlowRepository.Queryable.Where(x => x.WorkFlowParentId == lessonNeedToChange.Id)
                                                                                    .FirstOrDefaultAsync(cancellationToken);

                    if (changeTeacher == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumClassLiveWorkFlowErrorCode.ClassLiveWorkFlowNotExits));
                        return methodResult;
                    }

                    changeTeacher.TeacherId = request?.TeacherId;
                    changeTeacher.Type = EnumWorkFlowType.AssignTeacher;
                    _classLiveWorkFlowRepository.Update(changeTeacher);
                }
                else
                {

                    methodResult.AddErrorBadRequest(nameof(EnumClassLiveWorkFlowErrorCode.ClassLiveWorkFlowNotExits));
                    return methodResult;
                }

                await _classRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
