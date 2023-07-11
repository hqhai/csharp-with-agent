// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Shared.Enums;
using Fsel.Common.Helpers;
using Fsel.Course.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Fsel.Common.Models;

namespace Fsel.Course.Application.Queries.CategoryQuery
{
    public class GetEnumQuery : IRequest<MethodResult<IList<EnumModel>>>
    {
        public EnumCourseSourceData? EnumCourseSourceData { get; set; }
    }

    public class GetEnumHandler : IRequestHandler<GetEnumQuery, MethodResult<IList<EnumModel>>>
    {
        public GetEnumHandler()
        {
        }

        public async Task<MethodResult<IList<EnumModel>>> Handle(GetEnumQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<EnumModel>> methodResult = new MethodResult<IList<EnumModel>>();

            switch (request.EnumCourseSourceData)
            {
                case EnumCourseSourceData.CourseType:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumCourseType>();
                    break;
                case EnumCourseSourceData.CourseLevel:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumCourseLevel>();
                    break;

                case EnumCourseSourceData.CourseSkill:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumCourseSkill>();
                    break;

                case EnumCourseSourceData.CourseStatus:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumCourseStatus>();
                    break;

                case EnumCourseSourceData.ExtraPracticeType:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumExtraPracticeType>();
                    break;

                case EnumCourseSourceData.QuestionType:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumQuestionType>();
                    break;

                case EnumCourseSourceData.TimeCodeType:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumTimeCodeType>();
                    break;

                case EnumCourseSourceData.PlacementTestLevel:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumPlacementTestLevel>();
                    break;

                case EnumCourseSourceData.FinalTestLevel:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumFinalTestLevel>();
                    break;
                case EnumCourseSourceData.SubscriptionClass:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumSubscriptionClass>();
                    break;
                case EnumCourseSourceData.CurrentStatus:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumCurrentStatus>();
                    break;
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
