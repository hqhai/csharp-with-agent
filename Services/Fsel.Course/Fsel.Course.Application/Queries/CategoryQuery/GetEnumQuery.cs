// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Common.Enums;
using Fsel.Common.Helpers;
using Fsel.Course.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Course.Application.Queries.CategoryQuery
{
    public class GetEnumQuery : IRequest<MethodResult<IList<string>>>
    {
        public EnumCourseSourceData? EnumCourseSourceData { get; set; }
    }

    public class GetEnumHandler : IRequestHandler<GetEnumQuery, MethodResult<IList<string>>>
    {
        public GetEnumHandler()
        {
        }

        public async Task<MethodResult<IList<string>>> Handle(GetEnumQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<string>> methodResult = new MethodResult<IList<string>>();

            switch (request.EnumCourseSourceData)
            {
                case EnumCourseSourceData.CourseType:
                    methodResult.Result = ConvertHelper.EnumToListStr<EnumCourseType>();
                    break;

                case EnumCourseSourceData.CourseSkill:
                    methodResult.Result = ConvertHelper.EnumToListStr<EnumCourseSkill>();
                    break;

                case EnumCourseSourceData.CourseStatus:
                    methodResult.Result = ConvertHelper.EnumToListStr<EnumCourseStatus>();
                    break;

                case EnumCourseSourceData.ExtraPracticeType:
                    methodResult.Result = ConvertHelper.EnumToListStr<EnumExtraPracticeType>();
                    break;

                case EnumCourseSourceData.QuestionType:
                    methodResult.Result = ConvertHelper.EnumToListStr<EnumQuestionType>();
                    break;

                case EnumCourseSourceData.TimeCodeType:
                    methodResult.Result = ConvertHelper.EnumToListStr<EnumTimeCodeType>();
                    break;
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
