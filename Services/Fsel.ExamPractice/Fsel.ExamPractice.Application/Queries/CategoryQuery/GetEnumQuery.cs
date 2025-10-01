// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Application.Queries.CategoryQuery
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetEnumQuery : IRequest<MethodResult<IList<EnumModel>>>
    {
        public Domain.Enums.EnumCourseSourceData? EnumCourseSourceData { get; set; }
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
                case Domain.Enums.EnumCourseSourceData.CourseSkill:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumCourseSkill>();
                    break;

                case Domain.Enums.EnumCourseSourceData.ExamPracticeStatus:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumExamPracticeStatus>();
                    break;

                case Domain.Enums.EnumCourseSourceData.ExamPracticeSubType:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumExamPracticeSubType>();
                    break;

                case Domain.Enums.EnumCourseSourceData.ExamPracticeType:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumExamPracticeType>().Where(x => x.Value == EnumExamPracticeType.Vstep.ToString()).ToList();
                    break;

                case Domain.Enums.EnumCourseSourceData.SectionExamPracticeType:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumSectionExamPracticeType>();
                    break;

                case Domain.Enums.EnumCourseSourceData.CourseLevel:
                    methodResult.Result = ConvertHelper.EnumToListModel<Domain.Enums.EnumCourseLevel>();
                    break;
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
