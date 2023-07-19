// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.CategoryQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetEnumWorkFlowQuery : IRequest<MethodResult<IList<EnumModel>>>
    {
        public EnumWorkFlowType? WorkFlowType { get; set; }
    }

    public class GetEnumWorkFlowQueryHandler : IRequestHandler<GetEnumWorkFlowQuery, MethodResult<IList<EnumModel>>>
    {
        public GetEnumWorkFlowQueryHandler()
        {
        }

        public async Task<MethodResult<IList<EnumModel>>> Handle(GetEnumWorkFlowQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<EnumModel>> methodResult = new MethodResult<IList<EnumModel>>();
            switch (request.WorkFlowType)
            {
                case EnumWorkFlowType.ChangeTeacher:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumWorkFlowChangeTeacherStatus>();
                    break;

                case EnumWorkFlowType.AssignTeacher:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumWorkFlowAssignTeacherStatus>();
                    break;

                case EnumWorkFlowType.CancelSchedule:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumWorkFlowCancelScheduleStatus>();
                    break;

                default:
                    break;
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
