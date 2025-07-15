// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Application.Queries.CategoryQuery
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
    using Fsel.ExamPractice.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using static Fsel.ExamPractice.Infrastructure.Common.EnumHelper;

    public class GetTagExamPracticeQuery : IRequest<MethodResult<IList<ExamPracticeTagModel>>>
    {
        public EnumExamPracticeType Type { get; set; }
    }

    public class GetTagExamPracticeQueryHandler : IRequestHandler<GetTagExamPracticeQuery, MethodResult<IList<ExamPracticeTagModel>>>
    {
        public GetTagExamPracticeQueryHandler()
        {
        }

        public async Task<MethodResult<IList<ExamPracticeTagModel>>> Handle(GetTagExamPracticeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<ExamPracticeTagModel>> methodResult = new MethodResult<IList<ExamPracticeTagModel>>();
            methodResult.Result = request.Type.GetTags();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
